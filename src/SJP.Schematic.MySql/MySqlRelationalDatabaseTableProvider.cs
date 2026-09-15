using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Queries;

namespace SJP.Schematic.MySql;

/// <summary>
/// A database table provider for MySQL.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class MySqlRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the given database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public MySqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

        _catalogFeatures = new AsyncLazy<GetCatalogFeatures.Result>(LoadCatalogFeatures);
    }

    /// <summary>
    /// A database connection that is specific to a given MySQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection factory to query the database.
    /// </summary>
    /// <value>A connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Creates a query cache for a given query context.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels every query the cache has started. Pass the token of the operation that owns the cache, since the cached queries are shared by every part of that operation.</param>
    /// <returns>A query cache.</returns>
    protected MySqlTableQueryCache CreateQueryCache(CancellationToken cancellationToken) => new(
        new AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token), cancellationToken),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token), cancellationToken),
        new AsyncCache<Identifier, TableKeys, MySqlTableQueryCache>(LoadKeysAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, MySqlTableQueryCache>(LoadIndexesAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache>(LoadParentKeysAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, MySqlTableQueryCache>(
            async (tableName, cache, token) => GetColumnLookup(await cache.GetColumnsAsync(tableName, token)), cancellationToken)
    );

    /// <summary>
    /// Enumerates all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async IAsyncEnumerable<IRelationalDatabaseTable> EnumerateAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache(cancellationToken);
        var tableNames = await LoadTableNamesAsync(queryCache, cancellationToken);

        var results = tableNames.SelectOrderedPrefetchAsync((tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct), Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);

        await foreach (var result in results.WithCancellation(cancellationToken))
            yield return result;
    }

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache(cancellationToken);
        var tableNames = await LoadTableNamesAsync(queryCache, cancellationToken);

        return await tableNames.SelectBoundedAsync((tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct), Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);
    }

    private async Task<IReadOnlyList<Identifier>> LoadTableNamesAsync(MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var tableNames = await DbConnection.QueryEnumerableAsync(
                GetAllTableNames.Sql,
                new GetAllTableNames.Query { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            )
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .ToListAsync(cancellationToken);

        // These names come straight from the catalog, so foreign keys between the tables being loaded
        // can find each other's names without querying for them again.
        foreach (var tableName in tableNames)
            queryCache.TryAddTableName(GetTableNameCacheKey(tableName), tableName);

        return tableNames;
    }

    /// <summary>
    /// Gets a database table.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var queryCache = CreateQueryCache(cancellationToken);
        var candidateTableName = QualifyTableName(tableName);
        return LoadTable(candidateTableName, queryCache, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="tableName">A table name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<Option<Identifier>> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        tableName = QualifyTableName(tableName);
        var qualifiedTableName = DbConnection.QueryFirstOrNone(
            GetTableName.Sql,
            new GetTableName.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName
            .Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.TableName))
            .ToOption();
    }

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">The query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <see langword="null" />.</exception>
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        var candidateTableName = QualifyTableName(tableName);

        return ResolveTableNameAsync(candidateTableName, queryCache, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private static async Task<Option<Identifier>> ResolveTableNameAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        // The name is resolved through the query cache, under the same key that foreign-key loading uses, so that
        // a self-referencing table or its child tables do not query for this table's name again. Table names may be
        // compared case-insensitively, so the name as stored in the catalog is cached too.
        var resolvedTableName = await queryCache.GetTableNameAsync(GetTableNameCacheKey(tableName), cancellationToken);
        resolvedTableName.IfSome(name => queryCache.TryAddTableName(GetTableNameCacheKey(name), name));
        return resolvedTableName;
    }

    private static Identifier GetTableNameCacheKey(Identifier tableName) => Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName);

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var (
            columns,
            checks,
            triggers,
            indexes,
            primaryKey,
            uniqueKeys,
            parentKeys,
            childKeys,
            options
        ) = await (
            queryCache.GetColumnsAsync(tableName, cancellationToken),
            LoadChecksAsync(tableName, cancellationToken),
            LoadTriggersAsync(tableName, cancellationToken),
            queryCache.GetIndexesAsync(tableName, cancellationToken),
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            queryCache.GetForeignKeysAsync(tableName, cancellationToken),
            LoadChildKeysAsync(tableName, queryCache, cancellationToken),
            LoadTableOptionsAsync(tableName, queryCache, cancellationToken)
        ).WhenAll();

        return new RelationalDatabaseTable(
            tableName,
            columns,
            primaryKey,
            uniqueKeys,
            parentKeys,
            childKeys,
            FilterConstraintIndexes(indexes, primaryKey, uniqueKeys),
            checks,
            triggers,
            options.Kind,
            options.Partitioning,
            options.SystemVersioning,
            options.IsLogged,
            options.Collation
        );
    }

    private async Task<TableOptions> LoadTableOptionsAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var optionsResult = await DbConnection.QueryAsync(
            GetTableOptions.Sql,
            new GetTableOptions.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var options = optionsResult.FirstOrDefault();
        if (options == null)
            return TableOptions.Default;

        var partitioning = Option<ITablePartitioning>.None;
        if (!options.PartitionMethod.IsNullOrWhiteSpace())
            partitioning = Option<ITablePartitioning>.Some(await LoadPartitioningAsync(tableName, options, queryCache, cancellationToken));

        var collation = !options.Collation.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(options.Collation))
            : Option<Identifier>.None;

        // MySQL does not report temporary tables in information_schema, and InnoDB always writes to
        // the redo log, so neither a temporary kind nor an unlogged table can be observed here.
        return new TableOptions(
            partitioning.IsSome ? TableKind.PartitionParent : TableKind.Regular,
            partitioning,
            Option<ITableSystemVersioning>.None,
            true,
            collation
        );
    }

    private async Task<ITablePartitioning> LoadPartitioningAsync(Identifier tableName, GetTableOptions.Result options, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var (partitionNames, columnLookup) = await (
            DbConnection.QueryAsync(
                GetTablePartitions.Sql,
                new GetTablePartitions.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ),
            queryCache.GetColumnLookupAsync(tableName, cancellationToken)
        ).WhenAll();

        var partitions = partitionNames
            .Select(static partitionName => Identifier.CreateQualifiedIdentifier(partitionName))
            .ToList();

        return new TablePartitioning(options.PartitionMethod!, GetPartitionColumns(options.PartitionExpression, columnLookup), partitions);
    }

    // MySQL records a partitioning expression rather than a column list. The KEY and COLUMNS methods
    // write that expression as nothing but a comma-separated list of quoted column names, which is
    // the only shape read back here; anything else, e.g. HASH(year(`d`)), reports no columns rather
    // than a guess at which columns the expression touches.
    private static IReadOnlyList<IDatabaseColumn> GetPartitionColumns(string? partitionExpression, IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup)
    {
        if (partitionExpression.IsNullOrWhiteSpace())
            return [];

        var columns = new List<IDatabaseColumn>();
        foreach (var part in partitionExpression.Split(','))
        {
            var trimmed = part.Trim();
            if (trimmed.Length < 3 || trimmed[0] != '`' || trimmed[^1] != '`')
                return [];

            var columnName = Identifier.CreateQualifiedIdentifier(trimmed[1..^1]);
            if (!columnLookup.TryGetValue(columnName, out var column))
                return [];

            columns.Add(column);
        }

        return columns;
    }

    private sealed record TableOptions(
        TableKind Kind,
        Option<ITablePartitioning> Partitioning,
        Option<ITableSystemVersioning> SystemVersioning,
        bool IsLogged,
        Option<Identifier> Collation
    )
    {
        public static TableOptions Default { get; } = new(
            TableKind.Regular,
            Option<ITablePartitioning>.None,
            Option<ITableSystemVersioning>.None,
            true,
            Option<Identifier>.None
        );
    }

    // An index that exists only to enforce a primary or unique key constraint is reported by that
    // constraint's IDatabaseKey.BackingIndex, so it is not repeated in the table's indexes. Without
    // this, every key would also be listed as an index, because information_schema.statistics does
    // not distinguish the two.
    private static IReadOnlyCollection<IDatabaseIndex> FilterConstraintIndexes(
        IReadOnlyCollection<IDatabaseIndex> indexes,
        Option<IDatabaseKey> primaryKey,
        IReadOnlyCollection<IDatabaseKey> uniqueKeys
    )
    {
        var backingIndexNames = uniqueKeys
            .Concat(primaryKey.ToList())
            .SelectMany(static key => key.BackingIndex.ToList())
            .Select(static index => index.Name)
            .ToHashSet();
        if (backingIndexNames.Count == 0)
            return indexes;

        return indexes.Where(index => !backingIndexNames.Contains(index.Name)).ToList();
    }

    // MySQL names the index enforcing a primary or unique key constraint after the constraint itself,
    // i.e. PRIMARY for a primary key, so the two are matched by name.
    private static Option<IDatabaseIndex> GetBackingIndex(IReadOnlyCollection<IDatabaseIndex> indexes, Identifier constraintName)
    {
        var backingIndex = indexes.FirstOrDefault(index => index.Name == constraintName);
        return backingIndex != null
            ? Option<IDatabaseIndex>.Some(backingIndex)
            : Option<IDatabaseIndex>.None;
    }

    /// <summary>
    /// Retrieves the primary key and unique keys for the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The table's primary key, if available, and its unique keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<TableKeys> LoadKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    // Primary and unique keys differ only in their constraint type, so both are read with one query. The
    // rows arrive in key column order.
    private async Task<TableKeys> LoadKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var keyRows = await DbConnection.QueryAsync(
            GetTableKeys.Sql,
            new GetTableKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var rows = keyRows.ToList();
        if (rows.Count == 0)
            return NoKeys;

        var (columnLookup, indexes) = await (
            queryCache.GetColumnLookupAsync(tableName, cancellationToken),
            queryCache.GetIndexesAsync(tableName, cancellationToken)
        ).WhenAll();

        var primaryKey = CreatePrimaryKey(
            rows.Where(static row => string.Equals(row.KeyType, Constants.PrimaryKey, StringComparison.Ordinal)),
            columnLookup,
            indexes
        );
        var uniqueKeys = CreateUniqueKeys(
            rows.Where(static row => string.Equals(row.KeyType, Constants.Unique, StringComparison.Ordinal)),
            columnLookup,
            indexes
        );

        return new TableKeys(primaryKey, uniqueKeys);
    }

    private static Option<IDatabaseKey> CreatePrimaryKey(
        IEnumerable<GetTableKeys.Result> rows,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        var groupedByName = rows.GroupAsDictionary(static row => new { row.ConstraintName });
        if (groupedByName.Count == 0)
            return Option<IDatabaseKey>.None;

        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;

        var keyColumns = groupedByName
            .Where(row => string.Equals(row.Key.ConstraintName, constraintName, StringComparison.Ordinal))
            .SelectMany(g => g.Value.ConvertAll(row => columnLookup[row.ColumnName!]))
            .ToList();

        var backingIndex = GetBackingIndex(indexes, constraintName);

        var primaryKey = new MySqlDatabasePrimaryKey(keyColumns, backingIndex);
        return Option<IDatabaseKey>.Some(primaryKey);
    }

    private static IReadOnlyCollection<IDatabaseKey> CreateUniqueKeys(
        IEnumerable<GetTableKeys.Result> rows,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        var groupedByName = rows.GroupAsDictionary(static row => new { row.ConstraintName });
        if (groupedByName.Count == 0)
            return [];

        var result = new List<IDatabaseKey>(groupedByName.Count);
        foreach (var uk in groupedByName)
        {
            var columns = uk.Value.ConvertAll(row => columnLookup[row.ColumnName!]);
            var backingIndex = GetBackingIndex(indexes, uk.Key.ConstraintName);

            var uniqueKey = new MySqlDatabaseKey(uk.Key.ConstraintName, DatabaseKeyType.Unique, columns, backingIndex);
            result.Add(uniqueKey);
        }
        return result;
    }

    private static readonly TableKeys NoKeys = new(Option<IDatabaseKey>.None, []);

    /// <summary>
    /// The primary key and unique keys declared on a table, which are read from the catalog together.
    /// </summary>
    /// <param name="PrimaryKey">The table's primary key, if it has one.</param>
    /// <param name="UniqueKeys">The table's unique keys.</param>
    protected sealed record TableKeys(Option<IDatabaseKey> PrimaryKey, IReadOnlyCollection<IDatabaseKey> UniqueKeys);

    /// <summary>
    /// Retrieves indexes that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of indexes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var features = await _catalogFeatures;
        var indexesSql = features.HasIndexExpressionColumn ? GetTableIndexes.Sql : GetTableIndexes.SqlWithoutExpression;

        var queryResult = await DbConnection.QueryAsync(
            indexesSql,
            new GetTableIndexes.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var indexColumns = queryResult
            .GroupAsDictionary(static row => new { row.IndexName, row.IsNonUnique, row.IndexType, row.IsVisible })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var result = new List<IDatabaseIndex>(indexColumns.Count);

        foreach (var indexInfo in indexColumns)
        {
            var isUnique = !indexInfo.Key.IsNonUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var indexCols = indexInfo.Value
                .OrderBy(static row => row.ColumnOrdinal)
                .Select(row =>
                {
                    // 'D' is the only value that means descending, an unsorted index reports null
                    var order = string.Equals(row.ColumnSort, Constants.DescendingSort, StringComparison.OrdinalIgnoreCase)
                        ? IndexColumnOrder.Descending
                        : IndexColumnOrder.Ascending;

                    // a functional index column is defined by an expression instead of a column
                    if (row.ColumnName == null || !columnLookup.TryGetValue(row.ColumnName, out var column))
                    {
                        return !row.Expression.IsNullOrWhiteSpace()
                            ? new MySqlDatabaseIndexColumn(row.Expression, order)
                            : null;
                    }

                    var prefixLength = row.PrefixLength.HasValue
                        ? Option<int>.Some(row.PrefixLength.Value)
                        : Option<int>.None;

                    return new MySqlDatabaseIndexColumn(Dialect.QuoteName(column.Name), column, order, prefixLength);
                })
                .Where(static col => col != null)
                .Select(static col => col!)
                .ToList();
            if (indexCols.Empty())
                continue;

            var indexType = indexInfo.Key.IndexType != null && IndexTypeMapping.TryGetValue(indexInfo.Key.IndexType, out var mappedIndexType)
                ? mappedIndexType
                : IndexType.Unknown;
            var isVisible = !string.Equals(indexInfo.Key.IsVisible, Constants.No, StringComparison.OrdinalIgnoreCase);

            var index = new MySqlDatabaseIndex(indexName, isUnique, indexCols, indexType, isVisible);
            result.Add(index);
        }

        return result;
    }

    /// <summary>
    /// Retrieves child keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of child keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableChildKeys.Sql,
            new GetTableChildKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var (primaryKey, uniqueKeys) = await (
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken)
        ).WhenAll();
        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

        // Resolve the key each foreign key references on this table first, so that a child table whose
        // foreign keys reference nothing that can be resolved is never queried at all.
        var childTables = new Dictionary<(string Schema, string Name), List<ChildForeignKey>>();
        foreach (var childKey in queryResult)
        {
            IDatabaseKey? parentKey = null;
            if (string.Equals(childKey.ParentKeyType, Constants.PrimaryKey, StringComparison.Ordinal))
                primaryKey.IfSome(k => parentKey = k);
            else if (uniqueKeyLookup.TryGetValue(childKey.ParentKeyName, out var uniqueKey))
                parentKey = uniqueKey;

            if (parentKey == null)
                continue;

            var childTableKey = (childKey.ChildTableSchema, childKey.ChildTableName);
            if (!childTables.TryGetValue(childTableKey, out var childForeignKeys))
            {
                childForeignKeys = [];
                childTables[childTableKey] = childForeignKeys;
            }
            childForeignKeys.Add(new ChildForeignKey(childKey, parentKey));
        }

        if (childTables.Count == 0)
            return [];

        // Each child table only needs its name, columns and foreign key columns to build its side of the
        // foreign key, so the child tables are independent of one another and are loaded concurrently.
        var childTableKeys = await childTables.ToList().SelectBoundedAsync(
            (childTable, ct) => LoadChildTableKeysAsync(tableName, childTable.Key.Schema, childTable.Key.Name, childTable.Value, queryCache, ct),
            Math.Max(1, DbConnection.MaxConcurrentQueries),
            cancellationToken);

        return childTableKeys.SelectMany(static keys => keys).ToList();
    }

    private async Task<IReadOnlyList<IDatabaseRelationalKey>> LoadChildTableKeysAsync(
        Identifier tableName,
        string childTableSchema,
        string childTableLocalName,
        IReadOnlyCollection<ChildForeignKey> foreignKeys,
        MySqlTableQueryCache queryCache,
        CancellationToken cancellationToken)
    {
        var candidateChildTableName = Identifier.CreateQualifiedIdentifier(childTableSchema, childTableLocalName);
        var resolvedChildTableName = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken);
        var childTableName = resolvedChildTableName.MatchUnsafe(static name => name, static () => (Identifier?)null);
        if (childTableName == null)
            return [];

        // The child table's columns are read by key, which only opens that one table. Joining them onto the
        // child-key query instead would make MariaDB scan the key columns of every table on the server on
        // every call, including for tables that have no child keys at all.
        var (childColumnLookup, foreignKeyColumns) = await (
            queryCache.GetColumnLookupAsync(childTableName, cancellationToken),
            DbConnection.QueryAsync(
                GetTableForeignKeyColumns.Sql,
                new GetTableForeignKeyColumns.Query { SchemaName = childTableName.Schema!, TableName = childTableName.LocalName },
                cancellationToken
            )
        ).WhenAll();
        var foreignKeyColumnLookup = foreignKeyColumns.GroupAsDictionary(static row => row.ChildKeyName, StringComparer.Ordinal);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var foreignKey in foreignKeys)
        {
            var row = foreignKey.Row;
            if (!foreignKeyColumnLookup.TryGetValue(row.ChildKeyName, out var columnRows))
                continue;

            var childKey = CreateForeignKey(row.ChildKeyName, columnRows, childColumnLookup);
            var deleteAction = ReferentialActionMapping[row.DeleteAction];
            var updateAction = ReferentialActionMapping[row.UpdateAction];

            result.Add(new MySqlRelationalKey(childTableName, childKey, tableName, foreignKey.ParentKey, deleteAction, updateAction));
        }

        return result;
    }

    private sealed record ChildForeignKey(GetTableChildKeys.Result Row, IDatabaseKey ParentKey);

    // Builds a foreign key as declared on the table holding it. Parent-key and child-key loading both build
    // it here, so a foreign key reads the same whichever end of the relationship it is loaded from.
    private static MySqlDatabaseKey CreateForeignKey(string constraintName, IEnumerable<IForeignKeyColumnRow> columnRows, IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup)
    {
        var keyName = Identifier.CreateQualifiedIdentifier(constraintName);
        var keyColumns = columnRows
            .OrderBy(static row => row.ConstraintColumnId)
            .Select(row => columnLookup[row.ColumnName])
            .ToList();

        return new MySqlDatabaseKey(keyName, DatabaseKeyType.Foreign, keyColumns);
    }

    /// <summary>
    /// Retrieves check constraints defined on a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of check constraints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadChecksAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var features = await _catalogFeatures;
        if (!features.HasCheckConstraints)
            return [];

        var sql = (features.HasConstraintEnforcedColumn, features.HasCheckConstraintTableNameColumn) switch
        {
            (true, _) => GetTableCheckConstraints.Sql,
            (false, true) => GetTableCheckConstraints.SqlByTableName,
            (false, false) => GetTableCheckConstraints.SqlWithoutEnforced,
        };

        return await DbConnection.QueryEnumerableAsync(
                sql,
                new GetTableCheckConstraints.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            )
            .Select(row =>
            {
                var checkName = Identifier.CreateQualifiedIdentifier(row.ConstraintName);
                var isEnabled = string.Equals("YES", row.Enforced, StringComparison.OrdinalIgnoreCase);
                return new MySqlCheckConstraint(checkName, row.Definition, isEnabled);
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves foreign keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableParentKeys.Sql,
            new GetTableParentKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var foreignKeys = queryResult.GroupAsDictionary(static row => new
        {
            row.ChildKeyName,
            row.ParentTableSchema,
            row.ParentTableName,
            row.ParentKeyName,
            KeyType = row.ParentKeyType,
            row.DeleteAction,
            row.UpdateAction,
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        // Several foreign keys can reference the same parent table, so its unique-key lookup is
        // memoised here rather than rebuilt for every foreign key.
        var parentUniqueKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
            var resolvedName = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken);

            Identifier? parentTableName = null;

            await resolvedName
                .BindAsync(async name =>
                {
                    parentTableName = name;
                    if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKey, StringComparison.Ordinal))
                    {
                        var primaryKey = await queryCache.GetPrimaryKeyAsync(name, cancellationToken);
                        return primaryKey.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                    if (!parentUniqueKeyLookups.TryGetValue(name, out var parentUniqueKeyLookup))
                    {
                        var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(name, cancellationToken);
                        parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);
                        parentUniqueKeyLookups[name] = parentUniqueKeyLookup;
                    }

                    return parentUniqueKeyLookup.TryGetValue(parentKeyName.LocalName, out var uniqueKey)
                        ? OptionAsync<IDatabaseKey>.Some(uniqueKey)
                        : OptionAsync<IDatabaseKey>.None;
                })
                .IfSome(key =>
                {
                    var childKey = CreateForeignKey(fkey.Key.ChildKeyName, fkey.Value, columnLookup);

                    var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                    var relationalKey = new DatabaseRelationalKey(tableName, childKey, parentTableName!, key, deleteAction, updateAction);
                    result.Add(relationalKey);
                });
        }

        return result;
    }

    /// <summary>
    /// Retrieves the columns for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadColumnsAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        return await DbConnection.QueryEnumerableAsync(
                GetTableColumns.Sql,
                new GetTableColumns.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            )
            .Select(row =>
            {
                var typeMetadata = MySqlColumnTypeMetadata.Create(
                    row.DataTypeName,
                    row.ColumnType,
                    !row.Collation.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                        : Option<Identifier>.None,
                    row.CharacterMaxLength,
                    new NumericPrecision(row.Precision, row.Scale),
                    row.DateTimePrecision.HasValue
                        ? Option<int>.Some(row.DateTimePrecision.Value)
                        : Option<int>.None);
                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                // MySQL exposes no per-column start or increment: the starting value is a table
                // option that moves as rows are inserted, and the step is the server-wide
                // auto_increment_increment variable. Both are server state rather than schema, so
                // the sequence is described by its defaults of 1 and 1.
                var isAutoIncrement = row.ExtraInformation?.Contains(Constants.AutoIncrement, StringComparison.OrdinalIgnoreCase) == true;
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None))
                    : Option<IAutoIncrement>.None;
                var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
                var isNullable = !string.Equals(row.IsNullable, Constants.No, StringComparison.OrdinalIgnoreCase);
                var defaultValue = MySqlDefaultValueParser.Parse(row.DefaultValue, row.ExtraInformation);
                var computedColumnDefinition = isComputed
                    ? Option<string>.Some(row.ComputedColumnDefinition!)
                    : Option<string>.None;

                // 'extra' spells out how a generated column is kept, e.g. 'STORED GENERATED'.
                var computedStorage = row.ExtraInformation?.Contains(Constants.StoredGenerated, StringComparison.OrdinalIgnoreCase) == true
                    ? ComputedColumnStorage.Stored
                    : ComputedColumnStorage.Virtual;

                // MySQL 8.0.23 and later report a column declared INVISIBLE in 'extra' as well.
                var isHidden = row.ExtraInformation?.Contains(Constants.Invisible, StringComparison.OrdinalIgnoreCase) == true;

                return new DatabaseColumn(
                    columnName,
                    columnType,
                    isNullable,
                    defaultValue,
                    autoIncrement,
                    isComputed,
                    computedColumnDefinition,
                    computedStorage,
                    isHidden);
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all triggers defined on a table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of triggers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadTriggersAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableTriggers.Sql,
            new GetTableTriggers.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var triggers = queryResult.GroupAsDictionary(static row => new
        {
            row.TriggerName,
            row.Definition,
            row.Timing,
        }).ToList();
        if (triggers.Empty())
            return [];

        var result = new List<IDatabaseTrigger>(triggers.Count);
        foreach (var trig in triggers)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
            var queryTiming = Enum.TryParse(trig.Key.Timing, true, out TriggerQueryTiming timing) ? timing : TriggerQueryTiming.Before;
            var definition = trig.Key.Definition;

            var events = TriggerEvent.None;
            foreach (var trigEvent in trig.Value.Select(tr => tr.TriggerEvent))
            {
                if (string.Equals(trigEvent, Constants.Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(trigEvent, Constants.Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(trigEvent, Constants.Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else
                    events |= TriggerEvent.Other;
            }

            var trigger = new MySqlDatabaseTrigger(triggerName, definition, queryTiming, events);
            result.Add(trigger);
        }

        return result;
    }

    /// <summary>
    /// Qualifies the name of a table, using known identifier defaults.
    /// </summary>
    /// <param name="tableName">A table name to qualify.</param>
    /// <returns>A table name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyTableName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var schema = tableName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
    }

    /// <summary>
    /// A mapping from the referential actions as described in MySQL, to a <see cref="ReferentialAction"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<string, ReferentialAction>(StringComparer.OrdinalIgnoreCase)
    {
        ["NO ACTION"] = ReferentialAction.NoAction,
        ["RESTRICT"] = ReferentialAction.Restrict,
        ["CASCADE"] = ReferentialAction.Cascade,
        ["SET NULL"] = ReferentialAction.SetNull,
        ["SET DEFAULT"] = ReferentialAction.SetDefault,
    };

    private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

        foreach (var column in columns)
        {
            if (column.Name != null)
                result[column.Name.LocalName] = column;
        }

        return result;
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseKey> GetDatabaseKeyLookup(IReadOnlyCollection<IDatabaseKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<Identifier, IDatabaseKey>(keys.Count);

        foreach (var key in keys)
        {
            key.Name.IfSome(name => result[name.LocalName] = key);
        }

        return result;
    }

    // The catalog varies between MySQL and MariaDB releases, so the queries that depend on those
    // differences are chosen from a single description of what the connected server provides.
    private Task<GetCatalogFeatures.Result> LoadCatalogFeatures() =>
        DbConnection.QuerySingleAsync<GetCatalogFeatures.Result>(GetCatalogFeatures.Sql, CancellationToken.None);

    private readonly AsyncLazy<GetCatalogFeatures.Result> _catalogFeatures;

    // information_schema.statistics.index_type values.
    private static readonly IReadOnlyDictionary<string, IndexType> IndexTypeMapping = new Dictionary<string, IndexType>(StringComparer.OrdinalIgnoreCase)
    {
        ["BTREE"] = IndexType.BTree,
        ["HASH"] = IndexType.Hash,
        ["FULLTEXT"] = IndexType.FullText,
        ["SPATIAL"] = IndexType.Spatial,
        ["RTREE"] = IndexType.Spatial,
    };

    private static class Constants
    {
        public const string AutoIncrement = "auto_increment";

        public const string Delete = "DELETE";

        public const string DescendingSort = "D";

        public const string Insert = "INSERT";

        public const string Invisible = "INVISIBLE";

        public const string No = "NO";

        public const string PrimaryKey = "PRIMARY KEY";

        public const string StoredGenerated = "STORED GENERATED";

        public const string Unique = "UNIQUE";

        public const string Update = "UPDATE";
    }

    /// <summary>
    /// A query cache provider for MySQL tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class MySqlTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, TableKeys, MySqlTableQueryCache> _keys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, MySqlTableQueryCache> _indexes;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache> _foreignKeys;
        private readonly AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, MySqlTableQueryCache> _columnLookups;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="keyLoader">A primary and unique key cache.</param>
        /// <param name="indexLoader">An index cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <param name="columnLookupLoader">A column lookup cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="keyLoader"/>, <paramref name="indexLoader"/>, <paramref name="foreignKeyLoader"/> or <paramref name="columnLookupLoader"/> are <see langword="null" />.</exception>
        public MySqlTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache> columnLoader,
            AsyncCache<Identifier, TableKeys, MySqlTableQueryCache> keyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, MySqlTableQueryCache> indexLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache> foreignKeyLoader,
            AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, MySqlTableQueryCache> columnLookupLoader
        )
        {
            _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _keys = keyLoader ?? throw new ArgumentNullException(nameof(keyLoader));
            _indexes = indexLoader ?? throw new ArgumentNullException(nameof(indexLoader));
            _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
            _columnLookups = columnLookupLoader ?? throw new ArgumentNullException(nameof(columnLookupLoader));
        }

        /// <summary>
        /// Retrieves the table name from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table name, if matched in the database.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<Option<Identifier>> GetTableNameAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _tableNames.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Adds a table name that is already known to exist, so that looking up <paramref name="tableName"/> does not query the database.
        /// </summary>
        /// <param name="tableName">A table name, in the form it will be looked up with.</param>
        /// <param name="resolvedTableName">The resolved name of the table.</param>
        /// <returns><see langword="true" /> if the name was added; <see langword="false" /> if <paramref name="tableName"/> is already cached.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="resolvedTableName"/> is <see langword="null" />.</exception>
        public bool TryAddTableName(Identifier tableName, Identifier resolvedTableName)
        {
            ArgumentNullException.ThrowIfNull(tableName);
            ArgumentNullException.ThrowIfNull(resolvedTableName);

            return _tableNames.TryAdd(tableName, Option<Identifier>.Some(resolvedTableName));
        }

        /// <summary>
        /// Retrieves a table's columns from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyList<IDatabaseColumn>> GetColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _columns.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's primary key from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A primary key, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<Option<IDatabaseKey>> GetPrimaryKeyAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return GetPrimaryKeyAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> GetPrimaryKeyAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var keys = await _keys.GetByKeyAsync(tableName, this, cancellationToken);
            return keys.PrimaryKey;
        }

        /// <summary>
        /// Retrieves a table's unique keys from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of unique keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyCollection<IDatabaseKey>> GetUniqueKeysAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return GetUniqueKeysAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> GetUniqueKeysAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var keys = await _keys.GetByKeyAsync(tableName, this, cancellationToken);
            return keys.UniqueKeys;
        }

        /// <summary>
        /// Retrieves a table's indexes from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of indexes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyCollection<IDatabaseIndex>> GetIndexesAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _indexes.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's foreign keys from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> GetForeignKeysAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _foreignKeys.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's column lookup from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A lookup of a table's columns, keyed by column name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _columnLookups.GetByKeyAsync(tableName, this, cancellationToken);
        }
    }
}