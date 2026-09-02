using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Queries;

namespace SJP.Schematic.MySql;

/// <summary>
/// A MySQL database routine provider.
/// </summary>
/// <seealso cref="IDatabaseRoutineProvider" />
public class MySqlDatabaseRoutineProvider : IDatabaseRoutineProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseRoutineProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public MySqlDatabaseRoutineProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection that is specific to a given MySQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// A database connection factory to query the database.
    /// </summary>
    /// <value>A connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Enumerates all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines(CancellationToken cancellationToken = default)
    {
        return DbConnection.QueryEnumerableAsync(
                GetAllRoutineNames.Sql,
                new GetAllRoutineNames.Query { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            )
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
            .Select(QualifyRoutineName)
            .SelectAwait(LoadRoutineAsyncCore);
    }

    /// <summary>
    /// Gets all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default)
    {
        var routineNames = await DbConnection.QueryEnumerableAsync(
                GetAllRoutineNames.Sql,
                new GetAllRoutineNames.Query { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            )
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
            .Select(QualifyRoutineName)
            .ToListAsync(cancellationToken);

        return await routineNames
            .Select(routineName => LoadRoutineAsyncCore(routineName, cancellationToken))
            .ToArray()
            .WhenAll();
    }

    /// <summary>
    /// Retrieves a database routine, if available.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var candidateRoutineName = QualifyRoutineName(routineName);
        return LoadRoutine(candidateRoutineName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the routine. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="routineName">A routine name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A routine name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var candidateRoutineName = QualifyRoutineName(routineName);
        var qualifiedRoutineName = DbConnection.QueryFirstOrNone(
            GetRoutineName.Sql,
            new GetRoutineName.Query { SchemaName = candidateRoutineName.Schema!, RoutineName = candidateRoutineName.LocalName },
            cancellationToken
        );

        return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(candidateRoutineName.Server, candidateRoutineName.Database, name.SchemaName, name.RoutineName));
    }

    /// <summary>
    /// Retrieves a routine from the database, if available.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseRoutine> LoadRoutine(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var candidateRoutineName = QualifyRoutineName(routineName);
        return GetResolvedRoutineName(candidateRoutineName, cancellationToken)
            .MapAsync(name => LoadRoutineAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseRoutine> LoadRoutineAsyncCore(Identifier routineName, CancellationToken cancellationToken)
    {
        var routineDetail = await LoadRoutineDetailAsync(routineName, cancellationToken);
        var parameterRows = await DbConnection.QueryAsync(
            GetRoutineParameters.Sql,
            new GetRoutineParameters.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        // MySQL stores a function's return value alongside its parameters, at ordinal position zero
        var returnType = parameterRows
            .Where(static row => row.Ordinal == 0)
            .Select(GetParameterType)
            .HeadOrNone();
        var parameters = parameterRows
            .Where(static row => row.Ordinal > 0)
            .OrderBy(static row => row.Ordinal)
            .Select(BuildParameter)
            .ToList();

        return new DatabaseRoutine(
            routineName,
            routineDetail!.Definition,
            GetRoutineType(routineDetail.RoutineType),
            !routineDetail.Language.IsNullOrWhiteSpace() ? Option<string>.Some(routineDetail.Language) : Option<string>.None,
            parameters,
            returnType
        );
    }

    private IDatabaseRoutineParameter BuildParameter(GetRoutineParameters.Result row)
    {
        var parameterName = !row.ParameterName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.ParameterName))
            : Option<Identifier>.None;

        return new DatabaseRoutineParameter(
            parameterName,
            GetParameterType(row),
            GetParameterDirection(row.ParameterMode),
            // MySQL routine parameters cannot declare a default
            Option<string>.None,
            row.Ordinal
        );
    }

    private IDbType GetParameterType(GetRoutineParameters.Result row)
    {
        var typeMetadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
            Collation = !row.Collation.IsNullOrWhiteSpace()
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                : Option<Identifier>.None,
            MaxLength = row.CharacterMaxLength,
            NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
        };

        return Dialect.TypeProvider.CreateColumnType(typeMetadata);
    }

    private static RoutineParameterDirection GetParameterDirection(string? parameterMode)
    {
        if (string.Equals(parameterMode, Constants.Out, StringComparison.OrdinalIgnoreCase))
            return RoutineParameterDirection.Output;
        if (string.Equals(parameterMode, Constants.InOut, StringComparison.OrdinalIgnoreCase))
            return RoutineParameterDirection.InputOutput;

        return RoutineParameterDirection.Input;
    }

    private static RoutineType GetRoutineType(string routineType)
    {
        if (string.Equals(routineType, Constants.Procedure, StringComparison.OrdinalIgnoreCase))
            return RoutineType.Procedure;
        if (string.Equals(routineType, Constants.Function, StringComparison.OrdinalIgnoreCase))
            return RoutineType.Function;

        return RoutineType.Unknown;
    }

    /// <summary>
    /// Retrieves the definition of the routine from the database.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A routine definition.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected async Task<string?> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var routineDetail = await LoadRoutineDetailAsync(routineName, cancellationToken);
        return routineDetail?.Definition;
    }

    private async Task<GetRoutineDefinition.Result?> LoadRoutineDetailAsync(Identifier routineName, CancellationToken cancellationToken)
    {
        var results = await DbConnection.QueryAsync(
            GetRoutineDefinition.Sql,
            new GetRoutineDefinition.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        return results.FirstOrDefault();
    }

    /// <summary>
    /// Qualifies the name of a routine, using known identifier defaults.
    /// </summary>
    /// <param name="routineName">A routine name to qualify.</param>
    /// <returns>A routine name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyRoutineName(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var schema = routineName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, routineName.LocalName);
    }

    private static class Constants
    {
        public const string Function = "FUNCTION";

        public const string InOut = "INOUT";

        public const string Out = "OUT";

        public const string Procedure = "PROCEDURE";
    }
}