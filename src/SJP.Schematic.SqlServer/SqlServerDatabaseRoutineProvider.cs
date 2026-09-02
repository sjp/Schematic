using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A comment provider for SQL Server database routines.
/// </summary>
/// <seealso cref="IDatabaseRoutineProvider" />
public class SqlServerDatabaseRoutineProvider : IDatabaseRoutineProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseRoutineProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public SqlServerDatabaseRoutineProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A type provider used to describe routine parameter and return types.
    /// </summary>
    /// <value>A database column type provider.</value>
    protected IDbTypeProvider TypeProvider { get; } = new SqlServerDbTypeProvider();

    /// <summary>
    /// Enumerates all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllRoutineNames.Result>(GetAllRoutineNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
            .Select(QualifyRoutineName)
            .SelectAwait(LoadRoutineAsyncCore);
    }

    /// <summary>
    /// Retrieves all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default)
    {
        var routineNames = await Connection.QueryEnumerableAsync<GetAllRoutineNames.Result>(GetAllRoutineNames.Sql, cancellationToken)
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
        var qualifiedRoutineName = Connection.QueryFirstOrNone(
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
        var parameterRows = await Connection.QueryAsync(
            GetRoutineParameters.Sql,
            new GetRoutineParameters.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        // parameter_id 0 is a scalar function's return value rather than a parameter. A table-valued
        // function has no such row, so its return type is unavailable rather than absent.
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
            GetRoutineType(routineDetail.RoutineTypeCode),
            SqlServerRoutineLanguage,
            parameters,
            returnType
        );
    }

    private IDatabaseRoutineParameter BuildParameter(GetRoutineParameters.Result row)
    {
        // sys.parameters names a parameter with its leading '@', which is part of the name in T-SQL
        var parameterName = !row.ParameterName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.ParameterName))
            : Option<Identifier>.None;

        // an OUTPUT parameter in T-SQL is also readable inside the routine, so it is an in/out parameter
        var direction = row.IsOutput
            ? RoutineParameterDirection.InputOutput
            : RoutineParameterDirection.Input;

        return new DatabaseRoutineParameter(
            parameterName,
            GetParameterType(row),
            direction,
            !row.DefaultValue.IsNullOrWhiteSpace() ? Option<string>.Some(row.DefaultValue) : Option<string>.None,
            row.Ordinal
        );
    }

    private IDbType GetParameterType(GetRoutineParameters.Result row)
    {
        var typeMetadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
            // sys.parameters records no collation, unlike sys.columns
            Collation = Option<Identifier>.None,
            MaxLength = row.MaxLength,
            NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
        };

        return TypeProvider.CreateColumnType(typeMetadata);
    }

    private static RoutineType GetRoutineType(string routineTypeCode) => routineTypeCode switch
    {
        Constants.ProcedureTypeCode => RoutineType.Procedure,
        Constants.ScalarFunctionTypeCode or Constants.InlineTableFunctionTypeCode or Constants.TableFunctionTypeCode => RoutineType.Function,
        _ => RoutineType.Unknown,
    };

    /// <summary>
    /// Retrieves the definition of a routine.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a routine.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected async Task<string?> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var routineDetail = await LoadRoutineDetailAsync(routineName, cancellationToken);
        return routineDetail?.Definition;
    }

    private async Task<GetRoutineDefinition.Result?> LoadRoutineDetailAsync(Identifier routineName, CancellationToken cancellationToken)
    {
        var results = await Connection.QueryAsync(
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

    // every routine this provider exposes is read from sys.sql_modules, which only holds T-SQL;
    // CLR routines live in sys.assembly_modules and are filtered out by the object type predicate
    private static readonly Option<string> SqlServerRoutineLanguage = Option<string>.Some("SQL");

    private static class Constants
    {
        public const string ProcedureTypeCode = "P";

        public const string ScalarFunctionTypeCode = "FN";

        public const string InlineTableFunctionTypeCode = "IF";

        public const string TableFunctionTypeCode = "TF";
    }
}