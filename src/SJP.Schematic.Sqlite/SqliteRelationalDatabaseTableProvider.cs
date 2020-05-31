using System;
using System.Collections.Concurrent;
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
using SJP.Schematic.Sqlite.Exceptions;
using SJP.Schematic.Sqlite.Parsing;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Sqlite.Query;

namespace SJP.Schematic.Sqlite
{
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
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="pragma"/> are <c>null</c>.</exception>
        public SqliteRelationalDatabaseTableProvider(ISchematicConnection connection, ISqliteConnectionPragma pragma, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            ConnectionPragma = pragma ?? throw new ArgumentNullException(nameof(pragma));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

            _dbVersion = new AsyncLazy<Version>(LoadDbVersionAsync);
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
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// The dialect for the associated database.
        /// </summary>
        /// <value>A database dialect.</value>
        protected IDatabaseDialect Dialect => Connection.Dialect;

        /// <summary>
        /// Creates a query cache for a given query context
        /// </summary>
        /// <returns>A query cache.</returns>
        protected SqliteTableQueryCache CreateQueryCache() => new SqliteTableQueryCache(
            new AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache>((tableName, _, token) => GetParsedTableDefinitionAsync(tableName, token)),
            new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache>(LoadColumnsAsync),
            new AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache>(LoadPrimaryKeyAsync),
            new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache>(LoadUniqueKeysAsync),
            new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache>(LoadParentKeysAsync)
        );

        /// <summary>
        /// Gets all database tables.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database tables.</returns>
        public virtual async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var dbNamesQuery = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbNamesQuery
                .OrderBy(d => d.seq)
                .Select(d => d.name)
                .ToList();

            var qualifiedTableNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = TablesQuery(dbName);
                var queryResult = await DbConnection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
                var names = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedTableNames.AddRange(names);
            }

            var tableNames = qualifiedTableNames
                .OrderBy(name => name.Schema)
                .ThenBy(name => name.LocalName);

            var queryCache = CreateQueryCache();
            foreach (var tableName in tableNames)
                yield return await LoadTableAsyncCore(tableName, queryCache, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a SQL query that retrieves the names of all tables in a given schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TablesQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' order by name";
        }

        /// <summary>
        /// Gets a database table.
        /// </summary>
        /// <param name="tableName">A database table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetTableAsyncCore(tableName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseTable>> GetTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (IsReservedTableName(tableName))
                return Option<IRelationalDatabaseTable>.None;

            if (tableName.Schema != null)
            {
                return await LoadTable(tableName, cancellationToken)
                    .ToOption()
                    .ConfigureAwait(false);
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = LoadTable(qualifiedTableName, cancellationToken);

                var tableIsSome = await table.IsSome.ConfigureAwait(false);
                if (tableIsSome)
                    return await table.ToOption().ConfigureAwait(false);
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        /// <summary>
        /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="tableName">A table name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameAsyncCore(tableName, cancellationToken).ToAsync();
        }

        private async Task<Option<Identifier>> GetResolvedTableNameAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (IsReservedTableName(tableName))
                return Option<Identifier>.None;

            if (tableName.Schema != null)
            {
                var sql = TableNameQuery(tableName.Schema);
                var tableLocalName = await DbConnection.ExecuteScalarAsync<string>(
                    sql,
                    new { TableName = tableName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                if (tableLocalName != null)
                {
                    var dbList = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
                    var tableSchemaName = dbList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, tableName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (tableSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + tableName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableSchemaName, tableLocalName));
                }
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbNamesResult
                .OrderBy(l => l.seq)
                .Select(l => l.name)
                .ToList();
            foreach (var dbName in dbNames)
            {
                var sql = TableNameQuery(dbName);
                var tableLocalName = await DbConnection.ExecuteScalarAsync<string>(
                    sql,
                    new { TableName = tableName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                if (tableLocalName != null)
                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, tableLocalName));
            }

            return Option<Identifier>.None;
        }

        /// <summary>
        /// Creates a SQL query that resolves a table name to its canonical form.
        /// </summary>
        /// <param name="schemaName">A schema name.</param>
        /// <returns>A SQL query</returns>
        /// <exception cref="ArgumentNullException"><paramref name="schemaName"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual string TableNameQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
        }

        /// <summary>
        /// Retrieves a table from the database, if available.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, CancellationToken cancellationToken)
            => LoadTable(tableName, CreateQueryCache(), cancellationToken);

        /// <summary>
        /// Retrieves a table from the database, if available.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">The query cache.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            var candidateTableName = QualifyTableName(tableName);
            return GetResolvedTableName(candidateTableName, cancellationToken)
                .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var parsedTableTask = queryCache.GetParsedTableAsync(tableName, cancellationToken);
            var columnsTask = queryCache.GetColumnsAsync(tableName, cancellationToken);
            var triggersTask = LoadTriggersAsync(tableName, cancellationToken);
            var primaryKeyTask = LoadPrimaryKeyAsync(tableName, queryCache, cancellationToken);
            var uniqueKeysTask = LoadUniqueKeysAsync(tableName, queryCache, cancellationToken);
            var indexesTask = LoadIndexesAsync(tableName, queryCache, cancellationToken);
            var parentKeysTask = queryCache.GetForeignKeysAsync(tableName, cancellationToken);
            var childKeysTask = LoadChildKeysAsync(tableName, queryCache, cancellationToken);

            await Task.WhenAll(parsedTableTask, columnsTask, triggersTask, primaryKeyTask, uniqueKeysTask, indexesTask, parentKeysTask, childKeysTask).ConfigureAwait(false);

            var parsedTable = await parsedTableTask.ConfigureAwait(false);
            var columns = await columnsTask.ConfigureAwait(false);
            var triggers = await triggersTask.ConfigureAwait(false);
            var primaryKey = await primaryKeyTask.ConfigureAwait(false);
            var uniqueKeys = await uniqueKeysTask.ConfigureAwait(false);
            var indexes = await indexesTask.ConfigureAwait(false);
            var parentKeys = await parentKeysTask.ConfigureAwait(false);
            var childKeys = await childKeysTask.ConfigureAwait(false);

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
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Option<IDatabaseKey>.None;
                tableName = resolvedName;
            }

            var pragma = GetDatabasePragma(tableName.Schema!);
            var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return Option<IDatabaseKey>.None;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Empty())
                return Option<IDatabaseKey>.None;

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var keyColumns = pkColumns
                .Where(c => columnLookup.ContainsKey(c.name))
                .Select(c => columnLookup[c.name])
                .ToList();

            var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken).ConfigureAwait(false);

            var primaryKeyName = parsedTable.PrimaryKey.Bind(c => c.Name.Map(Identifier.CreateQualifiedIdentifier));
            var primaryKey = new SqliteDatabaseKey(primaryKeyName, DatabaseKeyType.Primary, keyColumns);

            return Option<IDatabaseKey>.Some(primaryKey);
        }

        /// <summary>
        /// Retrieves indexes that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of indexes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Array.Empty<IDatabaseIndex>();
                tableName = resolvedName;
            }

            var pragma = GetDatabasePragma(tableName.Schema!);
            var indexLists = await pragma.IndexListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == Constants.CreateIndex).ToList();
            if (nonConstraintIndexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);
            var result = new List<IDatabaseIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                if (indexList.name == null)
                    continue;

                var indexInfo = await pragma.IndexXInfoAsync(indexList.name, cancellationToken).ConfigureAwait(false);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Where(i => i.name != null && columnLookup.ContainsKey(i.name))
                    .Select(i =>
                    {
                        var order = i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var column = columnLookup[i.name!];
                        var expression = Dialect.QuoteName(column.Name);
                        return new DatabaseIndexColumn(expression, column, order);
                    })
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key && i.cid >= 0 && i.name != null && columnLookup.ContainsKey(i.name))
                    .OrderBy(i => i.name)
                    .Select(i => columnLookup[i.name!])
                    .ToList();

                var index = new SqliteDatabaseIndex(indexList.name, indexList.unique, indexColumns, includedColumns);
                result.Add(index);
            }

            return result;
        }

        /// <summary>
        /// Retrieves unique keys that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of unique keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Array.Empty<IDatabaseKey>();
                tableName = resolvedName;
            }

            var pragma = GetDatabasePragma(tableName.Schema!);
            var indexLists = await pragma.IndexListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == Constants.UniqueConstraint
                    && i.unique
                    && i.name != null)
                .ToList();
            if (ukIndexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken).ConfigureAwait(false);

            var columnLookup = GetColumnLookup(columns);
            var parsedUniqueConstraints = parsedTable.UniqueKeys;

            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = await pragma.IndexXInfoAsync(ukIndexList.name, cancellationToken).ConfigureAwait(false);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0 && i.name != null)
                    .OrderBy(i => i.seqno)
                    .ToList();
                var columnNames = orderedColumns
                    .Select(i => i.name)
                    .ToList();
                var keyColumns = orderedColumns
                    .Where(i => columnLookup.ContainsKey(i.name!))
                    .Select(i => columnLookup[i.name!])
                    .ToList();

                var parsedUniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
                var uniqueConstraint = parsedUniqueConstraint != null
                    ? Option<UniqueKey>.Some(parsedUniqueConstraint)
                    : Option<UniqueKey>.None;
                var keyName = uniqueConstraint.Bind(uc => uc.Name.Map(Identifier.CreateQualifiedIdentifier));

                var uniqueKey = new SqliteDatabaseKey(keyName, DatabaseKeyType.Unique, keyColumns);
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
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Array.Empty<IDatabaseRelationalKey>();
                tableName = resolvedName;
            }

            var dbList = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbList
                .Where(d => string.Equals(tableName.Schema, d.name, StringComparison.OrdinalIgnoreCase)) // schema name must match, no cross-schema FKs allowed
                .OrderBy(d => d.seq)
                .Select(d => d.name)
                .ToList();

            var qualifiedChildTableNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = TablesQuery(dbName);
                var queryResult = await DbConnection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
                var tableNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedChildTableNames.AddRange(tableNames);
            }

            var result = new List<IDatabaseRelationalKey>();

            foreach (var childTableName in qualifiedChildTableNames)
            {
                var childTableParentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken).ConfigureAwait(false);
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
        /// <exception cref="ArgumentNullException"><paramref name="parsedTable"/> is <c>null</c>.</exception>
        protected virtual IReadOnlyCollection<IDatabaseCheckConstraint> LoadChecks(ParsedTableData parsedTable)
        {
            if (parsedTable == null)
                throw new ArgumentNullException(nameof(parsedTable));

            var checks = parsedTable.Checks.ToList();
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>(checks.Count);

            foreach (var ck in checks)
            {
                var startIndex = ck.Definition.First().Position.Absolute;
                var lastToken = ck.Definition.Last();
                var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                var definition = parsedTable.Definition[startIndex..endIndex];
                var checkName = ck.Name.Map(Identifier.CreateQualifiedIdentifier);
                var check = new SqliteCheckConstraint(checkName, definition);
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
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Array.Empty<IDatabaseRelationalKey>();
                tableName = resolvedName;
            }

            var pragma = GetDatabasePragma(tableName.Schema!);
            var queryResult = await pragma.ForeignKeyListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                ForeignKeyId = row.id,
                ParentTableName = row.table,
                OnDelete = row.on_delete,
                OnUpdate = row.on_update
            }).ToList();
            if (foreignKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(tableName.Schema, fkey.Key.ParentTableName);
                Identifier? parentTableName = null;
                await GetResolvedTableName(candidateParentTableName, cancellationToken)
                    .BindAsync(async name =>
                    {
                        parentTableName = name; // required for later binding

                        var parentTableColumns = await queryCache.GetColumnsAsync(name, cancellationToken).ConfigureAwait(false);
                        var parentTableColumnLookup = GetColumnLookup(parentTableColumns);

                        var rows = fkey.OrderBy(row => row.seq).ToList();
                        var parentColumns = rows
                            .Where(row => parentTableColumnLookup.ContainsKey(row.to))
                            .Select(row => parentTableColumnLookup[row.to])
                            .ToList();

                        var parentPrimaryKey = await queryCache.GetPrimaryKeyAsync(name, cancellationToken).ConfigureAwait(false);
                        var pkColumnsEqual = parentPrimaryKey
                            .Match(
                                k => k.Columns.Select(col => col.Name).SequenceEqual(parentColumns.Select(col => col.Name)),
                                () => false
                            );
                        if (pkColumnsEqual)
                            return parentPrimaryKey.ToAsync();

                        var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(name, cancellationToken).ConfigureAwait(false);
                        var parentUniqueKey = parentUniqueKeys.FirstOrDefault(uk =>
                            uk.Columns.Select(ukCol => ukCol.Name)
                                .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                        return parentUniqueKey != null
                            ? OptionAsync<IDatabaseKey>.Some(parentUniqueKey)
                            : OptionAsync<IDatabaseKey>.None;
                    })
                    .Map(key =>
                    {
                        var rows = fkey.OrderBy(row => row.seq).ToList();

                        // don't need to check for the parent schema as cross-schema references are not supported
                        var parsedConstraint = parsedTable.ParentKeys
                            .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                        var parsedConstraintOption = parsedConstraint != null
                            ? Option<ForeignKey>.Some(parsedConstraint)
                            : Option<ForeignKey>.None;

                        var childKeyName = parsedConstraintOption.Bind(fk => fk.Name.Map(Identifier.CreateQualifiedIdentifier));
                        var childKeyColumns = rows
                            .Where(row => columnLookup.ContainsKey(row.from))
                            .Select(row => columnLookup[row.from])
                            .ToList();

                        var childKey = new SqliteDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                        var deleteAction = GetReferentialAction(fkey.Key.OnDelete);
                        var updateAction = GetReferentialAction(fkey.Key.OnUpdate);

                        return new DatabaseRelationalKey(tableName, childKey, parentTableName!, key, deleteAction, updateAction);
                    })
                    .IfSome(key => result.Add(key))
                    .ConfigureAwait(false);
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
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <c>null</c>.</exception>
        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadColumnsAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var version = await _dbVersion.Task.ConfigureAwait(false);
            return version >= new Version(3, 31, 0)
                ? await LoadAllColumnsAsync(tableName, queryCache, cancellationToken).ConfigureAwait(false)
                : await LoadPhysicalColumnsAsync(tableName, queryCache, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadAllColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Array.Empty<IDatabaseColumn>();
                tableName = resolvedName;
            }

            var pragma = GetDatabasePragma(tableName.Schema!);
            var tableInfos = await pragma.TableXInfoAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();
            var parsedColumns = parsedTable.Columns;

            foreach (var tableInfo in tableInfos)
            {
                if (tableInfo.name == null)
                    continue;

                var parsedColumnInfo = parsedColumns.First(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                var affinity = AffinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                    : Option<IAutoIncrement>.None;
                var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                    ? Option<string>.Some(tableInfo.dflt_value)
                    : Option<string>.None;

                if (parsedColumnInfo.ComputedColumnType == SqliteGeneratedColumnType.None)
                {
                    var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, defaultValue, autoIncrement);
                    result.Add(column);
                }
                else
                {
                    var startIndex = parsedColumnInfo.ComputedDefinition.First().Position.Absolute;
                    var lastToken = parsedColumnInfo.ComputedDefinition.Last();
                    var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                    var definition = parsedTable.Definition[startIndex..endIndex];

                    var column = new DatabaseComputedColumn(tableInfo.name, columnType, !tableInfo.notnull, defaultValue, definition);
                    result.Add(column);
                }
            }

            return result;
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadPhysicalColumnsAsync(Identifier tableName, SqliteTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Array.Empty<IDatabaseColumn>();
                tableName = resolvedName;
            }

            var pragma = GetDatabasePragma(tableName.Schema!);
            var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var parsedTable = await queryCache.GetParsedTableAsync(tableName, cancellationToken).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();
            var parsedColumns = parsedTable.Columns;

            foreach (var tableInfo in tableInfos)
            {
                if (tableInfo.name == null)
                    continue;

                var parsedColumnInfo = parsedColumns.First(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                var affinity = AffinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                    : Option<IAutoIncrement>.None;
                var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                    ? Option<string>.Some(tableInfo.dflt_value)
                    : Option<string>.None;

                var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, defaultValue, autoIncrement);
                result.Add(column);
            }

            return result;
        }

        /// <summary>
        /// Retrieves all triggers defined on a table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of triggers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTriggersAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return Array.Empty<IDatabaseTrigger>();
                tableName = resolvedName;
            }

            var triggerQuery = TriggerDefinitionQuery(tableName.Schema!);
            var triggerInfos = await DbConnection.QueryAsync<SqliteMaster>(
                triggerQuery,
                new { TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var triggerSql = triggerInfo.sql;
                var parsedTrigger = _triggerParserCache.GetOrAdd(triggerSql, sql => new Lazy<ParsedTriggerData>(() =>
                {
                    var tokenizeResult = Tokenizer.TryTokenize(sql);
                    if (!tokenizeResult.HasValue)
                        throw new SqliteTriggerParsingException(tableName, triggerSql, tokenizeResult.ErrorMessage + " at " + tokenizeResult.ErrorPosition.ToString());

                    var tokens = tokenizeResult.Value;
                    return TriggerParser.ParseTokens(tokens);
                })).Value;

                var trigger = new SqliteDatabaseTrigger(triggerInfo.name, triggerSql, parsedTrigger.Timing, parsedTrigger.Event);
                result.Add(trigger);
            }

            return result;
        }

        /// <summary>
        /// Creates a SQL query that retrieves triggers for a given table.
        /// </summary>
        /// <param name="schema">A table's schema.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual string TriggerDefinitionQuery(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            return $"select * from { Dialect.QuoteIdentifier(schema) }.sqlite_master where type = 'trigger' and tbl_name = @TableName";
        }

        private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

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
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual Task<ParsedTableData> GetParsedTableDefinitionAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetParsedTableDefinitionAsyncCore(tableName, cancellationToken);
        }

        private async Task<ParsedTableData> GetParsedTableDefinitionAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName.Schema == null)
            {
                var resolvedName = await GetResolvedTableName(tableName, cancellationToken)
                    .MatchUnsafe(name => name, () => (Identifier?)null).ConfigureAwait(false);
                if (resolvedName == null)
                    return ParsedTableData.Empty($"Table '{ tableName.LocalName }' does not exist.");
                tableName = resolvedName;
            }

            var definitionQuery = TableDefinitionQuery(tableName.Schema!);
            var tableSql = await DbConnection.ExecuteScalarAsync<string>(
                definitionQuery,
                new { TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            return _tableParserCache.GetOrAdd(tableSql, sql => new Lazy<ParsedTableData>(() =>
            {
                var tokenizeResult = Tokenizer.TryTokenize(sql);
                if (!tokenizeResult.HasValue)
                    throw new SqliteTableParsingException(tableName, tableSql, tokenizeResult.ErrorMessage + " at " + tokenizeResult.ErrorPosition.ToString());

                var tokens = tokenizeResult.Value;
                return TableParser.ParseTokens(sql, tokens);
            })).Value;
        }

        /// <summary>
        /// Creates a query that retrieves a table's <c>CREATE TABLE</c> definition.
        /// </summary>
        /// <param name="schema">A schema name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual string TableDefinitionQuery(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            return $"select sql from { Dialect.QuoteIdentifier(schema) }.sqlite_master where type = 'table' and tbl_name = @TableName";
        }

        /// <summary>
        /// Retrieves a pragma that accesses and modifies a particular schema/database.
        /// </summary>
        /// <param name="schema">A schema name.</param>
        /// <returns>A database pragma.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual ISqliteDatabasePragma GetDatabasePragma(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            return _dbPragmaCache.GetOrAdd(schema, _ => new DatabasePragma(Connection, schema));
        }

        /// <summary>
        /// Determines whether a table's name is a SQLite reserved table name.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <returns><c>true</c> if the table name is a reserved table name; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected static bool IsReservedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return tableName.LocalName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Qualifies the name of a table, using known identifier defaults.
        /// </summary>
        /// <param name="tableName">A table name to qualify.</param>
        /// <returns>A table name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected Identifier QualifyTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var schema = tableName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(schema, tableName.LocalName);
        }

        /// <summary>
        /// Retrieves a strongly typed referential action given a string definition from SQLite.
        /// </summary>
        /// <param name="pragmaUpdateAction">An update action from SQLite.</param>
        /// <returns>A referential action.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pragmaUpdateAction"/> is <c>null</c>, empty or whitespace.</exception>
        protected static ReferentialAction GetReferentialAction(string pragmaUpdateAction)
        {
            if (pragmaUpdateAction.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(pragmaUpdateAction));

            return RelationalUpdateMapping.ContainsKey(pragmaUpdateAction)
                ? RelationalUpdateMapping[pragmaUpdateAction]
                : ReferentialAction.NoAction;
        }

        private Task<Version> LoadDbVersionAsync() => Dialect.GetDatabaseVersionAsync(Connection);

        private readonly ConcurrentDictionary<string, Lazy<ParsedTableData>> _tableParserCache = new ConcurrentDictionary<string, Lazy<ParsedTableData>>();
        private readonly ConcurrentDictionary<string, Lazy<ParsedTriggerData>> _triggerParserCache = new ConcurrentDictionary<string, Lazy<ParsedTriggerData>>();
        private readonly ConcurrentDictionary<string, ISqliteDatabasePragma> _dbPragmaCache = new ConcurrentDictionary<string, ISqliteDatabasePragma>();

        private readonly AsyncLazy<Version> _dbVersion;

        private static readonly IReadOnlyDictionary<string, ReferentialAction> RelationalUpdateMapping = new Dictionary<string, ReferentialAction>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = ReferentialAction.NoAction,
            ["RESTRICT"] = ReferentialAction.Restrict,
            ["SET NULL"] = ReferentialAction.SetNull,
            ["SET DEFAULT"] = ReferentialAction.SetDefault,
            ["CASCADE"] = ReferentialAction.Cascade
        };

        private static readonly SqliteTypeAffinityParser AffinityParser = new SqliteTypeAffinityParser();
        private static readonly SqliteTokenizer Tokenizer = new SqliteTokenizer();
        private static readonly SqliteTableParser TableParser = new SqliteTableParser();
        private static readonly SqliteTriggerParser TriggerParser = new SqliteTriggerParser();

        private static class Constants
        {
            public const string CreateIndex = "c";

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

            /// <summary>
            /// Initializes a new instance of the <see cref="SqliteTableQueryCache"/> class.
            /// </summary>
            /// <param name="parsedTableLoader">A table parsing result cache.</param>
            /// <param name="columnLoader">A column cache.</param>
            /// <param name="primaryKeyLoader">A primary key cache.</param>
            /// <param name="uniqueKeyLoader">A unique key cache.</param>
            /// <param name="foreignKeyLoader">A foreign key cache.</param>
            /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="parsedTableLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/> or <paramref name="foreignKeyLoader"/> are <c>null</c>.</exception>
            public SqliteTableQueryCache(
                AsyncCache<Identifier, ParsedTableData, SqliteTableQueryCache> parsedTableLoader,
                AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqliteTableQueryCache> columnLoader,
                AsyncCache<Identifier, Option<IDatabaseKey>, SqliteTableQueryCache> primaryKeyLoader,
                AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqliteTableQueryCache> uniqueKeyLoader,
                AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqliteTableQueryCache> foreignKeyLoader
            )
            {
                _parsedTables = parsedTableLoader ?? throw new ArgumentNullException(nameof(parsedTableLoader));
                _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
                _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
                _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
                _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
            }

            /// <summary>
            /// Retrieves a table's parsed definition from the cache, querying the database when not populated.
            /// </summary>
            /// <param name="tableName">A table name.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The parsed definition of a table.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
            public Task<ParsedTableData> GetParsedTableAsync(Identifier tableName, CancellationToken cancellationToken)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                return _parsedTables.GetByKeyAsync(tableName, this, cancellationToken);
            }

            /// <summary>
            /// Retrieves a table's columns from the cache, querying the database when not populated.
            /// </summary>
            /// <param name="tableName">A table name.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A collection of columns.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
            public Task<IReadOnlyList<IDatabaseColumn>> GetColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                return _columns.GetByKeyAsync(tableName, this, cancellationToken);
            }

            /// <summary>
            /// Retrieves a table's primary key from the cache, querying the database when not populated.
            /// </summary>
            /// <param name="tableName">A table name.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A primary key, if available.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
            public Task<Option<IDatabaseKey>> GetPrimaryKeyAsync(Identifier tableName, CancellationToken cancellationToken)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                return _primaryKeys.GetByKeyAsync(tableName, this, cancellationToken);
            }

            /// <summary>
            /// Retrieves a table's unique keys from the cache, querying the database when not populated.
            /// </summary>
            /// <param name="tableName">A table name.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A collection of unique keys.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
            public Task<IReadOnlyCollection<IDatabaseKey>> GetUniqueKeysAsync(Identifier tableName, CancellationToken cancellationToken)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                return _uniqueKeys.GetByKeyAsync(tableName, this, cancellationToken);
            }

            /// <summary>
            /// Retrieves a table's foreign keys from the cache, querying the database when not populated.
            /// </summary>
            /// <param name="tableName">A table name.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A collection of foreign keys.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
            public Task<IReadOnlyCollection<IDatabaseRelationalKey>> GetForeignKeysAsync(Identifier tableName, CancellationToken cancellationToken)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                return _foreignKeys.GetByKeyAsync(tableName, this, cancellationToken);
            }
        }
    }
}
