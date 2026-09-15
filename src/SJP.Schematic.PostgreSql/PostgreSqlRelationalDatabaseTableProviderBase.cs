using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A database table provider for PostgreSQL.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class PostgreSqlRelationalDatabaseTableProviderBase : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlRelationalDatabaseTableProviderBase"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public PostgreSqlRelationalDatabaseTableProviderBase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
    }

    /// <summary>
    /// A database connection that is specific to a given PostgreSQL database.
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
    /// Gets a database column type provider.
    /// </summary>
    /// <value>A type provider.</value>
    protected IDbTypeProvider TypeProvider => Dialect.TypeProvider;

    /// <summary>
    /// Creates a query cache for a given query context.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels every query the cache has started. Pass the token of the operation that owns the cache, since the cached queries are shared by every part of that operation.</param>
    /// <returns>A query cache.</returns>
    /// <remarks>
    /// Table names in the cache are matched exactly, without applying the identifier resolution strategy, because
    /// foreign-key loading looks them up using names read from the catalog. Case-folding a catalog name could match
    /// a different table whose name differs only in case.
    /// </remarks>
    protected PostgreSqlTableQueryCache CreateQueryCache(CancellationToken cancellationToken) => new(
        new AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache>((tableName, _, token) => GetResolvedTableNameStrict(tableName, token).ToOption(), cancellationToken),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token), cancellationToken),
        new AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, PostgreSqlTableQueryCache>(async (tableName, cache, token) => GetColumnLookup(await cache.GetColumnsAsync(tableName, token)), cancellationToken),
        new AsyncCache<Identifier, TableKeys, PostgreSqlTableQueryCache>(LoadKeysAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, PostgreSqlTableQueryCache>(LoadIndexesAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache>(LoadParentKeysAsync, cancellationToken)
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

        var tables = tableNames.SelectOrderedPrefetchAsync(
            (tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct),
            Math.Max(1, DbConnection.MaxConcurrentQueries),
            cancellationToken);

        await foreach (var table in tables.WithCancellation(cancellationToken))
            yield return table;
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

        return await tableNames.SelectBoundedAsync(
            (tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct),
            Math.Max(1, DbConnection.MaxConcurrentQueries),
            cancellationToken);
    }

    private async Task<IReadOnlyList<Identifier>> LoadTableNamesAsync(PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var tableNames = await DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
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
    protected Task<Option<Identifier>> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(tableName)
            .Select(QualifyTableName);

        return resolvedNames
            .Select(name => GetResolvedTableNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken)
            .ToOption();
    }

    /// <summary>
    /// Gets the resolved name of the table without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="tableName">A table name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedTableNameStrict(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var candidateTableName = QualifyTableName(tableName);
        var qualifiedTableName = DbConnection.QueryFirstOrNone(
            GetTableName.Sql,
            new GetTableName.Query { SchemaName = candidateTableName.Schema!, TableName = candidateTableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.TableName));
    }

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">The query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <see langword="null" />.</exception>
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        var candidateTableName = QualifyTableName(tableName);
        return ResolveTableNameAsync(candidateTableName, queryCache, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<Option<Identifier>> ResolveTableNameAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        // Each candidate is looked up exactly, through the query cache and under the same key that foreign-key
        // loading uses. When the table references itself, that foreign key's lookup of this table's name then
        // finds the result instead of querying for it again.
        foreach (var candidateTableName in IdentifierResolver.GetResolutionOrder(tableName))
        {
            var resolvedTableName = await queryCache.GetTableNameAsync(GetTableNameCacheKey(QualifyTableName(candidateTableName)), cancellationToken);
            if (resolvedTableName.IsSome)
                return resolvedTableName;
        }

        return Option<Identifier>.None;
    }

    private static Identifier GetTableNameCacheKey(Identifier tableName) => Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName);

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
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

    private async Task<TableOptions> LoadTableOptionsAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
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
        if (string.Equals(options.RelKind, PartitionedRelKind, StringComparison.Ordinal))
            partitioning = Option<ITablePartitioning>.Some(await LoadPartitioningAsync(tableName, options, queryCache, cancellationToken));

        var kind = options.IsPartition
            ? TableKind.Partition
            : partitioning.IsSome
                ? TableKind.PartitionParent
                : string.Equals(options.Persistence, TemporaryPersistence, StringComparison.Ordinal)
                    ? TableKind.Temporary
                    : TableKind.Regular;

        var isLogged = !string.Equals(options.Persistence, UnloggedPersistence, StringComparison.Ordinal);

        // PostgreSQL has no table-level collation; it is defined per character column instead.
        return new TableOptions(kind, partitioning, Option<ITableSystemVersioning>.None, isLogged, Option<Identifier>.None);
    }

    private async Task<ITablePartitioning> LoadPartitioningAsync(Identifier tableName, GetTableOptions.Result options, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var (partitionColumnNames, partitionNames, columnLookup) = await (
            DbConnection.QueryAsync(
                GetTablePartitionColumns.Sql,
                new GetTablePartitionColumns.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ),
            DbConnection.QueryAsync(
                GetTablePartitions.Sql,
                new GetTablePartitions.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ),
            queryCache.GetColumnLookupAsync(tableName, cancellationToken)
        ).WhenAll();

        var partitionColumns = partitionColumnNames
            .Select(columnName => columnLookup.TryGetValue(columnName, out var column) ? column : null)
            .Where(static column => column != null)
            .Select(static column => column!)
            .ToList();
        var partitions = partitionNames
            .Select(partition => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, partition.SchemaName, partition.TableName))
            .ToList();

        var strategy = PartitionStrategyMapping.TryGetValue(options.PartitionStrategy ?? string.Empty, out var mappedStrategy)
            ? mappedStrategy
            : UnknownPartitionStrategy;

        return new TablePartitioning(strategy, partitionColumns, partitions);
    }

    private const string PartitionedRelKind = "p";
    private const string TemporaryPersistence = "t";
    private const string UnloggedPersistence = "u";
    private const string UnknownPartitionStrategy = "UNKNOWN";

    private static readonly IReadOnlyDictionary<string, string> PartitionStrategyMapping = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["r"] = "RANGE",
        ["l"] = "LIST",
        ["h"] = "HASH",
    };

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
    // constraint's IDatabaseKey.BackingIndex, so it is not repeated in the table's indexes.
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

    // PostgreSQL names the index enforcing a primary or unique key constraint after the constraint
    // itself, so the two are matched by name.
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
    protected Task<TableKeys> LoadKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    // Primary and unique keys differ only in their constraint type, so both are read with one query.
    private async Task<TableKeys> LoadKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
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
            rows.Where(static row => string.Equals(row.KeyType, Constants.PrimaryKeyType, StringComparison.Ordinal)),
            columnLookup,
            indexes
        );
        var uniqueKeys = CreateUniqueKeys(
            rows.Where(static row => string.Equals(row.KeyType, Constants.UniqueKeyType, StringComparison.Ordinal)),
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
        var groupedByName = rows.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDeferrable, row.IsInitiallyDeferred });
        if (groupedByName.Count == 0)
            return Option<IDatabaseKey>.None;

        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;
        if (constraintName == null)
            return Option<IDatabaseKey>.None;

        var keyColumns = ResolveColumns(
            firstRow.Value
                .Where(static row => row.ColumnName != null)
                .OrderBy(static row => row.OrdinalPosition)
                .Select(static row => (Identifier)row.ColumnName!),
            columnLookup
        ).ToList();

        var backingIndex = GetBackingIndex(indexes, constraintName);
        var deferrability = GetDeferrability(firstRow.Key.IsDeferrable, firstRow.Key.IsInitiallyDeferred);

        var primaryKey = new PostgreSqlDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, backingIndex, true, deferrability);
        return Option<IDatabaseKey>.Some(primaryKey);
    }

    private static IReadOnlyCollection<IDatabaseKey> CreateUniqueKeys(
        IEnumerable<GetTableKeys.Result> rows,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        var groupedByName = rows.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDeferrable, row.IsInitiallyDeferred });
        if (groupedByName.Count == 0)
            return [];

        var result = new List<IDatabaseKey>(groupedByName.Count);
        foreach (var uk in groupedByName)
        {
            var columns = ResolveColumns(
                uk.Value
                    .Where(static row => row.ColumnName != null)
                    .OrderBy(static row => row.OrdinalPosition)
                    .Select(static row => (Identifier)row.ColumnName!),
                columnLookup
            ).ToList();
            var backingIndex = GetBackingIndex(indexes, uk.Key.ConstraintName);
            var deferrability = GetDeferrability(uk.Key.IsDeferrable, uk.Key.IsInitiallyDeferred);

            var uniqueKey = new PostgreSqlDatabaseKey(uk.Key.ConstraintName, DatabaseKeyType.Unique, columns, backingIndex, true, deferrability);
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
    protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableIndexes.Sql,
            new GetTableIndexes.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        return PostgreSqlCatalogMapper.MapIndexes(queryResult, columnLookup, Dialect);
    }

    /// <summary>
    /// Retrieves child keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of child keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableChildKeys.Sql,
            new GetTableChildKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var (primaryKey, uniqueKeys, indexes) = await (
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            queryCache.GetIndexesAsync(tableName, cancellationToken)
        ).WhenAll();
        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);
        var uniqueIndexLookup = GetUniqueIndexLookup(indexes);

        // Resolve the key each foreign key references on this table first, so that a child table whose
        // foreign keys reference nothing that can be resolved is never queried at all.
        var childTables = new Dictionary<(string Schema, string Name), List<ChildForeignKey>>();
        foreach (var foreignKey in queryResult.GroupAsDictionary(static row => new { row.ChildTableSchema, row.ChildTableName, row.ChildKeyName, row.ParentKeyName, row.ParentKeyType }))
        {
            IDatabaseKey? parentKey = null;
            if (string.Equals(foreignKey.Key.ParentKeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                primaryKey.IfSome(k => parentKey = k);
            else if (uniqueKeyLookup.TryGetValue(foreignKey.Key.ParentKeyName, out var uniqueKey))
                parentKey = uniqueKey;
            else if (uniqueIndexLookup.TryGetValue(foreignKey.Key.ParentKeyName, out var uniqueIndex))
                // the foreign key references a unique index with no backing UNIQUE constraint
                parentKey = CreateKeyFromUniqueIndex(uniqueIndex);

            if (parentKey == null)
                continue;

            var childTableKey = (foreignKey.Key.ChildTableSchema, foreignKey.Key.ChildTableName);
            if (!childTables.TryGetValue(childTableKey, out var childForeignKeys))
            {
                childForeignKeys = [];
                childTables[childTableKey] = childForeignKeys;
            }
            childForeignKeys.Add(new ChildForeignKey(parentKey, foreignKey.Value));
        }

        if (childTables.Count == 0)
            return [];

        // Each child table only needs its name and columns to build its side of the foreign key, so the
        // child tables are independent of one another and are loaded concurrently.
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
        PostgreSqlTableQueryCache queryCache,
        CancellationToken cancellationToken)
    {
        var candidateChildTableName = Identifier.CreateQualifiedIdentifier(childTableSchema, childTableLocalName);
        var resolvedChildTableName = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken);
        var childTableName = resolvedChildTableName.MatchUnsafe(static name => name, static () => (Identifier?)null);
        if (childTableName == null)
            return [];

        var childColumnLookup = await queryCache.GetColumnLookupAsync(childTableName, cancellationToken);

        return foreignKeys
            .Select(fk => CreateRelationalKey(childTableName, childColumnLookup, tableName, fk.ParentKey, fk.Rows))
            .ToList();
    }

    private sealed record ChildForeignKey(IDatabaseKey ParentKey, IReadOnlyCollection<GetTableChildKeys.Result> Rows);

    /// <summary>
    /// Retrieves check constraints defined on a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of check constraints.</returns>
    protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        const string checkPrefix = "CHECK (";
        const string checkSuffix = ")";

        return await DbConnection.QueryEnumerableAsync(
                GetTableChecks.Sql,
                new GetTableChecks.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            )
            .Where(checkRow => !checkRow.Definition.IsNullOrWhiteSpace())
            .Select(checkRow =>
            {
                var definition = checkRow.Definition!;
                if (definition.StartsWith(checkPrefix, StringComparison.OrdinalIgnoreCase))
                    definition = definition[checkPrefix.Length..];
                if (definition.EndsWith(')') && definition.Length > 0) // check suffix
                    definition = definition[..^checkSuffix.Length];

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);

                return new PostgreSqlCheckConstraint(constraintName, definition, checkRow.IsValidated);
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
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
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
            row.ParentSchemaName,
            row.ParentTableName,
            row.ParentKeyName,
            KeyType = row.ParentKeyType,
            row.DeleteAction,
            row.UpdateAction,
            row.IsValidated,
            row.IsDeferrable,
            row.IsInitiallyDeferred,
            row.MatchType,
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        // memoises the parent table's unique-key/unique-index lookups across foreign keys that share
        // the same parent table, instead of rebuilding them (and re-querying the cache) once per key.
        var parentUniqueKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>(IdentifierComparer.Ordinal);
        var parentUniqueIndexLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseIndex>>(IdentifierComparer.Ordinal);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentSchemaName, fkey.Key.ParentTableName);
            var parentTableNameOption = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken);
            Identifier? resolvedParentTableName = null;

            await parentTableNameOption
                .BindAsync(async parentTableName =>
                {
                    resolvedParentTableName = parentTableName;
                    if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                    {
                        var pk = await queryCache.GetPrimaryKeyAsync(parentTableName, cancellationToken);
                        return pk.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                    if (!parentUniqueKeyLookups.TryGetValue(parentTableName, out var uniqueKeyLookup))
                    {
                        var uniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken);
                        uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);
                        parentUniqueKeyLookups[parentTableName] = uniqueKeyLookup;
                    }

                    if (uniqueKeyLookup.TryGetValue(parentKeyName.LocalName, out var uniqueKey))
                        return OptionAsync<IDatabaseKey>.Some(uniqueKey);

                    // the foreign key references a unique index with no backing UNIQUE constraint
                    if (!parentUniqueIndexLookups.TryGetValue(parentTableName, out var parentUniqueIndexLookup))
                    {
                        var parentIndexes = await queryCache.GetIndexesAsync(parentTableName, cancellationToken);
                        parentUniqueIndexLookup = GetUniqueIndexLookup(parentIndexes);
                        parentUniqueIndexLookups[parentTableName] = parentUniqueIndexLookup;
                    }

                    return parentUniqueIndexLookup.TryGetValue(parentKeyName.LocalName, out var uniqueIndex)
                        ? OptionAsync<IDatabaseKey>.Some(CreateKeyFromUniqueIndex(uniqueIndex))
                        : OptionAsync<IDatabaseKey>.None;
                })
                .Map(parentKey => CreateRelationalKey(tableName, columnLookup, resolvedParentTableName!, parentKey, fkey.Value))
                .IfSome(result.Add);
        }

        return result;
    }

    // Builds a foreign key from its column rows. Parent-key and child-key loading both build keys here, so a
    // foreign key reads the same whether it is loaded from the table declaring it or from the table it references.
    private IDatabaseRelationalKey CreateRelationalKey(
        Identifier childTableName,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> childColumnLookup,
        Identifier parentTableName,
        IDatabaseKey parentKey,
        IReadOnlyCollection<IForeignKeyColumnRow> rows)
    {
        // every row describes the same constraint, differing only in the column it names
        var constraint = rows.First();

        var orderedRows = rows
            .Where(static row => row.ColumnName != null)
            .OrderBy(static row => row.ConstraintColumnId)
            .ToList();

        var childKeyName = Identifier.CreateQualifiedIdentifier(constraint.ChildKeyName);
        var childKeyColumns = ResolveColumns(orderedRows.Select(static row => (Identifier)row.ColumnName), childColumnLookup).ToList();

        var deferrability = GetDeferrability(constraint.IsDeferrable, constraint.IsInitiallyDeferred);
        var childKey = new PostgreSqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, Option<IDatabaseIndex>.None, constraint.IsValidated, deferrability);

        var deleteAction = ReferentialActionMapping[constraint.DeleteAction];
        var updateAction = ReferentialActionMapping[constraint.UpdateAction];
        var matchType = MatchTypeMapping.TryGetValue(constraint.MatchType, out var mappedMatchType)
            ? mappedMatchType
            : ForeignKeyMatchType.Simple;
        var setNullColumns = ResolveColumns(
            orderedRows
                .Where(static row => row.IsSetNullColumn)
                .Select(static row => (Identifier)row.ColumnName),
            childColumnLookup
        ).ToList();

        return new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction, matchType, setNullColumns);
    }

    /// <summary>
    /// Retrieves the columns for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
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
                var typeMetadata = PostgreSqlColumnTypeMetadata.Create(
                    TypeProvider,
                    new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
                        row.DataType,
                        row.UdtSchema,
                        row.UdtName,
                        row.DomainSchema,
                        row.DomainName,
                        row.TypeKind,
                        row.ElementTypeSchema,
                        row.ElementTypeName,
                        row.ElementTypeKind,
                        row.EnumLabels),
                    !row.CollationName.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationCatalog, row.CollationSchema, row.CollationName))
                        : Option<Identifier>.None,
                    PostgreSqlColumnTypeMetadata.CreateMaxLength(row.CharacterMaximumLength, row.NumericPrecision, row.NumericPrecisionRadix),
                    PostgreSqlColumnTypeMetadata.CreateNumericPrecision(row.NumericPrecision, row.NumericScale, row.NumericPrecisionRadix),
                    row.DatetimePrecision.HasValue
                        ? Option<int>.Some(row.DatetimePrecision.Value)
                        : Option<int>.None);

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);

                var sequenceName = !row.SequenceSchemaName.IsNullOrWhiteSpace() && !row.SequenceLocalName.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.SequenceSchemaName, row.SequenceLocalName))
                    : Option<Identifier>.None;

                // Both an identity column and a serial column draw their values from a sequence, so
                // the sequence's parameters describe either kind. A serial column is an ordinary
                // column defaulting to nextval() over an owned sequence, so an explicitly supplied
                // value is always accepted.
                var autoIncrement = sequenceName.IsSome && row.SequenceStart is long seqStart && row.SequenceIncrement is long seqIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(
                        seqStart,
                        seqIncrement,
                        string.Equals(row.IdentityKind, Constants.IdentityAlways, StringComparison.Ordinal)
                            ? IdentityGeneration.Always
                            : IdentityGeneration.ByDefault,
                        ToNumericBound(row.SequenceMinValue),
                        ToNumericBound(row.SequenceMaxValue),
                        row.SequenceCycle == true,
                        sequenceName))
                    : Option<IAutoIncrement>.None;

                var defaultValue = PostgreSqlDefaultValueParser.Parse(row.ColumnDefault);
                var isNullable = string.Equals(row.IsNullable, Constants.Yes, StringComparison.Ordinal);

                var isComputed = !row.GenerationKind.IsNullOrEmpty();
                var computedDefinition = isComputed
                    ? Option<string>.Some(row.GenerationExpression ?? string.Empty)
                    : Option<string>.None;

                // Generated columns were stored-only until virtual generated columns arrived, so a
                // server that does not report the kind can only have stored ones.
                var computedStorage = string.Equals(row.GenerationKind, Constants.VirtualGenerated, StringComparison.Ordinal)
                    ? ComputedColumnStorage.Virtual
                    : ComputedColumnStorage.Stored;

                // PostgreSQL has no way to declare a column invisible, so every column a table
                // reports is one that SELECT * returns.
                return new DatabaseColumn(
                    columnName,
                    columnType,
                    isNullable,
                    defaultValue,
                    autoIncrement,
                    isComputed,
                    computedDefinition,
                    computedStorage);
            })
            .ToListAsync(cancellationToken);
    }

    private static Option<decimal> ToNumericBound(long? value)
    {
        return value.HasValue
            ? Option<decimal>.Some(value.Value)
            : Option<decimal>.None;
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

        return PostgreSqlCatalogMapper.MapTriggers(queryResult);
    }

    /// <summary>
    /// Creates a column lookup, keyed by the column's name.
    /// </summary>
    /// <param name="columns">Columns to create a lookup from.</param>
    /// <returns>A dictionary whose keys are column names, and the values are the columns associated with those names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" />.</exception>
    protected static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

        foreach (var column in columns)
        {
            if (column.Name != null)
                result[column.Name] = column;
        }

        return result;
    }

    // Resolves a sequence of column names against a lookup, preserving order and silently skipping any
    // name that has no corresponding column.
    private static IEnumerable<IDatabaseColumn> ResolveColumns(IEnumerable<Identifier> columnNames, IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup)
    {
        foreach (var name in columnNames)
        {
            if (columnLookup.TryGetValue(name, out var column))
                yield return column;
        }
    }

    private static ConstraintDeferrability GetDeferrability(bool isDeferrable, bool isInitiallyDeferred)
    {
        if (!isDeferrable)
            return ConstraintDeferrability.NotDeferrable;

        return isInitiallyDeferred
            ? ConstraintDeferrability.DeferrableInitiallyDeferred
            : ConstraintDeferrability.DeferrableInitiallyImmediate;
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseKey> GetDatabaseKeyLookup(IReadOnlyCollection<IDatabaseKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<Identifier, IDatabaseKey>(keys.Count);

        // Key constructors keep only the local name, so names are used as keys without rebuilding them.
        foreach (var key in keys)
        {
            var name = key.Name.MatchUnsafe(static n => n, static () => (Identifier?)null);
            if (name != null)
                result[name] = key;
        }

        return result;
    }

    // Builds a lookup of unique indexes by name. Used to resolve foreign keys that reference a unique index which
    // may not appear in a table's unique keys at all. This looks up such indexes by name so they can still be
    // resolved to a key.
    private static IReadOnlyDictionary<Identifier, IDatabaseIndex> GetUniqueIndexLookup(IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        ArgumentNullException.ThrowIfNull(indexes);

        var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

        // Index constructors keep only the local name, so names are used as keys without rebuilding them.
        foreach (var index in indexes)
        {
            if (index.IsUnique)
                result[index.Name] = index;
        }

        return result;
    }

    // Synthesizes a key from a unique index so that a foreign key referencing a bare unique index (i.e. one with
    // no backing UNIQUE constraint) can still be represented as an IDatabaseKey.
    private static IDatabaseKey CreateKeyFromUniqueIndex(IDatabaseIndex uniqueIndex)
    {
        var columns = uniqueIndex.Columns.SelectMany(static ic => ic.DependentColumns).ToList();
        return new PostgreSqlDatabaseKey(uniqueIndex.Name, DatabaseKeyType.Unique, columns);
    }

    /// <summary>
    /// A mapping from the referential actions as described in PostgreSQL, to a <see cref="ReferentialAction"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<string, ReferentialAction>(StringComparer.Ordinal)
    {
        ["a"] = ReferentialAction.NoAction,
        ["r"] = ReferentialAction.Restrict,
        ["c"] = ReferentialAction.Cascade,
        ["n"] = ReferentialAction.SetNull,
        ["d"] = ReferentialAction.SetDefault,
    };

    /// <summary>
    /// A mapping from the foreign key match types as described in PostgreSQL, to a <see cref="ForeignKeyMatchType"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, ForeignKeyMatchType> MatchTypeMapping { get; } = new Dictionary<string, ForeignKeyMatchType>(StringComparer.Ordinal)
    {
        ["s"] = ForeignKeyMatchType.Simple,
        ["p"] = ForeignKeyMatchType.Partial,
        ["f"] = ForeignKeyMatchType.Full,
    };

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
    /// A set of constants used to test results of queries.
    /// </summary>
    protected static class Constants
    {
        /// <summary>
        /// The built-in system schema.
        /// </summary>
        public const string PgCatalog = "pg_catalog";

        /// <summary>
        /// Determines whether a key type is a primary key.
        /// </summary>
        public const string PrimaryKeyType = "p";

        /// <summary>
        /// Determines whether a key type is a unique key.
        /// </summary>
        public const string UniqueKeyType = "u";

        /// <summary>
        /// Some queries return yes/no, this handles the yes case.
        /// </summary>
        public const string Yes = "YES";

        /// <summary>
        /// The <c>pg_attribute.attidentity</c> value given to an identity column that is generated always.
        /// </summary>
        public const string IdentityAlways = "a";

        /// <summary>
        /// The <c>pg_attribute.attgenerated</c> value given to a generated column that is computed on read.
        /// </summary>
        public const string VirtualGenerated = "v";
    }

    /// <summary>
    /// A query cache provider for PostgreSQL tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class PostgreSqlTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, PostgreSqlTableQueryCache> _columnLookups;
        private readonly AsyncCache<Identifier, TableKeys, PostgreSqlTableQueryCache> _keys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, PostgreSqlTableQueryCache> _indexes;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache> _foreignKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="columnLookupLoader">A column lookup cache.</param>
        /// <param name="keyLoader">A primary and unique key cache.</param>
        /// <param name="indexLoader">An index cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="columnLookupLoader"/>, <paramref name="keyLoader"/>, <paramref name="indexLoader"/> or <paramref name="foreignKeyLoader"/> are <see langword="null" />.</exception>
        public PostgreSqlTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache> columnLoader,
            AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, PostgreSqlTableQueryCache> columnLookupLoader,
            AsyncCache<Identifier, TableKeys, PostgreSqlTableQueryCache> keyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, PostgreSqlTableQueryCache> indexLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache> foreignKeyLoader
        )
        {
            _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _columnLookups = columnLookupLoader ?? throw new ArgumentNullException(nameof(columnLookupLoader));
            _keys = keyLoader ?? throw new ArgumentNullException(nameof(keyLoader));
            _indexes = indexLoader ?? throw new ArgumentNullException(nameof(indexLoader));
            _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
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
        /// Retrieves a table's column lookup from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A dictionary whose keys are column names, and the values are the columns associated with those names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _columnLookups.GetByKeyAsync(tableName, this, cancellationToken);
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
    }
}