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

        _dbVersion = new AsyncLazy<Version>(LoadDbVersionAsync);
        _databaseList = new AsyncLazy<IReadOnlyList<pragma_database_list>>(LoadDatabaseListAsync);
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
    /// Creates a query cache for a given query context
    /// </summary>
    /// <returns>A query cache.</returns>
    protected SqliteTableQueryCache CreateQueryCache() => new(
        new AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache>((tableName, _, token) => GetParsedTableDefinitionAsync(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache>(LoadColumnsAsync),
        new AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache>(LoadPrimaryKeyAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache>(LoadUniqueKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache>(LoadParentKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<pragma_index_list>, SqliteTableQueryCache>(LoadIndexListAsync)
    );

    /// <summary>
    /// Enumerates all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async IAsyncEnumerable<IRelationalDatabaseTable> EnumerateAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var dbNamesQuery = await _databaseList.Task;
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

        var tableNames = qualifiedTableNames
            .OrderBy(static name => name.Schema, StringComparer.Ordinal)
            .ThenBy(static name => name.LocalName, StringComparer.Ordinal);

        var queryCache = CreateQueryCache();
        foreach (var tableName in tableNames)
            yield return await LoadTableAsyncCore(tableName, queryCache, cancellationToken);
    }

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default)
    {
        var dbNamesQuery = await _databaseList.Task;
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

        var tableNames = qualifiedTableNames
            .SelectMany(tn => tn)
            .OrderBy(static name => name.Schema, StringComparer.Ordinal)
            .ThenBy(static name => name.LocalName, StringComparer.Ordinal)
            .ToArray();

        var queryCache = CreateQueryCache();

        return await tableNames
            .Select(tableName => LoadTableAsyncCore(tableName, queryCache, cancellationToken))
            .ToArray()
            .WhenAll();
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

        if (tableName.Schema != null)
            return await LoadTable(tableName, cancellationToken).ToOption();

        var dbNamesResult = await _databaseList.Task;
        var dbNames = dbNamesResult.OrderBy(static l => l.seq).Select(static l => l.name).ToList();
        foreach (var dbName in dbNames)
        {
            var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
            var table = LoadTable(qualifiedTableName, cancellationToken);

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetResolvedTableNameAsyncCore(tableName, cancellationToken).ToAsync();
    }

    private async Task<Option<Identifier>> GetResolvedTableNameAsyncCore(Identifier tableName, CancellationToken cancellationToken)
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
                var dbList = await _databaseList.Task;
                var tableSchemaName = dbList
                    .OrderBy(static s => s.seq)
                    .Select(static s => s.name)
                    .FirstOrDefault(s => string.Equals(s, tableName.Schema, StringComparison.OrdinalIgnoreCase));
                if (tableSchemaName == null)
                    throw new InvalidOperationException("Unable to find a database matching the given schema name: " + tableName.Schema);

                return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableSchemaName, queryResult));
            }
        }

        var dbNamesResult = await _databaseList.Task;
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
                return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, tableLocalName));
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
        => LoadTable(tableName, CreateQueryCache(), cancellationToken);

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
        return GetResolvedTableName(candidateTableName, cancellationToken)
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
            childKeys
        ) = await (
            queryCache.GetParsedTableAsync(tableName, cancellationToken),
            queryCache.GetColumnsAsync(tableName, cancellationToken),
            LoadTriggersAsync(tableName, cancellationToken),
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            LoadIndexesAsync(tableName, queryCache, cancellationToken),
            queryCache.GetForeignKeysAsync(tableName, cancellationToken),
            LoadChildKeysAsync(tableName, queryCache, cancellationToken)
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
            triggers
        );
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
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return Option<IDatabaseKey>.None;
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken);
        if (tableInfos.Empty())
            return Option<IDatabaseKey>.None;

        var pkColumns = tableInfos
            .Where(static ti => ti.pk > 0)
            .OrderBy(static ti => ti.pk)
            .ToList();
        if (pkColumns.Empty())
            return Option<IDatabaseKey>.None;

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken);
        var columnLookup = GetColumnLookup(columns);

        var keyColumns = pkColumns
            .Where(c => columnLookup.ContainsKey(c.name))
            .Select(c => columnLookup[c.name])
            .ToList();

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

        return LoadIndexListAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<pragma_index_list>> LoadIndexListAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
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
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
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

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken);
        var columnLookup = GetColumnLookup(columns);

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

            var includedColumns = indexInfo
                .Where(i => !i.key && i.cid >= 0 && i.name != null && columnLookup.ContainsKey(i.name))
                .OrderBy(static i => i.name, StringComparer.Ordinal)
                .Select(i => columnLookup[i.name!])
                .ToList();

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

        return dependencies
            .Where(dependency => columnLookup.ContainsKey(dependency.LocalName))
            .Select(dependency => columnLookup[dependency.LocalName])
            .Distinct()
            .ToList();
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
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
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

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken);
        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);

        var columnLookup = GetColumnLookup(columns);
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
            var keyColumns = orderedColumns
                .Where(i => columnLookup.ContainsKey(i.name!))
                .Select(i => columnLookup[i.name!])
                .ToList();

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
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var dbList = await _databaseList.Task;
        var dbNames = dbList
            .Where(d => string.Equals(tableName.Schema, d.name, StringComparison.OrdinalIgnoreCase)) // schema name must match, no cross-schema FKs allowed
            .OrderBy(static d => d.seq)
            .Select(static d => d.name)
            .ToList();

        var qualifiedChildTableNames = new List<Identifier>();

        foreach (var dbName in dbNames)
        {
            var sql = GetAllTableNames.Sql(Dialect, dbName);
            var tableNames = await DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(sql, cancellationToken)
                .Where(static result => !IsReservedTableName(result.TableName))
                .Select(result => Identifier.CreateQualifiedIdentifier(dbName, result.TableName))
                .ToListAsync(cancellationToken);

            qualifiedChildTableNames.AddRange(tableNames);
        }

        var result = new List<IDatabaseRelationalKey>();

        foreach (var childTableName in qualifiedChildTableNames)
        {
            var childTableParentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken);
            var matchingParentKeys = childTableParentKeys
                .Where(fk => string.Equals(tableName.Schema, fk.ParentTable.Schema, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(tableName.LocalName, fk.ParentTable.LocalName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            result.AddRange(matchingParentKeys);
        }

        return result;
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
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var queryResult = await pragma.ForeignKeyListAsync(tableName, cancellationToken);
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

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken);
        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(tableName.Schema, fkey.Key.ParentTableName);
            Identifier? parentTableName = null;
            var rows = fkey.Value.OrderBy(static row => row.seq).ToList();
            var hasImplicitParentColumns = rows.Any(static row => row.to == null);
            await GetResolvedTableName(candidateParentTableName, cancellationToken)
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

                    var parentTableColumns = await queryCache.GetColumnsAsync(name, cancellationToken);
                    var parentTableColumnLookup = GetColumnLookup(parentTableColumns);

                    var parentColumns = rows
                        .Where(row => parentTableColumnLookup.ContainsKey(row.to!))
                        .Select(row => parentTableColumnLookup[row.to!])
                        .ToList();

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
                    var childKeyColumns = rows
                        .Where(row => columnLookup.ContainsKey(row.from))
                        .Select(row => columnLookup[row.from])
                        .ToList();

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
        var version = await _dbVersion.Task;
        return version >= new Version(3, 31, 0)
            ? await LoadAllColumnsAsync(tableName, queryCache, cancellationToken)
            : await LoadPhysicalColumnsAsync(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadAllColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                .MatchUnsafe(static name => name, static () => (Identifier?)null);
            if (resolvedName == null)
                return [];
            tableName = resolvedName;
        }

        var pragma = GetDatabasePragma(tableName.Schema!);
        var tableInfos = await pragma.TableXInfoAsync(tableName, cancellationToken);
        if (tableInfos.Empty())
            return [];

        var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken);

        var result = new List<IDatabaseColumn>();
        var parsedColumns = parsedTable.Columns;
        var rowidAliasColumnName = GetRowidAliasColumnName(parsedTable);

        foreach (var tableInfo in tableInfos)
        {
            if (tableInfo.name == null)
                continue;

            var parsedColumnInfo = parsedColumns.First(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
            var columnTypeName = tableInfo.type;

            var affinity = AffinityParser.ParseTypeName(columnTypeName);
            // a COLLATE clause only applies to a text column, so one parsed for any other affinity is dropped
            var columnType = parsedColumnInfo.Collation == SqliteCollation.None || affinity != SqliteTypeAffinity.Text
                ? new SqliteColumnType(columnTypeName, affinity)
                : new SqliteColumnType(columnTypeName, affinity, parsedColumnInfo.Collation);

            var isAutoIncrement = parsedColumnInfo.IsAutoIncrement
                || string.Equals(rowidAliasColumnName, tableInfo.name, StringComparison.OrdinalIgnoreCase);
            var autoIncrement = isAutoIncrement
                ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None))
                : Option<IAutoIncrement>.None;
            // pragma table_info reports the text of the default, and the parsed definition says what
            // that text evaluates to
            var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                ? Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(tableInfo.dflt_value, parsedColumnInfo.DefaultValueKind))
                : Option<IDatabaseDefaultValue>.None;

            var isComputed = parsedColumnInfo.ComputedColumnType != SqliteGeneratedColumnType.None;
            var computedStorage = parsedColumnInfo.ComputedColumnType == SqliteGeneratedColumnType.Stored
                ? ComputedColumnStorage.Stored
                : ComputedColumnStorage.Virtual;

            var column = new DatabaseColumn(
                tableInfo.name,
                columnType,
                !tableInfo.notnull,
                defaultValue,
                autoIncrement,
                isComputed,
                parsedColumnInfo.ComputedDefinition,
                computedStorage);
            result.Add(column);
        }

        return result;
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadPhysicalColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
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
        var parsedColumns = parsedTable.Columns;
        var rowidAliasColumnName = GetRowidAliasColumnName(parsedTable);

        foreach (var tableInfo in tableInfos)
        {
            if (tableInfo.name == null)
                continue;

            var parsedColumnInfo = parsedColumns.First(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
            var columnTypeName = tableInfo.type;

            var affinity = AffinityParser.ParseTypeName(columnTypeName);
            // a COLLATE clause only applies to a text column, so one parsed for any other affinity is dropped
            var columnType = parsedColumnInfo.Collation == SqliteCollation.None || affinity != SqliteTypeAffinity.Text
                ? new SqliteColumnType(columnTypeName, affinity)
                : new SqliteColumnType(columnTypeName, affinity, parsedColumnInfo.Collation);

            var isAutoIncrement = parsedColumnInfo.IsAutoIncrement
                || string.Equals(rowidAliasColumnName, tableInfo.name, StringComparison.OrdinalIgnoreCase);
            var autoIncrement = isAutoIncrement
                ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None))
                : Option<IAutoIncrement>.None;
            // pragma table_info reports the text of the default, and the parsed definition says what
            // that text evaluates to
            var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                ? Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(tableInfo.dflt_value, parsedColumnInfo.DefaultValueKind))
                : Option<IDatabaseDefaultValue>.None;

            var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, defaultValue, autoIncrement);
            result.Add(column);
        }

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
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
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

    /// <summary>
    /// Gets the parsed table definition from a <c>CREATE TABLE</c> definition.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Parsed table data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<ParsedTableData> GetParsedTableDefinitionAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetParsedTableDefinitionAsyncCore(tableName, cancellationToken);
    }

    private async Task<ParsedTableData> GetParsedTableDefinitionAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        if (tableName.Schema == null)
        {
            var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
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
    /// Loads the list of databases attached to the current connection. The result is cached for the
    /// lifetime of this provider instance, as it only changes as a result of an explicit
    /// <c>ATTACH</c>/<c>DETACH</c> against the underlying connection.
    /// </summary>
    private async Task<IReadOnlyList<pragma_database_list>> LoadDatabaseListAsync()
    {
        var databaseList = await ConnectionPragma.DatabaseListAsync();
        return databaseList.ToList();
    }

    private readonly ConcurrentDictionary<string, Lazy<ParsedTableData>> _tableParserCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Lazy<ParsedTriggerData>> _triggerParserCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ISqliteDatabasePragma> _dbPragmaCache = new(StringComparer.Ordinal);

    private readonly AsyncLazy<Version> _dbVersion;
    private readonly AsyncLazy<IReadOnlyList<pragma_database_list>> _databaseList;

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
        private readonly AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache> _parsedTables;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache> _foreignKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<pragma_index_list>, SqliteTableQueryCache> _indexLists;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteTableQueryCache"/> class.
        /// </summary>
        /// <param name="parsedTableLoader">A table parsing result cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <param name="indexListLoader">An index list pragma cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="parsedTableLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/>, <paramref name="foreignKeyLoader"/> or <paramref name="indexListLoader"/> are <see langword="null" />.</exception>
        public SqliteTableQueryCache(
            AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache> parsedTableLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache> columnLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache> foreignKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<pragma_index_list>, SqliteTableQueryCache> indexListLoader
        )
        {
            _parsedTables = parsedTableLoader ?? throw new ArgumentNullException(nameof(parsedTableLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
            _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
            _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
            _indexLists = indexListLoader ?? throw new ArgumentNullException(nameof(indexListLoader));
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
    }
}