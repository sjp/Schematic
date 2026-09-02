using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A database routine provider for PostgreSQL databases.
/// </summary>
/// <seealso cref="IDatabaseRoutineProvider" />
public class PostgreSqlDatabaseRoutineProvider : IDatabaseRoutineProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseRoutineProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public PostgreSqlDatabaseRoutineProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
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
    /// Gets an identifier resolver that enables more relaxed matching against database object names.
    /// </summary>
    /// <value>An identifier resolver.</value>
    protected IIdentifierResolutionStrategy IdentifierResolver { get; }

    /// <summary>
    /// A type provider used to describe routine parameter and return types.
    /// </summary>
    /// <value>A database column type provider.</value>
    protected IDbTypeProvider TypeProvider { get; } = new PostgreSqlDbTypeProvider();

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
    protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(routineName)
            .Select(QualifyRoutineName);

        return resolvedNames
            .Select(name => GetResolvedRoutineNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the routine without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="routineName">A routine name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A routine name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedRoutineNameStrict(Identifier routineName, CancellationToken cancellationToken)
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
        var overloadRows = await LoadOverloadRowsAsync(routineName, cancellationToken);
        var parameterRows = await Connection.QueryAsync(
            GetRoutineParameters.Sql,
            new GetRoutineParameters.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        var parametersByRoutine = parameterRows
            .GroupBy(static row => row.RoutineOid)
            .ToDictionary(
                static rows => rows.Key,
                rows => rows.OrderBy(static row => row.Ordinal).Select(BuildParameter).ToList()
            );

        var overloads = overloadRows
            .Select(row => new DatabaseRoutineOverload(
                row.Definition!,
                parametersByRoutine.TryGetValue(row.RoutineOid, out var parameters)
                    ? parameters
                    : [],
                GetReturnType(row)
            ))
            .ToList();

        // the routine's name has already been resolved against pg_proc, so a routine with no
        // renderable definition means the catalog could not describe one of its overloads
        if (overloads.Count == 0)
            throw new SchematicException($"Unable to retrieve a definition for the routine '{routineName}'.");

        var firstOverload = overloads[0];
        var language = overloadRows[0].Language;
        var definition = overloads.Count == 1
            ? firstOverload.Definition
            : overloads.Select(static o => o.Definition).Join(OverloadDefinitionSeparator);

        return new DatabaseRoutine(
            routineName,
            definition,
            GetRoutineType(overloadRows[0].RoutineKind),
            !language.IsNullOrWhiteSpace() ? Option<string>.Some(language) : Option<string>.None,
            firstOverload.Parameters,
            firstOverload.ReturnType,
            // a name that carries a single signature is not overloaded, so it reports no overloads
            overloads.Count > 1 ? overloads : []
        );
    }

    private IDatabaseRoutineParameter BuildParameter(GetRoutineParameters.Result row)
    {
        var parameterName = !row.ParameterName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.ParameterName))
            : Option<Identifier>.None;

        return new DatabaseRoutineParameter(
            parameterName,
            CreateArgumentType(row.TypeSchema, row.TypeName),
            GetParameterDirection(row.ParameterMode),
            !row.DefaultValue.IsNullOrWhiteSpace() ? Option<string>.Some(row.DefaultValue) : Option<string>.None,
            row.Ordinal
        );
    }

    private Option<IDbType> GetReturnType(GetRoutineDefinition.Result row)
    {
        // a procedure returns nothing, and PostgreSQL records that as the pseudo-type 'void'
        if (row.ReturnTypeName.IsNullOrWhiteSpace() || string.Equals(row.ReturnTypeName, Constants.VoidTypeName, StringComparison.Ordinal))
            return Option<IDbType>.None;

        return Option<IDbType>.Some(CreateArgumentType(row.ReturnTypeSchema, row.ReturnTypeName));
    }

    private IDbType CreateArgumentType(string? typeSchema, string typeName)
    {
        // PostgreSQL discards length and precision modifiers on an argument or return type, so
        // there is no max length or numeric precision to report for one
        var typeMetadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(typeSchema, typeName),
            Collation = Option<Identifier>.None,
            MaxLength = 0,
            NumericPrecision = Option<INumericPrecision>.None,
        };

        return TypeProvider.CreateColumnType(typeMetadata);
    }

    private static RoutineParameterDirection GetParameterDirection(string parameterMode) => parameterMode switch
    {
        Constants.OutParameterMode => RoutineParameterDirection.Output,
        Constants.InOutParameterMode => RoutineParameterDirection.InputOutput,
        _ => RoutineParameterDirection.Input,
    };

    // a window function is still a function, and Schematic does not model windowing separately
    private static RoutineType GetRoutineType(string routineKind) => routineKind switch
    {
        Constants.ProcedureKind => RoutineType.Procedure,
        Constants.FunctionKind or Constants.WindowFunctionKind => RoutineType.Function,
        Constants.AggregateKind => RoutineType.Aggregate,
        _ => RoutineType.Unknown,
    };

    /// <summary>
    /// Retrieves the definition of a routine. When several overloads share the routine's name,
    /// their definitions are returned in one string, separated by blank lines.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a routine.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected async Task<string?> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var overloadRows = await LoadOverloadRowsAsync(routineName, cancellationToken);
        return overloadRows.Count == 0
            ? null
            : overloadRows.Select(static row => row.Definition!).Join(OverloadDefinitionSeparator);
    }

    private async Task<IReadOnlyList<GetRoutineDefinition.Result>> LoadOverloadRowsAsync(Identifier routineName, CancellationToken cancellationToken)
    {
        var results = await Connection.QueryAsync(
            GetRoutineDefinition.Sql,
            new GetRoutineDefinition.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        // an overload whose definition the catalog cannot render is dropped rather than reported
        // with an empty body, matching what string_agg() used to do with a null definition
        return results
            .Where(static row => !row.Definition.IsNullOrWhiteSpace())
            .ToList();
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

    private const string OverloadDefinitionSeparator = "\n\n";

    private static class Constants
    {
        public const string AggregateKind = "a";

        public const string FunctionKind = "f";

        public const string InOutParameterMode = "b";

        public const string OutParameterMode = "o";

        public const string ProcedureKind = "p";

        public const string VoidTypeName = "void";

        public const string WindowFunctionKind = "w";
    }
}