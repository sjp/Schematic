using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
/// <remarks>
/// MySQL lists a routine to any user with a privilege on it, such as <c>EXECUTE</c>, but only shows its body to the
/// routine's definer and to users allowed to read routine bodies. Such a routine is still returned, with a SQL comment
/// in place of its definition.
/// <para>
/// A schema may hold a function and a procedure that share a name. Only the function is returned for that name.
/// </para>
/// </remarks>
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
    public async IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var routineNames = await GetAllRoutineNamesAsync(cancellationToken);

        var results = routineNames.SelectOrderedPrefetchAsync(LoadRoutineAsyncCore, Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);

        await foreach (var result in results.WithCancellation(cancellationToken))
            yield return result;
    }

    /// <summary>
    /// Gets all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default)
    {
        var routineNames = await GetAllRoutineNamesAsync(cancellationToken);

        return await routineNames.SelectBoundedAsync(LoadRoutineAsyncCore, Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);
    }

    private async Task<List<Identifier>> GetAllRoutineNamesAsync(CancellationToken cancellationToken)
    {
        var rows = await DbConnection.QueryAsync(
            GetAllRoutineNames.Sql,
            new GetAllRoutineNames.Query { SchemaName = IdentifierDefaults.Schema! },
            cancellationToken
        );

        return MySqlRoutineSelection.OnePerName(rows, static row => row.RoutineName, static row => row.RoutineType)
            .OrderBy(static row => row.SchemaName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static row => row.RoutineName, StringComparer.OrdinalIgnoreCase)
            .Select(static row => Identifier.CreateQualifiedIdentifier(row.SchemaName, row.RoutineName))
            .Select(QualifyRoutineName)
            .ToList();
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
        var qualifiedRoutineName = DbConnection.QueryAsync(
                GetRoutineName.Sql,
                new GetRoutineName.Query { SchemaName = candidateRoutineName.Schema!, RoutineName = candidateRoutineName.LocalName },
                cancellationToken
            )
            .Map(rows => MySqlRoutineSelection.ForName(rows, candidateRoutineName.LocalName, static row => row.RoutineName, static row => row.RoutineType))
            .ToAsync();

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
        var rows = await DbConnection.QueryAsync(
            GetRoutineDefinition.Sql,
            new GetRoutineDefinition.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        // definition rows have no ordinal, see the query's comment
        var routineDetail = MySqlRoutineSelection.ForName(
            rows.Where(static row => row.Ordinal == null),
            routineName.LocalName,
            static row => row.RoutineName,
            static row => row.RoutineType
        );

        // the routine was dropped after its name was read
        if (routineDetail.IsNone)
            return new DatabaseRoutine(routineName, UnavailableDefinition, RoutineType.Unknown, Option<string>.None, [], Option<IDbType>.None);

        var detail = routineDetail.Single();

        // keep only the parameters of the routine whose definition was picked
        var parameterRows = rows
            .Where(row => row.Ordinal != null
                && string.Equals(row.RoutineName, detail.RoutineName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.RoutineType, detail.RoutineType, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // MySQL stores a function's return value alongside its parameters, at ordinal position zero
        var returnType = parameterRows
            .Where(static row => row.Ordinal == 0)
            .Select(GetParameterType)
            .HeadOrNone();
        // the query is unordered, see its comment
        var parameters = parameterRows
            .Where(static row => row.Ordinal > 0)
            .OrderBy(static row => row.Ordinal)
            .Select(BuildParameter)
            .ToList();

        return new DatabaseRoutine(
            routineName,
            !detail.Definition.IsNullOrWhiteSpace() ? detail.Definition : UnavailableDefinition,
            GetRoutineType(detail.RoutineType),
            !detail.Language.IsNullOrWhiteSpace() ? Option<string>.Some(detail.Language) : Option<string>.None,
            parameters,
            returnType
        );
    }

    private IDatabaseRoutineParameter BuildParameter(GetRoutineDefinition.Result row)
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
            row.Ordinal!.Value
        );
    }

    private IDbType GetParameterType(GetRoutineDefinition.Result row)
    {
        var typeMetadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName!),
            Collation = !row.Collation.IsNullOrWhiteSpace()
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                : Option<Identifier>.None,
            MaxLength = MySqlColumnTypeMetadata.ClampMaxLength(row.CharacterMaxLength ?? 0),
            NumericPrecision = new NumericPrecision(row.Precision ?? 0, row.Scale ?? 0),
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

    // information_schema.routines.routine_definition is null unless the user defined the routine or
    // may read routine bodies (SHOW_ROUTINE on MySQL, SHOW CREATE ROUTINE on MariaDB 11.3+)
    private const string UnavailableDefinition = "-- definition unavailable (requires the privilege to read routine bodies)";

    private static class Constants
    {
        public const string Function = "FUNCTION";

        public const string InOut = "INOUT";

        public const string Out = "OUT";

        public const string Procedure = "PROCEDURE";
    }
}