using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

namespace SJP.Schematic.PostgreSql
{
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
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
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
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

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
        /// Creates a query cache for a given query context
        /// </summary>
        /// <returns>A query cache.</returns>
        protected PostgreSqlTableQueryCache CreateQueryCache() => new(
            new AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
            new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token)),
            new AsyncCache<Identifier, Option<IDatabaseKey>, PostgreSqlTableQueryCache>(LoadPrimaryKeyAsync),
            new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, PostgreSqlTableQueryCache>(LoadUniqueKeysAsync),
            new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache>(LoadParentKeysAsync)
        );

        /// <summary>
        /// Gets all database tables.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database tables.</returns>
        public virtual async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResults = await DbConnection.QueryAsync<GetTableNamesQueryResult>(TablesQuery, cancellationToken).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
                .Select(QualifyTableName);

            var queryCache = CreateQueryCache();
            foreach (var tableName in tableNames)
                yield return await LoadTableAsyncCore(tableName, queryCache, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// A SQL query that retrieves the names of all tables in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @$"
select
    schemaname as ""{ nameof(GetTableNamesQueryResult.SchemaName) }"",
    tablename as ""{ nameof(GetTableNamesQueryResult.TableName) }""
from pg_catalog.pg_tables
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, tablename";

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

            var queryCache = CreateQueryCache();
            var candidateTableName = QualifyTableName(tableName);
            return LoadTable(candidateTableName, queryCache, cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="tableName">A table name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected Task<Option<Identifier>> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

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
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedTableNameStrict(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            var qualifiedTableName = DbConnection.QueryFirstOrNone<GetTableNameQueryResult>(
                TableNameQuery,
                new GetTableNameQuery { SchemaName = candidateTableName.Schema!, TableName = candidateTableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.TableName));
        }

        /// <summary>
        /// A SQL query definition that resolves a table name for the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @$"
select
    schemaname as ""{ nameof(GetTableNameQueryResult.SchemaName) }"",
    tablename as ""{ nameof(GetTableNameQueryResult.TableName) }""
from pg_catalog.pg_tables
where schemaname = @{ nameof(GetTableNameQuery.SchemaName) } and tablename = @{ nameof(GetTableNameQuery.TableName) }
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

        /// <summary>
        /// Retrieves a table from the database, if available.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">The query cache.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            var candidateTableName = QualifyTableName(tableName);
            return GetResolvedTableName(candidateTableName, cancellationToken)
                .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var columnsTask = queryCache.GetColumnsAsync(tableName, cancellationToken);
            var checksTask = LoadChecksAsync(tableName, cancellationToken);
            var triggersTask = LoadTriggersAsync(tableName, cancellationToken);
            var primaryKeyTask = queryCache.GetPrimaryKeyAsync(tableName, cancellationToken);
            var uniqueKeysTask = queryCache.GetUniqueKeysAsync(tableName, cancellationToken);
            var indexesTask = LoadIndexesAsync(tableName, queryCache, cancellationToken);
            var parentKeysTask = queryCache.GetForeignKeysAsync(tableName, cancellationToken);
            var childKeysTask = LoadChildKeysAsync(tableName, queryCache, cancellationToken);

            await Task.WhenAll(columnsTask, checksTask, triggersTask, primaryKeyTask, uniqueKeysTask, indexesTask, parentKeysTask, childKeysTask).ConfigureAwait(false);

            var columns = await columnsTask.ConfigureAwait(false);
            var checks = await checksTask.ConfigureAwait(false);
            var triggers = await triggersTask.ConfigureAwait(false);
            var primaryKey = await primaryKeyTask.ConfigureAwait(false);
            var parentKeys = await parentKeysTask.ConfigureAwait(false);
            var indexes = await indexesTask.ConfigureAwait(false);
            var uniqueKeys = await uniqueKeysTask.ConfigureAwait(false);
            var childKeys = await childKeysTask.ConfigureAwait(false);

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
        protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var primaryKeyColumns = await DbConnection.QueryAsync<GetTablePrimaryQueryResult>(
                PrimaryKeyQuery,
                new GetTablePrimaryKeyQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (primaryKeyColumns.Empty())
                return Option<IDatabaseKey>.None;

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var groupedByName = primaryKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            if (constraintName == null)
                return Option<IDatabaseKey>.None;

            var keyColumns = groupedByName
                .Where(row => string.Equals(row.Key.ConstraintName, constraintName, StringComparison.Ordinal))
                .SelectMany(g => g.Value
                    .Where(row => row.ColumnName != null && columnLookup.ContainsKey(row.ColumnName))
                    .OrderBy(static row => row.OrdinalPosition)
                    .Select(row => columnLookup[row.ColumnName!]))
                .ToList();

            var primaryKey = new PostgreSqlDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns);
            return Option<IDatabaseKey>.Some(primaryKey);
        }

        /// <summary>
        /// A SQL query that retrieves information on any primary key defined for a given table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

        private const string PrimaryKeyQuerySql = @$"
select
    kc.constraint_name as ""{ nameof(GetTablePrimaryQueryResult.ConstraintName) }"",
    kc.column_name as ""{ nameof(GetTablePrimaryQueryResult.ColumnName) }"",
    kc.ordinal_position as ""{ nameof(GetTablePrimaryQueryResult.OrdinalPosition) }""
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @{ nameof(GetTablePrimaryKeyQuery.SchemaName) } and tc.table_name = @{ nameof(GetTablePrimaryKeyQuery.TableName) }
    and tc.constraint_type = 'PRIMARY KEY'";

        /// <summary>
        /// Retrieves indexes that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of indexes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var queryResult = await DbConnection.QueryAsync<GetTableIndexesQueryResult>(
                IndexesQuery,
                new GetTableIndexesQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupAsDictionary(static row => new { row.IndexName, row.IsUnique, row.IsPrimary }).ToList();
            if (indexColumns.Empty())
                return Array.Empty<IDatabaseIndex>();

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo.Value
                    .Where(static row => row.IndexColumnExpression != null)
                    .OrderBy(static row => row.IndexColumnId)
                    .Select(row => new
                    {
                        row.IsDescending,
                        Expression = row.IndexColumnExpression,
                        Column = row.IndexColumnExpression != null && columnLookup.ContainsKey(row.IndexColumnExpression)
                            ? columnLookup[row.IndexColumnExpression]
                            : null
                    })
                    .Select(row =>
                    {
                        var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var expression = row.Column != null
                            ? Dialect.QuoteName(row.Column.Name)
                            : row.Expression;
                        return row.Column != null
                            ? new PostgreSqlDatabaseIndexColumn(expression!, row.Column, order)
                            : new PostgreSqlDatabaseIndexColumn(expression!, order);
                    })
                    .ToList();

                var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols);
                result.Add(index);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves information on indexes for a given table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @$"
select
    i.relname as ""{ nameof(GetTableIndexesQueryResult.IndexName) }"",
    idx.indisunique as ""{ nameof(GetTableIndexesQueryResult.IsUnique) }"",
    idx.indisprimary as ""{ nameof(GetTableIndexesQueryResult.IsPrimary) }"",
    pg_catalog.generate_subscripts(idx.indkey, 1) as ""{ nameof(GetTableIndexesQueryResult.IndexColumnId) }"",
    pg_catalog.unnest(array(
        select pg_catalog.pg_get_indexdef(idx.indexrelid, k + 1, true)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as ""{ nameof(GetTableIndexesQueryResult.IndexColumnExpression) }"",
    pg_catalog.unnest(array(
        select pg_catalog.pg_index_column_has_property(idx.indexrelid, k + 1, 'desc')
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as ""{ nameof(GetTableIndexesQueryResult.IsDescending) }"",
    (idx.indexprs is not null) or (idx.indkey::int[] @> array[0]) as ""{ nameof(GetTableIndexesQueryResult.IsFunctional) }""
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class t on idx.indrelid = t.oid
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
where
    t.relkind = 'r'
    and t.relname = @{ nameof(GetTableIndexesQuery.TableName) }
    and ns.nspname = @{ nameof(GetTableIndexesQuery.SchemaName) }";

        /// <summary>
        /// Retrieves unique keys that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of unique keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var uniqueKeyColumns = await DbConnection.QueryAsync<GetTableUniqueKeysQueryResult>(
                UniqueKeysQuery,
                new GetTableUniqueKeysQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var groupedByName = uniqueKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName });
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Value
                        .Where(row => row.ColumnName != null && columnLookup.ContainsKey(row.ColumnName))
                        .OrderBy(static row => row.OrdinalPosition)
                        .Select(row => columnLookup[row.ColumnName!])
                        .ToList(),
                })
                .ToList();
            if (constraintColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new PostgreSqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        /// <summary>
        /// A SQL query that returns unique key information for a particular table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UniqueKeysQuery => UniqueKeysQuerySql;

        private const string UniqueKeysQuerySql = @$"
select
    kc.constraint_name as ""{ nameof(GetTableUniqueKeysQueryResult.ConstraintName) }"",
    kc.column_name as ""{ nameof(GetTableUniqueKeysQueryResult.ColumnName) }"",
    kc.ordinal_position as ""{ nameof(GetTableUniqueKeysQueryResult.OrdinalPosition) }""
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @{ nameof(GetTableUniqueKeysQuery.SchemaName) } and tc.table_name = @{ nameof(GetTableUniqueKeysQuery.TableName) }
    and tc.constraint_type = 'UNIQUE'";

        /// <summary>
        /// Retrieves child keys that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of child keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var queryResult = await DbConnection.QueryAsync<GetTableChildKeysQueryResult>(
                ChildKeysQuery,
                new GetTableChildKeysQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var groupedChildKeys = queryResult.GroupAsDictionary(static row =>
            new
            {
                row.ChildTableSchema,
                row.ChildTableName,
                row.ChildKeyName,
                row.ParentKeyName,
                row.ParentKeyType,
                row.DeleteAction,
                row.UpdateAction
            }).ToList();
            if (groupedChildKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var primaryKey = await queryCache.GetPrimaryKeyAsync(tableName, cancellationToken).ConfigureAwait(false);
            var uniqueKeys = await queryCache.GetUniqueKeysAsync(tableName, cancellationToken).ConfigureAwait(false);
            var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);

            foreach (var groupedChildKey in groupedChildKeys)
            {
                // ensure we have a key to begin with
                IDatabaseKey? parentKey = null;
                if (string.Equals(groupedChildKey.Key.ParentKeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                    await primaryKey.IfSomeAsync(k => parentKey = k).ConfigureAwait(false);
                else if (uniqueKeyLookup.ContainsKey(groupedChildKey.Key.ParentKeyName))
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];

                if (parentKey == null)
                    continue;

                var candidateChildTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTableNameOption = queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken);

                await childTableNameOption
                    .BindAsync(async childTableName =>
                    {
                        var childParentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken).ConfigureAwait(false);
                        var parentKeyLookup = GetDatabaseKeyLookup(childParentKeys.Select(static fk => fk.ChildKey).ToList());

                        var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);
                        if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                            return OptionAsync<IDatabaseRelationalKey>.None;

                        var deleteAction = ReferentialActionMapping[groupedChildKey.Key.DeleteAction];
                        var updateAction = ReferentialActionMapping[groupedChildKey.Key.UpdateAction];
                        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, tableName, parentKey, deleteAction, updateAction);
                        return OptionAsync<IDatabaseRelationalKey>.Some(relationalKey);
                    })
                    .IfSome(relationalKey => result.Add(relationalKey))
                    .ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves information on child keys for a given table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ChildKeysQuery => ChildKeysQuerySql;

        private const string ChildKeysQuerySql = @$"
select
    ns.nspname as ""{ nameof(GetTableChildKeysQueryResult.ChildTableSchema) }"",
    t.relname as ""{ nameof(GetTableChildKeysQueryResult.ChildTableName) }"",
    c.conname as ""{ nameof(GetTableChildKeysQueryResult.ChildKeyName) }"",
    pkc.contype as ""{ nameof(GetTableChildKeysQueryResult.ParentKeyType) }"",
    pkc.conname as ""{ nameof(GetTableChildKeysQueryResult.ParentKeyName) }"",
    c.confupdtype as ""{ nameof(GetTableChildKeysQueryResult.UpdateAction) }"",
    c.confdeltype as ""{ nameof(GetTableChildKeysQueryResult.DeleteAction) }""
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid and c.contype = 'f'
inner join pg_catalog.pg_class pt on pt.oid = c.confrelid
inner join pg_catalog.pg_namespace pns on pns.oid = pt.relnamespace
left join pg_catalog.pg_depend d1  -- find constraint's dependency on an index
    on d1.objid = c.oid
    and d1.classid = 'pg_constraint'::regclass
    and d1.refclassid = 'pg_class'::regclass
    and d1.refobjsubid = 0
left join pg_catalog.pg_depend d2  -- find pkey/unique constraint for that index
    on d2.refclassid = 'pg_constraint'::regclass
    and d2.classid = 'pg_class'::regclass
    and d2.objid = d1.refobjid
    and d2.objsubid = 0
    and d2.deptype = 'i'
left join pg_catalog.pg_constraint pkc on pkc.oid = d2.refobjid
    and pkc.contype in ('p', 'u')
    and pkc.conrelid = c.confrelid
where pt.relname = @{ nameof(GetTableChildKeysQuery.TableName) } and pns.nspname = @{ nameof(GetTableChildKeysQuery.SchemaName) }";

        /// <summary>
        /// Retrieves check constraints defined on a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of check constraints.</returns>
        protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            var checks = await DbConnection.QueryAsync<GetTableChecksQueryResult>(
                ChecksQuery,
                new GetTableChecksQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var definition = checkRow.Definition;
                if (definition.IsNullOrWhiteSpace())
                    continue;

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);

                var check = new PostgreSqlCheckConstraint(constraintName, definition);
                result.Add(check);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves check constraint information for a table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @$"
select
    c.conname as ""{ nameof(GetTableChecksQueryResult.ConstraintName) }"",
    c.consrc as ""{ nameof(GetTableChecksQueryResult.Definition) }""
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @{ nameof(GetTableChecksQuery.TableName) }
    and ns.nspname = @{ nameof(GetTableChecksQuery.SchemaName) }";

        /// <summary>
        /// Retrieves foreign keys that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var queryResult = await DbConnection.QueryAsync<GetTableParentKeysQueryResult>(
                ParentKeysQuery,
                new GetTableParentKeysQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupAsDictionary(static row => new
            {
                row.ChildKeyName,
                row.ParentSchemaName,
                row.ParentTableName,
                row.ParentKeyName,
                KeyType = row.ParentKeyType,
                row.DeleteAction,
                row.UpdateAction
            }).ToList();
            if (foreignKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentSchemaName, fkey.Key.ParentTableName);
                var parentTableNameOption = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken).ConfigureAwait(false);
                Identifier? resolvedParentTableName = null;

                await parentTableNameOption
                    .BindAsync(async parentTableName =>
                    {
                        resolvedParentTableName = parentTableName;
                        if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                        {
                            var pk = await queryCache.GetPrimaryKeyAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                            return pk.ToAsync();
                        }

                        var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);
                        var uniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

                        return uniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                            ? OptionAsync<IDatabaseKey>.Some(uniqueKeyLookup[parentKeyName.LocalName])
                            : OptionAsync<IDatabaseKey>.None;
                    })
                    .Map(parentKey =>
                    {
                        var parentTableName = resolvedParentTableName!;

                        var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                        var childKeyColumns = fkey.Value
                            .Where(row => row.ColumnName != null && columnLookup.ContainsKey(row.ColumnName))
                            .OrderBy(static row => row.ConstraintColumnId)
                            .Select(row => columnLookup[row.ColumnName!])
                            .ToList();

                        var childKey = new PostgreSqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                        var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                        var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                        return new DatabaseRelationalKey(tableName, childKey, parentTableName, parentKey, deleteAction, updateAction);
                    })
                    .IfSome(relationalKey => result.Add(relationalKey))
                    .ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves information about foreign keys.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ParentKeysQuery => ParentKeysQuerySql;

        private const string ParentKeysQuerySql = @$"
select
    c.conname as ""{ nameof(GetTableParentKeysQueryResult.ChildKeyName) }"",
    tc.attname as ""{ nameof(GetTableParentKeysQueryResult.ColumnName) }"",
    child_cols.con_index as ""{ nameof(GetTableParentKeysQueryResult.ConstraintColumnId) }"",
    pns.nspname as ""{ nameof(GetTableParentKeysQueryResult.ParentSchemaName) }"",
    pt.relname as ""{ nameof(GetTableParentKeysQueryResult.ParentTableName) }"",
    pkc.contype as ""{ nameof(GetTableParentKeysQueryResult.ParentKeyType) }"",
    pkc.conname as ""{ nameof(GetTableParentKeysQueryResult.ParentKeyName) }"",
    c.confupdtype as ""{ nameof(GetTableParentKeysQueryResult.UpdateAction) }"",
    c.confdeltype as ""{ nameof(GetTableParentKeysQueryResult.DeleteAction) }""
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid and c.contype = 'f'
inner join pg_catalog.pg_attribute tc on tc.attrelid = t.oid and tc.attnum = any(c.conkey)
inner join pg_catalog.unnest(c.conkey) with ordinality as child_cols(col_index, con_index) on child_cols.col_index = tc.attnum
inner join pg_catalog.pg_class pt on pt.oid = c.confrelid
inner join pg_catalog.pg_namespace pns on pns.oid = pt.relnamespace
left join pg_catalog.pg_depend d1  -- find constraint's dependency on an index
    on d1.objid = c.oid
    and d1.classid = 'pg_constraint'::regclass
    and d1.refclassid = 'pg_class'::regclass
    and d1.refobjsubid = 0
left join pg_catalog.pg_depend d2  -- find pkey/unique constraint for that index
    on d2.refclassid = 'pg_constraint'::regclass
    and d2.classid = 'pg_class'::regclass
    and d2.objid = d1.refobjid
    and d2.objsubid = 0
    and d2.deptype = 'i'
left join pg_catalog.pg_constraint pkc on pkc.oid = d2.refobjid
    and pkc.contype in ('p', 'u')
    and pkc.conrelid = c.confrelid
where t.relname = @{ nameof(GetTableParentKeysQuery.TableName) } and ns.nspname = @{ nameof(GetTableParentKeysQuery.SchemaName) }";

        /// <summary>
        /// Retrieves the columns for a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An ordered collection of columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadColumnsAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var query = await DbConnection.QueryAsync<GetTableColumnsQueryResult>(
                ColumnsQuery,
                new GetTableColumnsQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(Constants.PgCatalog, row.DataType),
                    Collation = !row.CollationName.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationCatalog, row.CollationSchema, row.CollationName))
                        : Option<Identifier>.None,
                    MaxLength = row.CharacterMaximumLength > 0
                        ? row.CharacterMaximumLength
                        : CreatePrecisionFromBase(row.NumericPrecision, row.NumericPrecisionRadix),
                    NumericPrecision = row.NumericPrecisionRadix > 0
                        ? Option<INumericPrecision>.Some(CreatePrecisionWithScaleFromBase(row.NumericPrecision, row.NumericScale, row.NumericPrecisionRadix))
                        : Option<INumericPrecision>.None
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);

                var isAutoIncrement = !row.SerialSequenceSchemaName.IsNullOrWhiteSpace() && !row.SerialSequenceLocalName.IsNullOrWhiteSpace();
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                    : Option<IAutoIncrement>.None;
                var defaultValue = !row.ColumnDefault.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.ColumnDefault)
                    : Option<string>.None;
                var isNullable = string.Equals(row.IsNullable, Constants.Yes, StringComparison.Ordinal);

                var column = new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);
                result.Add(column);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves column definitions.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ColumnsQuery => ColumnsQuerySql;

        // a little bit convoluted due to the quote_ident() being required.
        // when missing, case folding will occur (we should have guaranteed that this is already done)
        // additionally the default behaviour misses the schema which may be necessary
        private const string ColumnsQuerySql = @$"
select
    column_name as ""{ nameof(GetTableColumnsQueryResult.ColumnName) }"",
    ordinal_position as ""{ nameof(GetTableColumnsQueryResult.OrdinalPosition) }"",
    column_default as ""{ nameof(GetTableColumnsQueryResult.ColumnDefault) }"",
    is_nullable as ""{ nameof(GetTableColumnsQueryResult.IsNullable) }"",
    data_type as ""{ nameof(GetTableColumnsQueryResult.DataType) }"",
    character_maximum_length as ""{ nameof(GetTableColumnsQueryResult.CharacterMaximumLength) }"",
    character_octet_length as ""{ nameof(GetTableColumnsQueryResult.CharacterOctetLength) }"",
    numeric_precision as ""{ nameof(GetTableColumnsQueryResult.NumericPrecision) }"",
    numeric_precision_radix as ""{ nameof(GetTableColumnsQueryResult.NumericPrecisionRadix) }"",
    numeric_scale as ""{ nameof(GetTableColumnsQueryResult.NumericScale) }"",
    datetime_precision as ""{ nameof(GetTableColumnsQueryResult.DatetimePrecision) }"",
    interval_type as ""{ nameof(GetTableColumnsQueryResult.IntervalType) }"",
    collation_catalog as ""{ nameof(GetTableColumnsQueryResult.CollationCatalog) }"",
    collation_schema as ""{ nameof(GetTableColumnsQueryResult.CollationSchema) }"",
    collation_name as ""{ nameof(GetTableColumnsQueryResult.CollationName) }"",
    domain_catalog as ""{ nameof(GetTableColumnsQueryResult.DomainCatalog) }"",
    domain_schema as ""{ nameof(GetTableColumnsQueryResult.DomainSchema) }"",
    domain_name as ""{ nameof(GetTableColumnsQueryResult.DomainName) }"",
    udt_catalog as ""{ nameof(GetTableColumnsQueryResult.UdtCatalog) }"",
    udt_schema as ""{ nameof(GetTableColumnsQueryResult.UdtSchema) }"",
    udt_name as ""{ nameof(GetTableColumnsQueryResult.UdtName) }"",
    dtd_identifier as ""{ nameof(GetTableColumnsQueryResult.DtdIdentifier) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[1] as ""{ nameof(GetTableColumnsQueryResult.SerialSequenceSchemaName) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[2] as ""{ nameof(GetTableColumnsQueryResult.SerialSequenceLocalName) }""
from information_schema.columns
where table_schema = @{ nameof(GetTableColumnsQuery.SchemaName) } and table_name = @{ nameof(GetTableColumnsQuery.TableName) }
order by ordinal_position";

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
            var queryResult = await DbConnection.QueryAsync<GetTableTriggersQueryResult>(
                TriggersQuery,
                new GetTableTriggersQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupAsDictionary(static row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing,
                row.EnabledFlag
            }).ToList();
            if (triggers.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
                var queryTiming = Enum.TryParse(trig.Key.Timing, true, out TriggerQueryTiming timing) ? timing : TriggerQueryTiming.Before;
                var definition = trig.Key.Definition;

                var events = TriggerEvent.None;
                foreach (var trigEvent in trig.Value)
                {
                    if (string.Equals(trigEvent.TriggerEvent, Constants.Insert, StringComparison.Ordinal))
                        events |= TriggerEvent.Insert;
                    else if (string.Equals(trigEvent.TriggerEvent, Constants.Update, StringComparison.Ordinal))
                        events |= TriggerEvent.Update;
                    else if (string.Equals(trigEvent.TriggerEvent, Constants.Delete, StringComparison.Ordinal))
                        events |= TriggerEvent.Delete;
                    else
                        throw new UnsupportedTriggerEventException(tableName, trigEvent.TriggerEvent ?? string.Empty);
                }

                var isEnabled = !string.Equals(trig.Key.EnabledFlag, Constants.DisabledFlag, StringComparison.Ordinal);
                var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves information about any triggers on the table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TriggersQuery => TriggersQuerySql;

        private const string TriggersQuerySql = @$"
select
    tr.tgname as ""{ nameof(GetTableTriggersQueryResult.TriggerName) }"",
    tgenabled as ""{ nameof(GetTableTriggersQueryResult.EnabledFlag) }"",
    itr.action_statement as ""{ nameof(GetTableTriggersQueryResult.Definition) }"",
    itr.action_timing as ""{ nameof(GetTableTriggersQueryResult.Timing) }"",
    itr.event_manipulation as ""{ nameof(GetTableTriggersQueryResult.TriggerEvent) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on t.oid = tr.tgrelid
inner join information_schema.triggers itr on ns.nspname = itr.event_object_schema and itr.event_object_table = t.relname and itr.trigger_name = tr.tgname
where t.relkind = 'r'
    and t.relname = @{ nameof(GetTableTriggersQuery.TableName) }
    and ns.nspname = @{ nameof(GetTableTriggersQuery.SchemaName) }";

        /// <summary>
        /// Creates a column lookup, keyed by the column's name.
        /// </summary>
        /// <param name="columns">Columns to create a lookup from.</param>
        /// <returns>A dictionary whose keys are column names, and the values are the columns associated with those names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <c>null</c>.</exception>
        protected static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
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

        private static IReadOnlyDictionary<Identifier, IDatabaseKey> GetDatabaseKeyLookup(IReadOnlyCollection<IDatabaseKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var result = new Dictionary<Identifier, IDatabaseKey>(keys.Count);

            foreach (var key in keys)
            {
                key.Name.IfSome(name => result[name.LocalName] = key);
            }

            return result;
        }

        /// <summary>
        /// Creates a numeric precision given a base.
        /// </summary>
        /// <param name="precision">The numeric precision.</param>
        /// <param name="radix">The radix.</param>
        /// <returns>A numeric precision object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="radix"/> is less than zero.</exception>
        protected static int CreatePrecisionFromBase(int precision, int radix)
        {
            if (precision <= 0)
                return 0;
            if (radix < 0)
                throw new ArgumentOutOfRangeException(nameof(radix));

            var newPrecision = Convert.ToInt64(Math.Pow(precision, radix));
            var newPrecisionStr = newPrecision.ToString(CultureInfo.InvariantCulture);

            return newPrecisionStr.Length;
        }

        /// <summary>
        /// Creates a numeric precision with scale, given a base.
        /// </summary>
        /// <param name="precision">The numeric precision.</param>
        /// <param name="scale">The numeric scale.</param>
        /// <param name="radix">The radix.</param>
        /// <returns>A numeric precision object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="precision"/> or <paramref name="scale"/> or <paramref name="radix"/> are less than zero.</exception>
        protected static INumericPrecision CreatePrecisionWithScaleFromBase(int precision, int scale, int radix)
        {
            if (precision < 0)
                throw new ArgumentOutOfRangeException(nameof(precision));
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale));
            if (radix < 0)
                throw new ArgumentOutOfRangeException(nameof(radix));

            var newPrecision = Convert.ToInt64(Math.Pow(precision, radix));
            var newPrecisionStr = newPrecision.ToString(CultureInfo.InvariantCulture);

            var newScale = Convert.ToInt64(Math.Pow(scale, radix));
            var newScaleStr = newScale.ToString(CultureInfo.InvariantCulture);

            return new NumericPrecision(newPrecisionStr.Length, newScaleStr.Length);
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
            ["d"] = ReferentialAction.SetDefault
        };

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
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
        }

        /// <summary>
        /// A set of constants used to test results of queries.
        /// </summary>
        protected static class Constants
        {
            /// <summary>
            /// Used to check whether a trigger event is a <c>DELETE</c> event.
            /// </summary>
            public const string Delete = "DELETE";

            /// <summary>
            /// Determines whether a trigger is enabled.
            /// </summary>
            public const string DisabledFlag = "D";

            /// <summary>
            /// Used to check whether a trigger event is an <c>INSERT</c> event.
            /// </summary>
            public const string Insert = "INSERT";

            /// <summary>
            /// The built-in system schema.
            /// </summary>
            public const string PgCatalog = "pg_catalog";

            /// <summary>
            /// Determines whether a key type is a primary key.
            /// </summary>
            public const string PrimaryKeyType = "p";

            /// <summary>
            /// Used to check whether a trigger event is an <c>UPDATE</c> event.
            /// </summary>
            public const string Update = "UPDATE";

            /// <summary>
            /// Some queries return yes/no, this handles the yes case.
            /// </summary>
            public const string Yes = "YES";

            /// <summary>
            /// Determines whether a column is generated.
            /// </summary>
            public const string Always = "ALWAYS";
        }

        /// <summary>
        /// A query cache provider for PostgreSQL tables. Ensures that a given query only occurs at most once for a given query context.
        /// </summary>
        protected class PostgreSqlTableQueryCache
        {
            private readonly AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache> _tableNames;
            private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache> _columns;
            private readonly AsyncCache<Identifier, Option<IDatabaseKey>, PostgreSqlTableQueryCache> _primaryKeys;
            private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, PostgreSqlTableQueryCache> _uniqueKeys;
            private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache> _foreignKeys;

            /// <summary>
            /// Initializes a new instance of the <see cref="PostgreSqlTableQueryCache"/> class.
            /// </summary>
            /// <param name="tableNameLoader">A table name cache.</param>
            /// <param name="columnLoader">A column cache.</param>
            /// <param name="primaryKeyLoader">A primary key cache.</param>
            /// <param name="uniqueKeyLoader">A unique key cache.</param>
            /// <param name="foreignKeyLoader">A foreign key cache.</param>
            /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/> or <paramref name="foreignKeyLoader"/> are <c>null</c>.</exception>
            public PostgreSqlTableQueryCache(
                AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache> tableNameLoader,
                AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache> columnLoader,
                AsyncCache<Identifier, Option<IDatabaseKey>, PostgreSqlTableQueryCache> primaryKeyLoader,
                AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, PostgreSqlTableQueryCache> uniqueKeyLoader,
                AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache> foreignKeyLoader
            )
            {
                _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
                _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
                _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
                _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
                _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
            }

            /// <summary>
            /// Retrieves the table name from the cache, querying the database when not populated.
            /// </summary>
            /// <param name="tableName">A table name.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A table name, if matched in the database.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
            public Task<Option<Identifier>> GetTableNameAsync(Identifier tableName, CancellationToken cancellationToken)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                return _tableNames.GetByKeyAsync(tableName, this, cancellationToken);
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
