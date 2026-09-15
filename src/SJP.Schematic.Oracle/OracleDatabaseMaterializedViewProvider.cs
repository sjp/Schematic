using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A materialized view provider for Oracle.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class OracleDatabaseMaterializedViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseMaterializedViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public OracleDatabaseMaterializedViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
    }

    /// <summary>
    /// A database connection that is specific to a given Oracle database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

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
    /// A database connection factory used to query the database.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Enumerates all materialized views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of materialized views.</returns>
    public async IAsyncEnumerable<IDatabaseView> EnumerateAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var viewNames = await GetAllViewNamesAsync(cancellationToken);

        var results = viewNames.SelectOrderedPrefetchAsync(LoadViewAsyncCore, Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);

        await foreach (var result in results.WithCancellation(cancellationToken))
            yield return result;
    }

    /// <summary>
    /// Gets all materialized views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of materialized views.</returns>
    public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default)
    {
        var viewNames = await GetAllViewNamesAsync(cancellationToken);

        return await viewNames.SelectBoundedAsync(LoadViewAsyncCore, Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);
    }

    internal ValueTask<List<Identifier>> GetAllViewNamesAsync(CancellationToken cancellationToken)
    {
        return DbConnection.QueryEnumerableAsync<GetAllMaterializedViewNames.Result>(GetAllMaterializedViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        return LoadView(candidateViewName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the materialized view. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(viewName)
            .Select(QualifyViewName);

        return resolvedNames
            .Select(name => GetResolvedViewNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the materialized view without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="viewName">A materialized view name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        var qualifiedViewName = DbConnection.QueryFirstOrNone(
            GetMaterializedViewName.Sql,
            new GetMaterializedViewName.Query { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
            cancellationToken
        );

        return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ViewName));
    }

    /// <summary>
    /// Retrieves a database view, if available.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view definition, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        return GetResolvedViewName(candidateViewName, cancellationToken)
            .MapAsync(name => LoadViewAsyncCore(name, cancellationToken));
    }

    internal async Task<IDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var (columns, definitionAndOptions, triggers, indexRows) = await (
            LoadColumnsAsync(viewName, cancellationToken),
            LoadDefinitionAndOptionsAsync(viewName, cancellationToken),
            LoadTriggersAsync(viewName, cancellationToken),
            LoadIndexRowsAsync(viewName, cancellationToken)
        ).WhenAll();

        var columnLookup = GetColumnLookup(columns);
        var indexes = OracleCatalogMapper.MapIndexes(indexRows, columnLookup, Dialect);

        return new DatabaseMaterializedView(
            viewName,
            definitionAndOptions.Definition!,
            columns,
            triggers,
            indexes,
            definitionAndOptions.RefreshMode,
            definitionAndOptions.RefreshMethod,
            definitionAndOptions.IsPopulated
        );
    }

    /// <summary>
    /// Retrieves the triggers defined on a materialized view, i.e. the DML triggers on its container table.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of triggers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadTriggersAsyncCore(viewName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetMaterializedViewTriggers.Sql,
            new GetMaterializedViewTriggers.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );

        return OracleCatalogMapper.MapTriggers(queryResult);
    }

    private Task<IEnumerable<GetTableIndexes.Result>> LoadIndexRowsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        return DbConnection.QueryAsync(
            GetMaterializedViewIndexes.Sql,
            new GetMaterializedViewIndexes.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// Retrieves the refresh metadata of a materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>When and how the view is refreshed, and whether it currently holds data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<(MaterializedViewRefreshMode RefreshMode, Option<string> RefreshMethod, bool IsPopulated)> LoadOptionsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadOptionsAsyncCore(viewName, cancellationToken);
    }

    private async Task<(MaterializedViewRefreshMode RefreshMode, Option<string> RefreshMethod, bool IsPopulated)> LoadOptionsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var (_, refreshMode, refreshMethod, isPopulated) = await LoadDefinitionAndOptionsAsync(viewName, cancellationToken);
        return (refreshMode, refreshMethod, isPopulated);
    }

    // The definition and the refresh metadata come from the same SYS.ALL_MVIEWS row, so they are read together.
    private async Task<(string? Definition, MaterializedViewRefreshMode RefreshMode, Option<string> RefreshMethod, bool IsPopulated)> LoadDefinitionAndOptionsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        var result = await DbConnection.QueryFirstOrNone(
            GetMaterializedViewDefinition.Sql,
            new GetMaterializedViewDefinition.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        ).ToOption();

        return result.Match(
            static row =>
            (
                row.Definition,
                row.RefreshMode != null && RefreshModeMapping.TryGetValue(row.RefreshMode, out var refreshMode)
                    ? refreshMode
                    : MaterializedViewRefreshMode.Unknown,
                !row.RefreshMethod.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.RefreshMethod)
                    : Option<string>.None,
                // a view built DEFERRED holds no data and cannot be queried until it is first
                // refreshed, which Oracle reports as an UNUSABLE staleness.
                !string.Equals(row.Staleness, UnusableValue, StringComparison.Ordinal)
            ),
            static () => ((string?)null, MaterializedViewRefreshMode.Unknown, Option<string>.None, false)
        );
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyList<IDatabaseColumn> columns)
    {
        var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

        foreach (var column in columns)
        {
            if (column.Name != null)
                result[column.Name] = column;
        }

        return result;
    }

    /// <summary>
    /// Retrieves the definition of a materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a materialized view.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<string?> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadDefinitionAsyncCore(viewName, cancellationToken);
    }

    private async Task<string?> LoadDefinitionAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var (definition, _, _, _) = await LoadDefinitionAndOptionsAsync(viewName, cancellationToken);
        return definition;
    }

    /// <summary>
    /// Retrieves the columns for a given materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadColumnsAsyncCore(viewName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var query = await DbConnection.QueryAsync(
            GetMaterializedViewColumns.Sql,
            new GetMaterializedViewColumns.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );

        var result = new List<IDatabaseColumn>();

        foreach (var row in query)
        {
            var typeMetadata = new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.DataLength,
                NumericPrecision = row.Precision > 0 || row.Scale > 0
                    ? Option<INumericPrecision>.Some(new NumericPrecision(row.Precision, row.Scale))
                    : Option<INumericPrecision>.None,
            };
            var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

            var isNullable = !string.Equals(row.IsNullable, NoValue, StringComparison.Ordinal);
            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var defaultValue = OracleDefaultValueParser.Parse(row.DefaultValue);
            // only a column declared INVISIBLE reaches this point; the query filters out the
            // system-generated hidden columns that back function-based indexes
            var isHidden = string.Equals(row.IsHidden, HiddenValue, StringComparison.Ordinal);

            var column = new OracleDatabaseColumn(
                columnName,
                columnType,
                isNullable,
                defaultValue,
                Option<IAutoIncrement>.None,
                false,
                Option<string>.None,
                ComputedColumnStorage.Unknown,
                isHidden);

            result.Add(column);
        }

        return result;
    }

    /// <summary>
    /// Qualifies the name of the view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <returns>A view name is at least as qualified as the given view name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyViewName(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var schema = viewName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
    }

    private const string NoValue = "N";

    private const string UnusableValue = "UNUSABLE";

    // ALL_TAB_COLS.HIDDEN_COLUMN value
    private const string HiddenValue = "YES";

    // ALL_MVIEWS.REFRESH_MODE values
    private static readonly IReadOnlyDictionary<string, MaterializedViewRefreshMode> RefreshModeMapping = new Dictionary<string, MaterializedViewRefreshMode>(StringComparer.OrdinalIgnoreCase)
    {
        ["DEMAND"] = MaterializedViewRefreshMode.OnDemand,
        ["COMMIT"] = MaterializedViewRefreshMode.OnCommit,
        ["NEVER"] = MaterializedViewRefreshMode.Never,
    };
}