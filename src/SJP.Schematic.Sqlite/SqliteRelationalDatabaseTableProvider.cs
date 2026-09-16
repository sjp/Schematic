using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using LanguageExt;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Sqlite.Exceptions;
using SJP.Schematic.Sqlite.Parsing;
using SJP.Schematic.Sqlite.Parsing.Antlr;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Sqlite.Pragma.Query;
using SJP.Schematic.Sqlite.Queries;
using StringHashSet = System.Collections.Generic.HashSet<string>;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A database table provider for SQLite.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class SqliteRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="pragma">A pragma for the given database connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="pragma"/> are <see langword="null" />.</exception>
    public SqliteRelationalDatabaseTableProvider(ISchematicConnection connection, ISqliteConnectionPragma pragma, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        ConnectionPragma = pragma ?? throw new ArgumentNullException(nameof(pragma));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

        // a failed load is not remembered, otherwise one transient error would break every later table load
        _dbVersion = new AsyncLazy<Version>(LoadDbVersionAsync, AsyncLazyFlags.RetryOnFailure);
    }

    /// <summary>
    /// A database connection that is specific to a given SQLite database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Accesses pragma that applies to the entire SQLite connection.
    /// </summary>
    /// <value>A connection pragma.</value>
    protected ISqliteConnectionPragma ConnectionPragma { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
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
    /// <remarks>
    /// The attached databases and each schema's table list are cached here rather than for the
    /// lifetime of the provider, because <c>ATTACH</c>, <c>DETACH</c> and DDL on the same connection
    /// can change them between calls.
    /// </remarks>
    protected SqliteTableQueryCache CreateQueryCache(CancellationToken cancellationToken) => new(
        _ => LoadDatabaseListAsync(cancellationToken),
        new AsyncCache<string, IReadOnlyDictionary<string, pragma_table_list>, SqliteTableQueryCache>((schema, _, token) => LoadTableListAsync(schema, token), cancellationToken),
        new AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache>(GetParsedTableDefinitionAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyList<pragma_table_xinfo>, SqliteTableQueryCache>(LoadTableXInfoAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache>(LoadColumnsAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, SqliteTableQueryCache>(LoadColumnLookupAsync, cancellationToken),
        new AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache>(LoadPrimaryKeyAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache>(LoadUniqueKeysAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache>(LoadParentKeysAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<pragma_index_list>, SqliteTableQueryCache>(LoadIndexListAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyList<pragma_foreign_key_list>, SqliteTableQueryCache>(LoadForeignKeyListAsync, cancellationToken),
        new AsyncCache<string, ILookup<string, Identifier>, SqliteTableQueryCache>(LoadChildTableLookupAsync, cancellationToken)
    );

    /// <summary>
    /// Enumerates all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async IAsyncEnumerable<IRelationalDatabaseTable> EnumerateAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache(cancellationToken);

        var dbNamesQuery = await queryCache.GetDatabaseListAsync(cancellationToken);
        var dbNames = dbNamesQuery
            .OrderBy(static d => d.seq)
            .Select(static d => d.name)
            .ToList();

        var qualifiedTableNames = new List<Identifier>();

        foreach (var dbName in dbNames)
        {
            var sql = GetAllTableNames.Sql(Dialect, dbName);
            var names = await DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(sql, cancellationToken)
                .Where(static result => !IsReservedTableName(result.TableName))
                .Select(result => Identifier.CreateQualifiedIdentifier(dbName, result.TableName))
                .ToListAsync(cancellationToken);

            qualifiedTableNames.AddRange(names);
        }

        var tableNames = (await FilterShadowTablesAsync(qualifiedTableNames, queryCache, cancellationToken))
            .OrderBy(static name => name.Schema, StringComparer.Ordinal)
            .ThenBy(static name => name.LocalName, StringComparer.Ordinal);

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

        var dbNamesQuery = await queryCache.GetDatabaseListAsync(cancellationToken);
        var dbNames = dbNamesQuery
            .OrderBy(static d => d.seq)
            .Select(static d => d.name)
            .ToList();

        var qualifiedTableNames = await dbNames
            .Select(dbName =>
            {
                var sql = GetAllTableNames.Sql(Dialect, dbName);
                return DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(sql, cancellationToken)
                    .Where(static result => !IsReservedTableName(result.TableName))
                    .Select(result => Identifier.CreateQualifiedIdentifier(dbName, result.TableName))
                    .ToListAsync(cancellationToken);
            })
            .ToArray()
            .WhenAll();

        var tableNames = (await FilterShadowTablesAsync(qualifiedTableNames.SelectMany(static tn => tn), queryCache, cancellationToken))
            .OrderBy(static name => name.Schema, StringComparer.Ordinal)
            .ThenBy(static name => name.LocalName, StringComparer.Ordinal)
            .ToArray();

        return await tableNames.SelectBoundedAsync(
            (tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct),
            Math.Max(1, DbConnection.MaxConcurrentQueries),
            cancellationToken);
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

        return GetTableAsyncCore(tableName, cancellationToken).ToAsync();
    }

    private async Task<Option<IRelationalDatabaseTable>> GetTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        if (IsReservedTableName(tableName))
            return Option<IRelationalDatabaseTable>.None;

        var queryCache = CreateQueryCache(cancellationToken);
        if (tableName.Schema != null)
            return await LoadTable(tableName, queryCache, cancellationToken).ToOption();

        var dbNamesResult = await queryCache.GetDatabaseListAsync(cancellationToken);
        var dbNames = dbNamesResult.OrderBy(static l => l.seq).Select(static l => l.name).ToList();
        foreach (var dbName in dbNames)
        {
            var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
            var table = LoadTable(qualifiedTableName, queryCache, cancellationToken);

            var tableIsSome = await table.IsSome;
            if (tableIsSome)
                return await table.ToOption();
        }

        return Option<IRelationalDatabaseTable>.None;
    }

    /// <summary>
    /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="tableName">A table name that will be resolved.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return GetResolvedTableNameAsyncCore(tableName, queryCache, cancellationToken).ToAsync();
    }

    private async Task<Option<Identifier>> GetResolvedTableNameAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (IsReservedTableName(tableName))
            return Option<Identifier>.None;

        if (tableName.Schema != null)
        {
            var sql = GetTableName.Sql(Dialect, tableName.Schema);
            var queryResult = await DbConnection.ExecuteScalarAsync(
                sql,
                new GetTableName.Query { TableName = tableName.LocalName },
                cancellationToken
            );

            if (queryResult != null)
            {
                var dbList = await queryCache.GetDatabaseListAsync(cancellationToken);
                var tableSchemaName = dbList
                    .OrderBy(static s => s.seq)
                    .Select(static s => s.name)
                    .FirstOrDefault(s => string.Equals(s, tableName.Schema, StringComparison.OrdinalIgnoreCase));
                if (tableSchemaName == null)
                    throw new InvalidOperationException("Unable to find a database matching the given schema name: " + tableName.Schema);

                var resolvedName = Identifier.CreateQualifiedIdentifier(tableSchemaName, queryResult);
                if (!await IsShadowTableAsync(resolvedName, queryCache, cancellationToken))
                    return Option<Identifier>.Some(resolvedName);
            }
        }

        var dbNamesResult = await queryCache.GetDatabaseListAsync(cancellationToken);
        var dbNames = dbNamesResult
            .OrderBy(static l => l.seq)
            .Select(static l => l.name)
            .ToList();
        foreach (var dbName in dbNames)
        {
            var sql = GetTableName.Sql(Dialect, dbName);
            var tableLocalName = await DbConnection.ExecuteScalarAsync(
                sql,
                new GetTableName.Query { TableName = tableName.LocalName },
                cancellationToken
            );

            if (tableLocalName != null)
            {
                var resolvedName = Identifier.CreateQualifiedIdentifier(dbName, tableLocalName);
                if (!await IsShadowTableAsync(resolvedName, queryCache, cancellationToken))
                    return Option<Identifier>.Some(resolvedName);
            }
        }

        return Option<Identifier>.None;
    }

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, CancellationToken cancellationToken)
        => LoadTable(tableName, CreateQueryCache(cancellationToken), cancellationToken);

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">The query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <see langword="null" />.</exception>
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, queryCache, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var (
            parsedTable,
            columns,
            triggers,
            primaryKey,
            uniqueKeys,
            indexes,
            parentKeys,
            childKeys,
            kind
        ) = await (
            queryCache.GetParsedTableAsync(tableName, cancellationToken),
            queryCache.GetColumnsAsync(tableName, cancellationToken),
            LoadTriggersAsync(tableName, queryCache, cancellationToken),
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            LoadIndexesAsync(tableName, queryCache, cancellationToken),
            queryCache.GetForeignKeysAsync(tableName, cancellationToken),
            LoadChildKeysAsync(tableName, queryCache, cancellationToken),
            LoadTableKindAsync(tableName, queryCache, cancellationToken)
        ).WhenAll();
        var checks = LoadChecks(parsedTable);

        return new RelationalDatabaseTable(
            tableName,
            columns,
            primaryKey,
            uniqueKeys,
            parentKeys,
            childKeys,
            indexes,
            checks,
            triggers,
            kind,
            // SQLite journals every write and has neither partitioning, system versioning, nor a
            // table-level collation, so a table's kind is the only storage fact it reports
            Option<ITablePartitioning>.None,
            Option<ITableSystemVersioning>.None,
            true,
            Option<Identifier>.None
        );
    }

    private async Task<TableKind> LoadTableKindAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var isTemporary = string.Equals(tableName.Schema, TempSchemaName, StringComparison.OrdinalIgnoreCase);
        var tableListEntry = await GetTableListEntryAsync(tableName, queryCache, cancellationToken);

        // a WITHOUT ROWID table is stored in the structure of its primary key index
        return tableListEntry != null && string.Equals(tableListEntry.type, VirtualTableType, StringComparison.OrdinalIgnoreCase)
            ? TableKind.Virtual
            : isTemporary
                ? TableKind.Temporary
                : tableListEntry?.wr == true
                    ? TableKind.IndexOrganized
                    : TableKind.Regular;
    }

    /// <summary>
    /// Retrieves the primary key for the given table, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A primary key, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return Option<IDatabaseKey>.None;
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var pkColumnNames = await LoadPrimaryKeyColumnNamesAsync(pragma, tableName, queryCache, cancellationToken);
        if (pkColumnNames.Count == 0)
            return Option<IDatabaseKey>.None;

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var keyColumns = new List<IDatabaseColumn>(pkColumnNames.Count);
        foreach (var pkColumnName in pkColumnNames)
        {
            if (columnLookup.TryGetValue(pkColumnName, out var keyColumn))
                keyColumns.Add(keyColumn);
        }

        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);

        var indexLists = await queryCache.GetIndexListAsync(tableName, cancellationToken);
        var pkIndexList = indexLists.FirstOrDefault(static i => string.Equals(i.origin, Constants.PrimaryKeyConstraint, StringComparison.Ordinal) && i.name != null);
        var backingIndex = pkIndexList != null
            ? await CreateConstraintIndexAsync(pragma, pkIndexList, columnLookup, cancellationToken)
            : Option<IDatabaseIndex>.None;

        var primaryKeyName = parsedTable.PrimaryKey.Bind(c => c.Name.Map(Identifier.CreateQualifiedIdentifier));
        var primaryKey = new SqliteDatabaseKey(primaryKeyName, DatabaseKeyType.Primary, keyColumns, backingIndex);

        return Option<IDatabaseKey>.Some(primaryKey);
    }

    // The primary key columns in key order. Where pragma table_xinfo is available the columns have
    // already read it, and its extra hidden and generated columns can never be part of the key.
    private async Task<IReadOnlyList<string>> LoadPrimaryKeyColumnNamesAsync(ISqliteDatabasePragma pragma, Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (await IsTableXInfoPragmaSupportedAsync())
        {
            var tableXInfos = await queryCache.GetTableXInfoAsync(tableName, cancellationToken);
            return tableXInfos
                .Where(static ti => ti.pk > 0)
                .OrderBy(static ti => ti.pk)
                .Select(static ti => ti.name)
                .ToList();
        }

        var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken);
        return tableInfos
            .Where(static ti => ti.pk > 0)
            .OrderBy(static ti => ti.pk)
            .Select(static ti => ti.name)
            .ToList();
    }

    /// <summary>
    /// Retrieves the index list pragma result for a given table, from the cache when available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of index list pragma results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<pragma_index_list>> LoadIndexListAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexListAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<pragma_index_list>> LoadIndexListAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var indexLists = await pragma.IndexListAsync(tableName, cancellationToken);
        return indexLists.ToList();
    }

    /// <summary>
    /// Retrieves indexes that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of indexes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var indexLists = await queryCache.GetIndexListAsync(tableName, cancellationToken);
        if (indexLists.Empty())
            return [];

        var nonConstraintIndexLists = indexLists.Where(static i => string.Equals(i.origin, Constants.CreateIndex, StringComparison.Ordinal)).ToList();
        if (nonConstraintIndexLists.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var indexDefinitions = await DbConnection.QueryAsync(
            GetTableIndexDefinitions.Sql(Dialect, tableName.Schema!),
            new GetTableIndexDefinitions.Query { TableName = tableName.LocalName },
            cancellationToken
        );
        var indexDefinitionLookup = indexDefinitions.ToDictionary(static d => d.IndexName, static d => d.Sql, StringComparer.Ordinal);

        var namedIndexLists = nonConstraintIndexLists.Where(static i => i.name != null).ToList();
        if (namedIndexLists.Empty())
            return [];

        var indexInfos = await namedIndexLists
            .Select(i => pragma.IndexXInfoAsync(i.name!, cancellationToken))
            .ToArray()
            .WhenAll();

        var result = new List<IDatabaseIndex>(namedIndexLists.Count);

        for (var idx = 0; idx < namedIndexLists.Count; idx++)
        {
            var indexList = namedIndexLists[idx];
            var indexInfo = indexInfos[idx];
            indexDefinitionLookup.TryGetValue(indexList.name, out var indexSchema);

            // the pragma does not report the expression behind a functional index column, so the
            // definitions are recovered from the index DDL and paired with the key columns by position
            var columnDefinitions = indexSchema != null
                ? GetIndexColumnDefinitions(indexSchema)
                : [];

            var keyColumnInfos = indexInfo
                .Where(static i => i.key)
                .OrderBy(static i => i.seqno)
                .ToList();

            var indexColumns = new List<IDatabaseIndexColumn>(keyColumnInfos.Count);
            for (var columnIndex = 0; columnIndex < keyColumnInfos.Count; columnIndex++)
            {
                var columnInfo = keyColumnInfos[columnIndex];
                var definition = columnIndex < columnDefinitions.Count ? columnDefinitions[columnIndex] : null;
                var indexColumn = CreateIndexColumn(columnInfo, definition, columnLookup);
                if (indexColumn != null)
                    indexColumns.Add(indexColumn);
            }

            if (indexColumns.Empty())
                continue;

            var includedColumnInfos = indexInfo
                .Where(static i => !i.key && i.cid >= 0 && i.name != null)
                .OrderBy(static i => i.name, StringComparer.Ordinal);

            var includedColumns = new List<IDatabaseColumn>();
            foreach (var includedColumnInfo in includedColumnInfos)
            {
                if (columnLookup.TryGetValue(includedColumnInfo.name!, out var includedColumn))
                    includedColumns.Add(includedColumn);
            }

            var filterDefinition = indexSchema != null
                ? GetIndexFilterDefinition(indexSchema)
                : Option<string>.None;

            var index = new SqliteDatabaseIndex(indexList.name, indexList.unique, indexColumns, includedColumns, filterDefinition);
            result.Add(index);
        }

        return result;
    }

    // A primary or unique key constraint is enforced by an automatically created index, which SQLite
    // reports through pragma index_list with an origin of 'pk' or 'u'.
    private async Task<Option<IDatabaseIndex>> CreateConstraintIndexAsync(
        ISqliteDatabasePragma pragma,
        pragma_index_list indexList,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        CancellationToken cancellationToken
    )
    {
        var indexInfo = await pragma.IndexXInfoAsync(indexList.name, cancellationToken);
        var indexColumns = indexInfo
            .Where(static i => i.key)
            .OrderBy(static i => i.seqno)
            .Select(i => CreateIndexColumn(i, null, columnLookup))
            .Where(static i => i != null)
            .Select(static i => i!)
            .ToList();

        return indexColumns.Count > 0
            ? Option<IDatabaseIndex>.Some(new SqliteDatabaseIndex(indexList.name, indexList.unique, indexColumns, [], Option<string>.None))
            : Option<IDatabaseIndex>.None;
    }

    // The rowid (cid = -1) is an implementation detail that SQLite appends to every index, so it is
    // not reported. An expression column (cid = -2) has no name in the pragma; its text comes from the
    // index DDL, and the columns it refers to are recovered by matching identifiers against the table.
    private IDatabaseIndexColumn? CreateIndexColumn(
        pragma_index_xinfo columnInfo,
        string? columnDefinition,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup
    )
    {
        var order = columnInfo.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
        var collation = !columnInfo.coll.IsNullOrWhiteSpace() && !string.Equals(columnInfo.coll, Constants.BinaryCollation, StringComparison.OrdinalIgnoreCase)
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(columnInfo.coll))
            : Option<Identifier>.None;

        if (columnInfo.cid >= 0 && columnInfo.name != null && columnLookup.TryGetValue(columnInfo.name, out var column))
            return new SqliteDatabaseIndexColumn(Dialect.QuoteName(column.Name), [column], order, collation);

        if (columnInfo.cid != ExpressionColumnId)
            return null;

        // an index whose definition could not be recovered still covers a column, so it is described
        // as an unknown expression rather than dropped, which would leave the index looking empty
        if (columnDefinition.IsNullOrWhiteSpace())
            return new SqliteDatabaseIndexColumn(UnknownExpression, [], order, collation);

        var dependentColumns = GetExpressionDependentColumns(columnDefinition, columnLookup);
        return new SqliteDatabaseIndexColumn(columnDefinition, dependentColumns, order, collation);
    }

    private static IReadOnlyCollection<IDatabaseColumn> GetExpressionDependentColumns(
        string expression,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup
    )
    {
        IReadOnlyCollection<Identifier> dependencies;
        try
        {
            dependencies = ExpressionDependencyProvider.GetDependencies(ExpressionObjectName, expression);
        }
        catch (ArgumentException)
        {
            return [];
        }

        var result = new List<IDatabaseColumn>();
        foreach (var dependency in dependencies)
        {
            if (columnLookup.TryGetValue(dependency.LocalName, out var column) && !result.Contains(column))
                result.Add(column);
        }

        return result;
    }

    // Splits the parenthesised column list of a CREATE INDEX statement on its top-level commas,
    // yielding one definition per key column, in the order that pragma index_xinfo reports them.
    private static IReadOnlyList<string> GetIndexColumnDefinitions(string indexSchema)
    {
        try
        {
            var tokens = SqliteLexing.GetSignificantTokens(indexSchema);

            var definitions = new List<string>();
            var depth = 0;
            var segmentStart = -1;

            for (var i = 0; i < tokens.Count; i++)
            {
                var tokenType = tokens[i].Type;
                if (tokenType == SQLiteLexer.OPEN_PAR)
                {
                    depth++;
                    if (depth == 1)
                        segmentStart = i + 1;
                }
                else if (tokenType == SQLiteLexer.CLOSE_PAR)
                {
                    depth--;
                    if (depth != 0)
                        continue;

                    AddColumnDefinition(definitions, indexSchema, tokens, segmentStart, i - 1);
                    return definitions;
                }
                else if (depth == 1 && tokenType == SQLiteLexer.COMMA)
                {
                    AddColumnDefinition(definitions, indexSchema, tokens, segmentStart, i - 1);
                    segmentStart = i + 1;
                }
            }

            return definitions;
        }
        catch (SqliteSyntaxErrorException)
        {
            // Unable to lex the index definition; no column definitions can be recovered from it.
            return [];
        }
    }

    private static void AddColumnDefinition(List<string> definitions, string indexSchema, IReadOnlyList<IToken> tokens, int startToken, int endToken)
    {
        // a column definition may be followed by COLLATE <name> and/or ASC | DESC, none of which
        // form part of the expression being indexed
        while (endToken >= startToken)
        {
            var tokenType = tokens[endToken].Type;
            if (tokenType is SQLiteLexer.ASC_ or SQLiteLexer.DESC_)
                endToken--;
            else if (endToken - 1 >= startToken && tokens[endToken - 1].Type == SQLiteLexer.COLLATE_)
                endToken -= 2;
            else
                break;
        }

        if (endToken < startToken)
            return;

        definitions.Add(indexSchema[tokens[startToken].StartIndex..(tokens[endToken].StopIndex + 1)]);
    }

    private static Option<string> GetIndexFilterDefinition(string indexSchema)
    {
        try
        {
            var tokens = SqliteLexing.GetSignificantTokens(indexSchema);

            // The filter expression is everything following the WHERE keyword.
            for (var i = 0; i < tokens.Count - 1; i++)
            {
                if (tokens[i].Type != SQLiteLexer.WHERE_)
                    continue;

                var definition = indexSchema[tokens[i + 1].StartIndex..];
                return !definition.IsNullOrWhiteSpace()
                    ? Option<string>.Some(definition)
                    : Option<string>.None;
            }

            return Option<string>.None;
        }
        catch (SqliteSyntaxErrorException)
        {
            // Unable to lex the index definition; treat it as having no filter expression.
            return Option<string>.None;
        }
    }

    /// <summary>
    /// Retrieves unique keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of unique keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var indexLists = await queryCache.GetIndexListAsync(tableName, cancellationToken);
        if (indexLists.Empty())
            return [];

        var ukIndexLists = indexLists
            .Where(static i => string.Equals(i.origin, Constants.UniqueConstraint, StringComparison.Ordinal) && i.unique && i.name != null)
            .ToList();
        if (ukIndexLists.Empty())
            return [];

        var result = new List<IDatabaseKey>(ukIndexLists.Count);

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);
        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);

        var parsedUniqueConstraints = parsedTable.UniqueKeys;

        var ukIndexXInfos = await ukIndexLists
            .Select(uk => pragma.IndexXInfoAsync(uk.name, cancellationToken))
            .ToArray()
            .WhenAll();

        for (var idx = 0; idx < ukIndexLists.Count; idx++)
        {
            var ukIndexList = ukIndexLists[idx];
            var indexXInfos = ukIndexXInfos[idx];
            var orderedColumns = indexXInfos
                .Where(i => i.key && i.cid >= 0 && i.name != null)
                .OrderBy(static i => i.seqno)
                .ToList();
            var columnNames = orderedColumns
                .ConvertAll(static i => i.name);
            var keyColumns = new List<IDatabaseColumn>(orderedColumns.Count);
            foreach (var orderedColumn in orderedColumns)
            {
                if (columnLookup.TryGetValue(orderedColumn.name!, out var keyColumn))
                    keyColumns.Add(keyColumn);
            }

            var parsedUniqueConstraint = parsedUniqueConstraints
                .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames, StringComparer.Ordinal));
            var uniqueConstraint = parsedUniqueConstraint != null
                ? Option<UniqueKey>.Some(parsedUniqueConstraint)
                : Option<UniqueKey>.None;
            var keyName = uniqueConstraint.Bind(uc => uc.Name.Map(Identifier.CreateQualifiedIdentifier));

            var backingIndexColumns = indexXInfos
                .Where(static i => i.key)
                .OrderBy(static i => i.seqno)
                .Select(i => CreateIndexColumn(i, null, columnLookup))
                .Where(static i => i != null)
                .Select(static i => i!)
                .ToList();
            var backingIndex = backingIndexColumns.Count > 0
                ? Option<IDatabaseIndex>.Some(new SqliteDatabaseIndex(ukIndexList.name, ukIndexList.unique, backingIndexColumns, [], Option<string>.None))
                : Option<IDatabaseIndex>.None;

            var uniqueKey = new SqliteDatabaseKey(keyName, DatabaseKeyType.Unique, keyColumns, backingIndex);
            result.Add(uniqueKey);
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
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        // schema name must match, no cross-schema FKs allowed
        var dbList = await queryCache.GetDatabaseListAsync(cancellationToken);
        var schemaName = dbList
            .OrderBy(static d => d.seq)
            .Select(static d => d.name)
            .FirstOrDefault(name => string.Equals(tableName.Schema, name, StringComparison.OrdinalIgnoreCase));
        if (schemaName == null)
            return [];

        var childTableLookup = await queryCache.GetChildTableLookupAsync(schemaName, cancellationToken);
        var result = new List<IDatabaseRelationalKey>();

        foreach (var childTableName in childTableLookup[tableName.LocalName])
        {
            var childTableParentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken);
            foreach (var parentKey in childTableParentKeys)
            {
                if (string.Equals(tableName.Schema, parentKey.ParentTable.Schema, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(tableName.LocalName, parentKey.ParentTable.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(parentKey);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Retrieves, for every table in a schema, the tables whose foreign keys name it as their parent.
    /// </summary>
    /// <param name="schemaName">A schema name, as reported by the database list.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A lookup from a parent table's local name, matched case-insensitively, to the names of its child tables.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="queryCache"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="schemaName"/> is <see langword="null" />, empty or whitespace.</exception>
    /// <remarks>
    /// SQLite has no reverse foreign key lookup, so the lookup is built once from the raw
    /// <c>pragma foreign_key_list</c> rows of every table. The parent names are as written in each
    /// constraint, so a child table found here may still reference a parent that does not exist;
    /// its fully loaded foreign keys decide whether a relationship is reported.
    /// </remarks>
    protected Task<ILookup<string, Identifier>> LoadChildTableLookupAsync(string schemaName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildTableLookupAsyncCore(schemaName, queryCache, cancellationToken);
    }

    private async Task<ILookup<string, Identifier>> LoadChildTableLookupAsyncCore(string schemaName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var sql = GetAllTableNames.Sql(Dialect, schemaName);
        var tableNames = await DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(sql, cancellationToken)
            .Where(static result => !IsReservedTableName(result.TableName))
            .Select(result => Identifier.CreateQualifiedIdentifier(schemaName, result.TableName))
            .ToListAsync(cancellationToken);
        var childTableNames = await FilterShadowTablesAsync(tableNames, queryCache, cancellationToken);

        var parentChildPairs = new List<(string ParentTableName, Identifier ChildTableName)>();
        var parentTableNames = new StringHashSet(StringComparer.OrdinalIgnoreCase);

        // a child table is listed once per parent, in table name order, however many of its
        // constraints reference that parent
        foreach (var childTableName in childTableNames)
        {
            var foreignKeyRows = await queryCache.GetForeignKeyListAsync(childTableName, cancellationToken);

            parentTableNames.Clear();
            foreach (var row in foreignKeyRows)
            {
                if (parentTableNames.Add(row.table))
                    parentChildPairs.Add((row.table, childTableName));
            }
        }

        return parentChildPairs.ToLookup(static pair => pair.ParentTableName, static pair => pair.ChildTableName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retrieves checks from parsed table information.
    /// </summary>
    /// <param name="parsedTable">Parsed table information.</param>
    /// <returns>A collection of check constraints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parsedTable"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IDatabaseCheckConstraint> LoadChecks(ParsedTableData parsedTable)
    {
        ArgumentNullException.ThrowIfNull(parsedTable);

        var checks = parsedTable.Checks.ToList();
        if (checks.Empty())
            return [];

        var result = new List<IDatabaseCheckConstraint>(checks.Count);

        foreach (var ck in checks)
        {
            var checkName = ck.Name.Map(Identifier.CreateQualifiedIdentifier);
            var check = new SqliteCheckConstraint(checkName, ck.Definition);
            result.Add(check);
        }

        return result;
    }

    /// <summary>
    /// Retrieves foreign keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var queryResult = await queryCache.GetForeignKeyListAsync(tableName, cancellationToken);
        if (queryResult.Empty())
            return [];

        var foreignKeys = queryResult.GroupAsDictionary(static row => new
        {
            ForeignKeyId = row.id,
            ParentTableName = row.table,
            OnDelete = row.on_delete,
            OnUpdate = row.on_update,
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);
        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            Identifier? parentTableName = null;
            var rows = fkey.Value.OrderBy(static row => row.seq).ToList();
            var hasImplicitParentColumns = rows.Any(static row => row.to == null);
            await GetResolvedParentTableNameAsync(tableName.Schema!, fkey.Key.ParentTableName, queryCache, cancellationToken)
                .ToAsync()
                .BindAsync(async name =>
                {
                    parentTableName = name; // required for later binding

                    var parentPrimaryKey = await queryCache.GetPrimaryKeyAsync(name, cancellationToken);

                    // the pragma reports a null parent column when the constraint omitted the parent
                    // column list, which in SQLite refers to the parent table's primary key. Taking the
                    // primary key's own columns keeps the parent column set accurate; a primary key of a
                    // different size cannot satisfy the constraint so no relationship is reported.
                    if (hasImplicitParentColumns)
                        return parentPrimaryKey.Filter(pk => pk.Columns.Count == rows.Count).ToAsync();

                    var parentTableColumnLookup = await queryCache.GetColumnLookupAsync(name, cancellationToken);

                    var parentColumns = new List<IDatabaseColumn>(rows.Count);
                    foreach (var row in rows)
                    {
                        if (parentTableColumnLookup.TryGetValue(row.to!, out var parentColumn))
                            parentColumns.Add(parentColumn);
                    }

                    var pkColumnsEqual = parentPrimaryKey
                        .Match(
                            k => k.Columns.Select(static col => col.Name).SequenceEqual(parentColumns.Select(static col => col.Name)),
                            static () => false
                        );
                    if (pkColumnsEqual)
                        return parentPrimaryKey.ToAsync();

                    var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(name, cancellationToken);
                    var parentUniqueKey = parentUniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(static ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(static pc => pc.Name)));
                    return parentUniqueKey != null
                        ? OptionAsync<IDatabaseKey>.Some(parentUniqueKey)
                        : OptionAsync<IDatabaseKey>.None;
                })
                .Map(key =>
                {
                    var childColumnNames = rows.Select(static row => row.from).ToList();

                    // don't need to check for the parent schema as cross-schema references are not supported.
                    // A constraint that omitted its parent columns has none to match on, so the child
                    // column list identifies it in the parsed definition instead.
                    var parsedConstraint = parsedTable.ParentKeys
                        .FirstOrDefault(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase)
                            && (hasImplicitParentColumns
                                ? fkc.ParentColumns.Empty() && fkc.Columns.SequenceEqual(childColumnNames, StringComparer.OrdinalIgnoreCase)
                                : fkc.ParentColumns.SequenceEqual(rows.Select(static row => row.to!), StringComparer.OrdinalIgnoreCase)));
                    var parsedConstraintOption = parsedConstraint != null
                        ? Option<ForeignKey>.Some(parsedConstraint)
                        : Option<ForeignKey>.None;

                    var childKeyName = parsedConstraintOption.Bind(fk => fk.Name.Map(Identifier.CreateQualifiedIdentifier));
                    var childKeyColumns = new List<IDatabaseColumn>(rows.Count);
                    foreach (var row in rows)
                    {
                        if (columnLookup.TryGetValue(row.from, out var childKeyColumn))
                            childKeyColumns.Add(childKeyColumn);
                    }

                    // the pragma reports neither DEFERRABLE nor MATCH, so both are read from the
                    // parsed CREATE TABLE definition when the constraint could be matched to one
                    var deferrability = parsedConstraintOption.Match(static fk => fk.Deferrability, static () => ConstraintDeferrability.NotDeferrable);
                    var matchType = parsedConstraintOption.Match(static fk => fk.MatchType, static () => ForeignKeyMatchType.Simple);

                    var childKey = new SqliteDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, Option<IDatabaseIndex>.None, deferrability);

                    var deleteAction = GetReferentialAction(fkey.Key.OnDelete);
                    var updateAction = GetReferentialAction(fkey.Key.OnUpdate);

                    return new DatabaseRelationalKey(tableName, childKey, parentTableName!, key, deleteAction, updateAction, matchType, []);
                })
                .IfSome(result.Add);
        }

        return result;
    }

    /// <summary>
    /// Resolves the table that a foreign key refers to. SQLite only looks for a foreign key's parent
    /// table in the schema of the child table, so no other attached database is searched, and a
    /// parent that does not exist resolves to nothing.
    /// </summary>
    private async Task<Option<Identifier>> GetResolvedParentTableNameAsync(string schemaName, string parentTableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var candidateName = Identifier.CreateQualifiedIdentifier(schemaName, parentTableName);
        if (IsReservedTableName(candidateName))
            return Option<Identifier>.None;

        if (await IsTableListPragmaSupportedAsync())
        {
            // the table list is already loaded to detect shadow tables, so no query is needed
            var tableList = await queryCache.GetTableListAsync(schemaName, cancellationToken);
            return tableList.TryGetValue(parentTableName, out var entry)
                && !string.Equals(entry.type, ViewType, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.type, ShadowTableType, StringComparison.OrdinalIgnoreCase)
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(schemaName, entry.name))
                    : Option<Identifier>.None;
        }

        var sql = GetTableName.Sql(Dialect, schemaName);
        var resolvedLocalName = await DbConnection.ExecuteScalarAsync(
            sql,
            new GetTableName.Query { TableName = parentTableName },
            cancellationToken
        );

        return resolvedLocalName != null
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(schemaName, resolvedLocalName))
            : Option<Identifier>.None;
    }

    /// <summary>
    /// Retrieves the foreign key list pragma result for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The foreign key list pragma rows for the table.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyList<pragma_foreign_key_list>> LoadForeignKeyListAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadForeignKeyListAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyList<pragma_foreign_key_list>> LoadForeignKeyListAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var foreignKeyList = await pragma.ForeignKeyListAsync(tableName, cancellationToken);
        return foreignKeyList.ToList();
    }

    /// <summary>
    /// Retrieves the columns for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadColumnsAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        return await IsTableXInfoPragmaSupportedAsync()
            ? await LoadAllColumnsAsync(tableName, queryCache, cancellationToken)
            : await LoadPhysicalColumnsAsync(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadAllColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var tableInfos = await queryCache.GetTableXInfoAsync(tableName, cancellationToken);
        if (tableInfos.Count == 0)
            return [];

        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);

        var result = new List<IDatabaseColumn>();
        var parsedColumns = GetParsedColumnLookup(parsedTable);
        var rowidAliasColumnName = GetRowidAliasColumnName(parsedTable);

        foreach (var tableInfo in tableInfos)
        {
            if (tableInfo.name == null)
                continue;

            // a virtual table has no parsed definition, so its columns are described by the pragma alone
            parsedColumns.TryGetValue(tableInfo.name, out var parsedColumnInfo);
            var columnTypeName = tableInfo.type;

            var affinity = AffinityParser.ParseTypeName(columnTypeName);
            var collation = parsedColumnInfo?.Collation ?? SqliteCollation.None;
            // a COLLATE clause only applies to a text column, so one parsed for any other affinity is dropped
            var columnType = collation == SqliteCollation.None || affinity != SqliteTypeAffinity.Text
                ? new SqliteColumnType(columnTypeName, affinity)
                : new SqliteColumnType(columnTypeName, affinity, collation);

            var isAutoIncrement = (parsedColumnInfo?.IsAutoIncrement ?? false)
                || string.Equals(rowidAliasColumnName, tableInfo.name, StringComparison.OrdinalIgnoreCase);
            var autoIncrement = isAutoIncrement
                ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None))
                : Option<IAutoIncrement>.None;
            // pragma table_info reports the text of the default, and the parsed definition says what
            // that text evaluates to
            var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                ? Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(tableInfo.dflt_value, parsedColumnInfo?.DefaultValueKind ?? DefaultValueKind.Unknown))
                : Option<IDatabaseDefaultValue>.None;

            var computedColumnType = parsedColumnInfo?.ComputedColumnType ?? SqliteGeneratedColumnType.None;
            var isComputed = computedColumnType != SqliteGeneratedColumnType.None;
            var computedStorage = computedColumnType == SqliteGeneratedColumnType.Stored
                ? ComputedColumnStorage.Stored
                : ComputedColumnStorage.Virtual;

            // the other hidden values describe a generated column, which SELECT * still expands to
            var isHidden = tableInfo.hidden == HiddenColumnType.Hidden;

            var column = new DatabaseColumn(
                tableInfo.name,
                columnType,
                !tableInfo.notnull,
                defaultValue,
                autoIncrement,
                isComputed,
                parsedColumnInfo?.ComputedDefinition,
                computedStorage,
                isHidden);
            result.Add(column);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the table extra info pragma result for a given table, which describes every column including hidden and generated ones.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table extra info pragma results, in column order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    /// <remarks>The pragma is only available from SQLite 3.31.0.</remarks>
    protected Task<IReadOnlyList<pragma_table_xinfo>> LoadTableXInfoAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadTableXInfoAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyList<pragma_table_xinfo>> LoadTableXInfoAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var tableInfos = await pragma.TableXInfoAsync(tableName, cancellationToken);
        return tableInfos.ToList();
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadPhysicalColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken);
        if (tableInfos.Empty())
            return [];

        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);

        var result = new List<IDatabaseColumn>();
        var parsedColumns = GetParsedColumnLookup(parsedTable);
        var rowidAliasColumnName = GetRowidAliasColumnName(parsedTable);

        foreach (var tableInfo in tableInfos)
        {
            if (tableInfo.name == null)
                continue;

            // a virtual table has no parsed definition, so its columns are described by the pragma alone
            parsedColumns.TryGetValue(tableInfo.name, out var parsedColumnInfo);
            var columnTypeName = tableInfo.type;

            var affinity = AffinityParser.ParseTypeName(columnTypeName);
            var collation = parsedColumnInfo?.Collation ?? SqliteCollation.None;
            // a COLLATE clause only applies to a text column, so one parsed for any other affinity is dropped
            var columnType = collation == SqliteCollation.None || affinity != SqliteTypeAffinity.Text
                ? new SqliteColumnType(columnTypeName, affinity)
                : new SqliteColumnType(columnTypeName, affinity, collation);

            var isAutoIncrement = (parsedColumnInfo?.IsAutoIncrement ?? false)
                || string.Equals(rowidAliasColumnName, tableInfo.name, StringComparison.OrdinalIgnoreCase);
            var autoIncrement = isAutoIncrement
                ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None))
                : Option<IAutoIncrement>.None;
            // pragma table_info reports the text of the default, and the parsed definition says what
            // that text evaluates to
            var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                ? Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(tableInfo.dflt_value, parsedColumnInfo?.DefaultValueKind ?? DefaultValueKind.Unknown))
                : Option<IDatabaseDefaultValue>.None;

            var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, defaultValue, autoIncrement);
            result.Add(column);
        }

        return result;
    }

    // The parsed columns of a table, keyed the way a pragma's column names are matched against them.
    // SQLite rejects a table that declares the same column twice, so a repeated name can only come from
    // a definition it would not accept; the first declaration wins, as a linear search would find it.
    private static Dictionary<string, Column> GetParsedColumnLookup(ParsedTableData parsedTable)
    {
        var result = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in parsedTable.Columns)
            result.TryAdd(column.Name, column);

        return result;
    }

    // A column declared as INTEGER PRIMARY KEY in a rowid table is an alias for the table's rowid,
    // and SQLite generates a value for it on insert exactly as it does for an AUTOINCREMENT column.
    // The keyword only additionally forbids reusing the ids of deleted rows, which is not something
    // the model describes, so both are reported as an auto-incrementing column.
    private static string? GetRowidAliasColumnName(ParsedTableData parsedTable)
    {
        if (parsedTable.IsWithoutRowId)
            return null;

        var primaryKey = parsedTable.PrimaryKey.MatchUnsafe(static pk => pk, static () => (PrimaryKey?)null);
        if (primaryKey == null)
            return null;

        var pkColumns = primaryKey.Columns.ToList();
        if (pkColumns.Count != 1)
            return null;

        // PRIMARY KEY(x DESC) declared as a table constraint is not a rowid alias, while the same
        // ordering given as a column constraint is; the parser only records an ordering for the
        // table-constraint form, so requiring an ascending column covers both.
        var pkColumn = pkColumns[0];
        if (pkColumn.Name == null || pkColumn.ColumnOrder != IndexColumnOrder.Ascending)
            return null;

        var column = parsedTable.Columns
            .FirstOrDefault(c => string.Equals(c.Name, pkColumn.Name, StringComparison.OrdinalIgnoreCase));

        return column != null && string.Equals(column.TypeDefinition.Trim(), SqliteIntegerTypeName, StringComparison.OrdinalIgnoreCase)
            ? column.Name
            : null;
    }

    // Only a column whose declared type is exactly INTEGER aliases the rowid; any other type name
    // with integer affinity, e.g. BIGINT, does not.
    private const string SqliteIntegerTypeName = "INTEGER";

    /// <summary>
    /// Retrieves all triggers defined on a table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of triggers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadTriggersAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var triggerQuery = GetTriggerDefinition.Sql(Dialect, tableName.Schema!);
        var triggerInfos = DbConnection.QueryEnumerableAsync(
            triggerQuery,
            new GetTriggerDefinition.Query { TableName = tableName.LocalName },
            cancellationToken
        );

        var result = new List<IDatabaseTrigger>();

        await foreach (var triggerInfo in triggerInfos.WithCancellation(cancellationToken))
        {
            var triggerSql = triggerInfo.Sql;
            var parsedTrigger = _triggerParserCache.GetOrAdd(triggerSql, sql => new Lazy<ParsedTriggerData>(() =>
            {
                try
                {
                    return TriggerParser.Parse(sql);
                }
                catch (SqliteTriggerParsingException ex)
                {
                    throw new SqliteTriggerParsingException(tableName, sql, ex.Message);
                }
            })).Value;

            var trigger = new SqliteDatabaseTrigger(
                triggerInfo.Name,
                triggerSql,
                parsedTrigger.Timing,
                parsedTrigger.Event,
                parsedTrigger.Condition,
                parsedTrigger.UpdateColumns
            );
            result.Add(trigger);
        }

        return result;
    }

    private static async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> LoadColumnLookupAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken);
        return GetColumnLookup(columns);
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
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

    /// <summary>
    /// Gets the parsed table definition from a <c>CREATE TABLE</c> definition.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Parsed table data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<ParsedTableData> GetParsedTableDefinitionAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return GetParsedTableDefinitionAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<ParsedTableData> GetParsedTableDefinitionAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return ParsedTableData.Empty($"Table '{tableName.LocalName}' does not exist.");
            tableName = resolvedName;
        }

        var definitionQuery = GetTableDefinition.Sql(Dialect, tableName.Schema!);
        var tableSql = await DbConnection.ExecuteScalarAsync(
            definitionQuery,
            new GetTableDefinition.Query { TableName = tableName.LocalName },
            cancellationToken
        );

        // a virtual table is declared with CREATE VIRTUAL TABLE ... USING <module>(...), whose
        // arguments are the module's business rather than column definitions, so there is nothing
        // here for the CREATE TABLE parser to read
        var tableListEntry = await GetTableListEntryAsync(tableName, queryCache, cancellationToken);
        if (tableListEntry != null && string.Equals(tableListEntry.type, VirtualTableType, StringComparison.OrdinalIgnoreCase))
            return ParsedTableData.Empty(tableSql!);

        return _tableParserCache.GetOrAdd(tableSql!, sql => new Lazy<ParsedTableData>(() =>
        {
            try
            {
                return TableParser.Parse(sql);
            }
            catch (SqliteTableParsingException ex)
            {
                throw new SqliteTableParsingException(tableName, sql, ex.Message);
            }
        })).Value;
    }

    /// <summary>
    /// Retrieves a pragma that accesses and modifies a particular schema/database.
    /// </summary>
    /// <param name="schema">A schema name.</param>
    /// <returns>A database pragma.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="schema"/> is empty or whitespace.</exception>
    protected ISqliteDatabasePragma GetDatabasePragma(string schema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        return _dbPragmaCache.GetOrAdd(schema, s => new DatabasePragma(Connection, s));
    }

    /// <summary>
    /// Determines whether a table's name is a SQLite reserved table name.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <returns><see langword="true" /> if the table name is a reserved table name; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected static bool IsReservedTableName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return tableName.LocalName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase);
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
        return Identifier.CreateQualifiedIdentifier(schema, tableName.LocalName);
    }

    /// <summary>
    /// Retrieves a strongly typed referential action given a string definition from SQLite.
    /// </summary>
    /// <param name="pragmaUpdateAction">An update action from SQLite.</param>
    /// <returns>A referential action.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pragmaUpdateAction"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="pragmaUpdateAction"/> is empty or whitespace.</exception>
    protected static ReferentialAction GetReferentialAction(string pragmaUpdateAction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pragmaUpdateAction);

        return RelationalUpdateMapping.ContainsKey(pragmaUpdateAction)
            ? RelationalUpdateMapping[pragmaUpdateAction]
            : ReferentialAction.NoAction;
    }

    private Task<Version> LoadDbVersionAsync() => new SqliteDatabaseProvider(Connection).GetDatabaseVersionAsync();

    /// <summary>
    /// Retrieves what <c>pragma table_list</c> reports for a table, if anything.
    /// </summary>
    private async Task<pragma_table_list?> GetTableListEntryAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, queryCache, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return null;
            tableName = resolvedName;
        }

        var tableList = await queryCache.GetTableListAsync(tableName.Schema!, cancellationToken);
        return tableList.GetValueOrDefault(tableName.LocalName);
    }

    // pragma table_list arrived in SQLite 3.37.0; on anything earlier nothing is known about a
    // table beyond its presence in sqlite_master.
    private async Task<IReadOnlyDictionary<string, pragma_table_list>> LoadTableListAsync(string schema, CancellationToken cancellationToken)
    {
        if (!await IsTableListPragmaSupportedAsync())
            return new Dictionary<string, pragma_table_list>(AsciiCaseInsensitiveStringComparer.Instance);

        var tableList = await GetDatabasePragma(schema).TableListAsync(cancellationToken);
        return tableList
            .GroupBy(static t => t.name, AsciiCaseInsensitiveStringComparer.Instance)
            .ToDictionary(static g => g.Key, static g => g.First(), AsciiCaseInsensitiveStringComparer.Instance);
    }

    private async Task<bool> IsTableListPragmaSupportedAsync()
    {
        var version = await _dbVersion.Task;
        return version >= TableListPragmaVersion;
    }

    private async Task<bool> IsTableXInfoPragmaSupportedAsync()
    {
        var version = await _dbVersion.Task;
        return version >= TableXInfoPragmaVersion;
    }

    /// <summary>
    /// Determines whether a table is a shadow table, i.e. one of the internal tables that backs a
    /// virtual table. Shadow tables are an implementation detail of the virtual table they belong
    /// to, so they are not reported as tables in their own right.
    /// </summary>
    private static async Task<bool> IsShadowTableAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var tableList = await queryCache.GetTableListAsync(tableName.Schema!, cancellationToken);
        var entry = tableList.GetValueOrDefault(tableName.LocalName);
        return entry != null && string.Equals(entry.type, ShadowTableType, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<IReadOnlyList<Identifier>> FilterShadowTablesAsync(IEnumerable<Identifier> tableNames, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var result = new List<Identifier>();
        foreach (var tableName in tableNames)
        {
            if (!await IsShadowTableAsync(tableName, queryCache, cancellationToken))
                result.Add(tableName);
        }

        return result;
    }

    /// <summary>
    /// Loads the list of databases attached to the current connection.
    /// </summary>
    private async Task<IReadOnlyList<pragma_database_list>> LoadDatabaseListAsync(CancellationToken cancellationToken)
    {
        var databaseList = await ConnectionPragma.DatabaseListAsync(cancellationToken);
        return databaseList.ToList();
    }

    private readonly ConcurrentDictionary<string, Lazy<ParsedTableData>> _tableParserCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Lazy<ParsedTriggerData>> _triggerParserCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ISqliteDatabasePragma> _dbPragmaCache = new(StringComparer.Ordinal);

    private readonly AsyncLazy<Version> _dbVersion;

    private static readonly FrozenDictionary<string, ReferentialAction> RelationalUpdateMapping = new Dictionary<string, ReferentialAction>(StringComparer.OrdinalIgnoreCase)
    {
        ["NO ACTION"] = ReferentialAction.NoAction,
        ["RESTRICT"] = ReferentialAction.Restrict,
        ["SET NULL"] = ReferentialAction.SetNull,
        ["SET DEFAULT"] = ReferentialAction.SetDefault,
        ["CASCADE"] = ReferentialAction.Cascade,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly SqliteTypeAffinityParser AffinityParser = new();
    private static readonly SqliteTableParser TableParser = new();
    private static readonly SqliteTriggerParser TriggerParser = new();

    // pragma index_xinfo reports an expression index column with this cid, and the rowid with -1.
    private const int ExpressionColumnId = -2;

    private const string TempSchemaName = "temp";
    private const string VirtualTableType = "virtual";
    private const string ViewType = "view";
    private const string ShadowTableType = "shadow";

    private static readonly Version TableListPragmaVersion = new(3, 37, 0);
    private static readonly Version TableXInfoPragmaVersion = new(3, 31, 0);

    private const string UnknownExpression = "<unknown expression>";

    private static readonly Identifier ExpressionObjectName = Identifier.CreateQualifiedIdentifier("index_column");

    private static readonly SqliteDependencyProvider ExpressionDependencyProvider = new();

    private static class Constants
    {
        public const string BinaryCollation = "BINARY";

        public const string CreateIndex = "c";

        public const string PrimaryKeyConstraint = "pk";

        public const string UniqueConstraint = "u";
    }

    /// <summary>
    /// A query cache provider for SQLite tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class SqliteTableQueryCache
    {
        private readonly AsyncLazy<IReadOnlyList<pragma_database_list>> _databaseList;
        private readonly AsyncCache<string, IReadOnlyDictionary<string, pragma_table_list>, SqliteTableQueryCache> _tableLists;
        private readonly AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache> _parsedTables;
        private readonly AsyncCache<Identifier, IReadOnlyList<pragma_table_xinfo>, SqliteTableQueryCache> _tableXInfos;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, SqliteTableQueryCache> _columnLookups;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache> _foreignKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<pragma_index_list>, SqliteTableQueryCache> _indexLists;
        private readonly AsyncCache<Identifier, IReadOnlyList<pragma_foreign_key_list>, SqliteTableQueryCache> _foreignKeyLists;
        private readonly AsyncCache<string, ILookup<string, Identifier>, SqliteTableQueryCache> _childTableLookups;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteTableQueryCache"/> class.
        /// </summary>
        /// <param name="databaseListLoader">Loads the databases attached to the connection.</param>
        /// <param name="tableListLoader">A cache of table list pragma results, keyed by schema name.</param>
        /// <param name="parsedTableLoader">A table parsing result cache.</param>
        /// <param name="tableXInfoLoader">A table extra info pragma cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="columnLookupLoader">A cache of column lookups, keyed by table name.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <param name="indexListLoader">An index list pragma cache.</param>
        /// <param name="foreignKeyListLoader">A foreign key list pragma cache.</param>
        /// <param name="childTableLookupLoader">A cache of child table lookups, keyed by schema name.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="databaseListLoader"/>, <paramref name="tableListLoader"/>, <paramref name="parsedTableLoader"/>, <paramref name="tableXInfoLoader"/>, <paramref name="columnLoader"/>, <paramref name="columnLookupLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/>, <paramref name="foreignKeyLoader"/>, <paramref name="indexListLoader"/>, <paramref name="foreignKeyListLoader"/> or <paramref name="childTableLookupLoader"/> are <see langword="null" />.</exception>
        public SqliteTableQueryCache(
            Func<CancellationToken, Task<IReadOnlyList<pragma_database_list>>> databaseListLoader,
            AsyncCache<string, IReadOnlyDictionary<string, pragma_table_list>, SqliteTableQueryCache> tableListLoader,
            AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache> parsedTableLoader,
            AsyncCache<Identifier, IReadOnlyList<pragma_table_xinfo>, SqliteTableQueryCache> tableXInfoLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache> columnLoader,
            AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, SqliteTableQueryCache> columnLookupLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache> foreignKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<pragma_index_list>, SqliteTableQueryCache> indexListLoader,
            AsyncCache<Identifier, IReadOnlyList<pragma_foreign_key_list>, SqliteTableQueryCache> foreignKeyListLoader,
            AsyncCache<string, ILookup<string, Identifier>, SqliteTableQueryCache> childTableLookupLoader
        )
        {
            ArgumentNullException.ThrowIfNull(databaseListLoader);

            // shared by every caller in this context, so no one caller's token may cancel the load
            _databaseList = new AsyncLazy<IReadOnlyList<pragma_database_list>>(() => databaseListLoader(CancellationToken.None), AsyncLazyFlags.RetryOnFailure);
            _tableLists = tableListLoader ?? throw new ArgumentNullException(nameof(tableListLoader));
            _parsedTables = parsedTableLoader ?? throw new ArgumentNullException(nameof(parsedTableLoader));
            _tableXInfos = tableXInfoLoader ?? throw new ArgumentNullException(nameof(tableXInfoLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _columnLookups = columnLookupLoader ?? throw new ArgumentNullException(nameof(columnLookupLoader));
            _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
            _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
            _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
            _indexLists = indexListLoader ?? throw new ArgumentNullException(nameof(indexListLoader));
            _foreignKeyLists = foreignKeyListLoader ?? throw new ArgumentNullException(nameof(foreignKeyListLoader));
            _childTableLookups = childTableLookupLoader ?? throw new ArgumentNullException(nameof(childTableLookupLoader));
        }

        /// <summary>
        /// Retrieves the databases attached to the connection from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The database list pragma results.</returns>
        public Task<IReadOnlyList<pragma_database_list>> GetDatabaseListAsync(CancellationToken cancellationToken)
        {
            return _databaseList.Task.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a schema's table list pragma results from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="schemaName">A schema name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The table list pragma results for the schema, keyed by table name, matched ignoring the case of ASCII letters as SQLite does.</returns>
        /// <exception cref="ArgumentException"><paramref name="schemaName"/> is <see langword="null" />, empty or whitespace.</exception>
        public Task<IReadOnlyDictionary<string, pragma_table_list>> GetTableListAsync(string schemaName, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

            return _tableLists.GetByKeyAsync(schemaName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's parsed definition from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The parsed definition of a table.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<ParsedTableData> GetParsedTableAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _parsedTables.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's extra info pragma result from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of table extra info pragma results, in column order.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyList<pragma_table_xinfo>> GetTableXInfoAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _tableXInfos.GetByKeyAsync(tableName, this, cancellationToken);
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
        /// Retrieves a lookup of a table's columns, keyed by column name, from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A lookup of columns, keyed by column name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        /// <remarks>The lookup is shared by every caller in this context, so it must not be modified.</remarks>
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

            return _primaryKeys.GetByKeyAsync(tableName, this, cancellationToken);
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

            return _uniqueKeys.GetByKeyAsync(tableName, this, cancellationToken);
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
        /// Retrieves a table's index list pragma result from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of index list pragma results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyCollection<pragma_index_list>> GetIndexListAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _indexLists.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's foreign key list pragma result from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign key list pragma results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyList<pragma_foreign_key_list>> GetForeignKeyListAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _foreignKeyLists.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a schema's child table lookup from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="schemaName">A schema name, as reported by the database list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A lookup from a parent table's local name to the names of the tables that reference it.</returns>
        /// <exception cref="ArgumentException"><paramref name="schemaName"/> is <see langword="null" />, empty or whitespace.</exception>
        public Task<ILookup<string, Identifier>> GetChildTableLookupAsync(string schemaName, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

            return _childTableLookups.GetByKeyAsync(schemaName, this, cancellationToken);
        }
    }
}