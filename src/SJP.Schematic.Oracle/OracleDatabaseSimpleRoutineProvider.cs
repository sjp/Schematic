using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A routine provider for Oracle databases that access routines (but not packages).
/// </summary>
/// <seealso cref="IDatabaseRoutineProvider" />
public class OracleDatabaseSimpleRoutineProvider : IDatabaseRoutineProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseSimpleRoutineProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public OracleDatabaseSimpleRoutineProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
    /// A type provider used to describe routine argument and return types.
    /// </summary>
    /// <value>A database column type provider.</value>
    protected IDbTypeProvider TypeProvider { get; } = new OracleDbTypeProvider();

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
        var definition = await LoadDefinitionAsync(routineName, cancellationToken);
        var signatureRows = await Connection.QueryAsync(
            GetRoutineSignature.Sql,
            new GetRoutineSignature.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );
        var signature = signatureRows.ToList();

        // a function's return value is argument position zero; a routine with no arguments at all
        // still produces one row, carrying only the routine's type
        var returnType = signature
            .Where(static row => row.Position == 0)
            .Select(GetArgumentType)
            .HeadOrNone();
        var parameters = signature
            .Where(static row => row.Position > 0)
            .OrderBy(static row => row.Position)
            .Select(BuildParameter)
            .ToList();

        return new DatabaseRoutine(
            routineName,
            definition,
            signature.Count > 0 ? GetRoutineType(signature[0].RoutineType) : RoutineType.Unknown,
            OracleRoutineLanguage,
            parameters,
            returnType
        );
    }

    private IDatabaseRoutineParameter BuildParameter(GetRoutineSignature.Result row)
    {
        var parameterName = !row.ArgumentName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.ArgumentName))
            : Option<Identifier>.None;

        return new DatabaseRoutineParameter(
            parameterName,
            GetArgumentType(row),
            GetParameterDirection(row.InOut),
            // ALL_ARGUMENTS.DEFAULT_VALUE is a LONG, which cannot be selected with the rest of the
            // row, so a defaulted argument is reported without the default's text
            Option<string>.None,
            row.Position!.Value
        );
    }

    private IDbType GetArgumentType(GetRoutineSignature.Result row)
    {
        var typeMetadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(row.ArgumentTypeSchema, row.ArgumentTypeName),
            // ALL_ARGUMENTS records a character set, not a collation
            Collation = Option<Identifier>.None,
            MaxLength = row.DataLength,
            NumericPrecision = row.Precision > 0 || row.Scale > 0
                ? Option<INumericPrecision>.Some(new NumericPrecision(row.Precision, row.Scale))
                : Option<INumericPrecision>.None,
        };

        return TypeProvider.CreateColumnType(typeMetadata);
    }

    private static RoutineParameterDirection GetParameterDirection(string? inOut)
    {
        if (string.Equals(inOut, Constants.Out, StringComparison.OrdinalIgnoreCase))
            return RoutineParameterDirection.Output;
        if (string.Equals(inOut, Constants.InOut, StringComparison.OrdinalIgnoreCase))
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
    /// Retrieves the definition of a routine.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a routine.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected Task<string> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return LoadDefinitionAsyncCore(routineName, cancellationToken);
    }

    private async Task<string> LoadDefinitionAsyncCore(Identifier routineName, CancellationToken cancellationToken)
    {
        // fast path
        if (string.Equals(routineName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal))
            return await LoadUserDefinitionAsyncCore(routineName, cancellationToken);

        var lines = await Connection.QueryAsync(
            GetRoutineDefinition.Sql,
            new GetRoutineDefinition.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        if (lines.Empty())
            return string.Empty;

        var definition = lines.Join(string.Empty);
        return OracleUnwrapper.Unwrap(definition);
    }

    private async Task<string> LoadUserDefinitionAsyncCore(Identifier routineName, CancellationToken cancellationToken)
    {
        var userLines = await Connection.QueryAsync(
            GetUserRoutineDefinition.Sql,
            new GetUserRoutineDefinition.Query { RoutineName = routineName.LocalName },
            cancellationToken
        );

        if (userLines.Empty())
            return string.Empty;

        var userDefinition = userLines.Join(string.Empty);
        return OracleUnwrapper.Unwrap(userDefinition);
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

    // this provider only exposes objects whose source is stored in ALL_SOURCE as a FUNCTION or a
    // PROCEDURE, and Oracle only holds PL/SQL there - a Java routine reaches SQL through a PL/SQL
    // call spec, which is what is read here
    private static readonly Option<string> OracleRoutineLanguage = Option<string>.Some("PL/SQL");

    private static class Constants
    {
        public const string Function = "FUNCTION";

        public const string InOut = "IN/OUT";

        public const string Out = "OUT";

        public const string Procedure = "PROCEDURE";
    }
}