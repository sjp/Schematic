using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A database table provider for MySQL.
    /// </summary>
    /// <seealso cref="IRelationalDatabaseTableProvider" />
    public class OracleRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleRelationalDatabaseTableProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
        protected OracleTableQueryCache CreateQueryCache() => new OracleTableQueryCache(
            new AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
            new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token)),
            new AsyncCache<Identifier, Option<IDatabaseKey>, OracleTableQueryCache>(LoadPrimaryKeyAsync),
            new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, OracleTableQueryCache>(LoadUniqueKeysAsync),
            new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache>(LoadParentKeysAsync)
        );

        /// <summary>
        /// Gets all database tables.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database tables.</returns>
        public virtual async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResults = await DbConnection.QueryAsync<QualifiedName>(TablesQuery, cancellationToken).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
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

        private const string TablesQuerySql = @"
select
    t.OWNER as SchemaName,
    t.TABLE_NAME as ObjectName
from SYS.ALL_TABLES t
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
where
    o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null
order by t.OWNER, t.TABLE_NAME";

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
        protected Task<Option<Identifier>> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
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
            var qualifiedTableName = DbConnection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = candidateTableName.Schema, TableName = candidateTableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// A SQL query definition that resolves a table name for the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select t.OWNER as SchemaName, t.TABLE_NAME as ObjectName
from SYS.ALL_TABLES t
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
where
    t.OWNER = :SchemaName and t.TABLE_NAME = :TableName
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null";

        /// <summary>
        /// Retrieves a table from the database, if available.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">The query cache.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            var candidateTableName = QualifyTableName(tableName);
            return GetResolvedTableName(candidateTableName, cancellationToken)
                .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var columnsTask = queryCache.GetColumnsAsync(tableName, cancellationToken);
            var triggersTask = LoadTriggersAsync(tableName, cancellationToken);
            await Task.WhenAll(columnsTask, triggersTask).ConfigureAwait(false);

            var columns = await columnsTask.ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);
            var triggers = await triggersTask.ConfigureAwait(false);

            var primaryKeyTask = queryCache.GetPrimaryKeyAsync(tableName, cancellationToken);
            var uniqueKeysTask = queryCache.GetUniqueKeysAsync(tableName, cancellationToken);
            var indexesTask = LoadIndexesAsync(tableName, columnLookup, cancellationToken);
            var checksTask = LoadChecksAsync(tableName, columnLookup, cancellationToken);
            var childKeysTask = LoadChildKeysAsync(tableName, queryCache, cancellationToken);
            var parentKeysTask = queryCache.GetForeignKeysAsync(tableName, cancellationToken);

            await Task.WhenAll(primaryKeyTask, checksTask, uniqueKeysTask, indexesTask, childKeysTask, parentKeysTask).ConfigureAwait(false);

            var primaryKey = await primaryKeyTask.ConfigureAwait(false);
            var uniqueKeys = await uniqueKeysTask.ConfigureAwait(false);
            var indexes = await indexesTask.ConfigureAwait(false);
            var checks = await checksTask.ConfigureAwait(false);
            var childKeys = await childKeysTask.ConfigureAwait(false);
            var parentKeys = await parentKeysTask.ConfigureAwait(false);

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
        protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var primaryKeyColumns = await DbConnection.QueryAsync<ConstraintColumnMapping>(
                PrimaryKeyQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (primaryKeyColumns.Empty())
                return Option<IDatabaseKey>.None;

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName, row.EnabledStatus });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            var isEnabled = firstRow.Key.EnabledStatus == Constants.Enabled;

            var keyColumns = firstRow
                .OrderBy(row => row.ColumnPosition)
                .Where(row => row.ColumnName != null && columnLookup.ContainsKey(row.ColumnName))
                .Select(row => columnLookup[row.ColumnName!])
                .ToList();

            var primaryKey = constraintName != null
                ? new OracleDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, isEnabled)
                : (IDatabaseKey?)null;
            return primaryKey != null
                ? Option<IDatabaseKey>.Some(primaryKey)
                : Option<IDatabaseKey>.None;
        }

        /// <summary>
        /// A SQL query that retrieves information on any primary key defined for a given table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

        private const string PrimaryKeyQuerySql = @"
select
    ac.CONSTRAINT_NAME as ConstraintName,
    ac.STATUS as EnabledStatus,
    acc.COLUMN_NAME as ColumnName,
    acc.POSITION as ColumnPosition
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
where ac.OWNER = :SchemaName and ac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'P'";

        /// <summary>
        /// Retrieves indexes that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="columns">Columns for the given table.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of indexes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columns"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadIndexesAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await DbConnection.QueryAsync<IndexColumns>(
                IndexesQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IndexProperty, row.Uniqueness }).ToList();
            if (indexColumns.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var indexProperties = (OracleIndexProperties)indexInfo.Key.IndexProperty;
                var isUnique = indexInfo.Key.Uniqueness == Constants.Unique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnPosition)
                    .Where(row => row.ColumnName != null)
                    .Select(row => new { row.IsDescending, Column = row.ColumnName! })
                    .Select(row =>
                    {
                        var order = row.IsDescending == Constants.Y ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var indexColumns = columns.ContainsKey(row.Column)
                            ? new[] { columns[row.Column] }
                            : Array.Empty<IDatabaseColumn>();
                        var expression = Dialect.QuoteName(row.Column);
                        return new DatabaseIndexColumn(expression, indexColumns, order);
                    })
                    .ToList();

                var index = new OracleDatabaseIndex(indexName, isUnique, indexCols, indexProperties);
                result.Add(index);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves information on indexes for a given table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @"
select
    ai.OWNER as IndexOwner,
    ai.INDEX_NAME as IndexName,
    ai.UNIQUENESS as Uniqueness,
    ind.PROPERTY as IndexProperty,
    aic.COLUMN_NAME as ColumnName,
    aic.COLUMN_POSITION as ColumnPosition,
    aic.DESCEND as IsDescending
from SYS.ALL_INDEXES ai
inner join SYS.ALL_OBJECTS ao on ai.OWNER = ao.OWNER and ai.INDEX_NAME = ao.OBJECT_NAME
inner join SYS.IND$ ind on ao.OBJECT_ID = ind.OBJ#
inner join SYS.ALL_IND_COLUMNS aic
    on ai.OWNER = aic.INDEX_OWNER and ai.INDEX_NAME = aic.INDEX_NAME
where ai.TABLE_OWNER = :SchemaName and ai.TABLE_NAME = :TableName
    and aic.TABLE_OWNER = :SchemaName and aic.TABLE_NAME = :TableName
    and ao.OBJECT_TYPE = 'INDEX'
order by aic.COLUMN_POSITION";

        /// <summary>
        /// Retrieves unique keys that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of unique keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var uniqueKeyColumns = await DbConnection.QueryAsync<ConstraintColumnMapping>(
                UniqueKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var groupedByName = uniqueKeyColumns
                .Where(row => row.ConstraintName != null)
                .GroupBy(row => new { ConstraintName = row.ConstraintName!, row.EnabledStatus });
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.OrderBy(row => row.ColumnPosition)
                        .Where(row => row.ColumnName != null && columnLookup.ContainsKey(row.ColumnName))
                        .Select(row => columnLookup[row.ColumnName!])
                        .ToList(),
                    IsEnabled = g.Key.EnabledStatus == Constants.Enabled
                })
                .ToList();
            if (constraintColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            return constraintColumns
                .Select(uk => new OracleDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled))
                .ToList();
        }

        /// <summary>
        /// A SQL query that returns unique key information for a particular table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UniqueKeysQuery => UniqueKeysQuerySql;

        private const string UniqueKeysQuerySql = @"
select
    ac.CONSTRAINT_NAME as ConstraintName,
    ac.STATUS as EnabledStatus,
    acc.COLUMN_NAME as ColumnName,
    acc.POSITION as ColumnPosition
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
where ac.OWNER = :SchemaName and ac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'U'";

        /// <summary>
        /// Retrieves child keys that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of child keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var queryResult = await DbConnection.QueryAsync<ChildKeyData>(
                ChildKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var childKeyRows = queryResult.ToList();
            if (childKeyRows.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var primaryKey = await queryCache.GetPrimaryKeyAsync(tableName, cancellationToken).ConfigureAwait(false);
            var uniqueKeys = await queryCache.GetUniqueKeysAsync(tableName, cancellationToken).ConfigureAwait(false);
            var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

            var result = new List<IDatabaseRelationalKey>(childKeyRows.Count);

            foreach (var childKeyRow in childKeyRows)
            {
                // ensure we have a key to begin with
                IDatabaseKey? parentKey = null;
                if (childKeyRow.ParentKeyType == Constants.PrimaryKeyType)
                    await primaryKey.IfSomeAsync(k => parentKey = k).ConfigureAwait(false);
                else if (childKeyRow.ParentKeyName != null && uniqueKeyLookup.ContainsKey(childKeyRow.ParentKeyName))
                    parentKey = uniqueKeyLookup[childKeyRow.ParentKeyName];
                if (parentKey == null)
                    continue;

                var candidateChildTableName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildTableSchema, childKeyRow.ChildTableName);
                var childTableNameOption = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken).ConfigureAwait(false);

                await childTableNameOption
                    .BindAsync(async childTableName =>
                    {
                        var parentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken).ConfigureAwait(false);
                        var parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());

                        var childKeyName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildKeyName);
                        if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                            return OptionAsync<IDatabaseRelationalKey>.None;

                        var deleteAction = childKeyRow.DeleteAction != null && ReferentialActionMapping.ContainsKey(childKeyRow.DeleteAction)
                            ? ReferentialActionMapping[childKeyRow.DeleteAction]
                            : ReferentialAction.NoAction;

                        var relationalKey = new OracleRelationalKey(childTableName, childKey, tableName, parentKey, deleteAction);
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

        private const string ChildKeysQuerySql = @"
select
    ac.OWNER as ChildTableSchema,
    ac.TABLE_NAME as ChildTableName,
    ac.CONSTRAINT_NAME as ChildKeyName,
    ac.STATUS as EnabledStatus,
    ac.DELETE_RULE as DeleteAction,
    pac.CONSTRAINT_NAME as ParentKeyName,
    pac.CONSTRAINT_TYPE as ParentKeyType
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME
where pac.OWNER = :SchemaName and pac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'R' and pac.CONSTRAINT_TYPE in ('P', 'U')";

        /// <summary>
        /// Retrieves check constraints defined on a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="columns">Columns from the associated table.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of check constraints.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadChecksAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var checks = await DbConnection.QueryAsync<CheckConstraintData>(
                ChecksQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var columnNotNullConstraints = columns.Keys
                .Select(k => k.LocalName)
                .Select(GenerateNotNullDefinition)
                .ToList();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var definition = checkRow.Definition;
                if (definition == null || columnNotNullConstraints.Contains(definition))
                    continue;

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var isEnabled = checkRow.EnabledStatus == Constants.Enabled;

                var check = new DatabaseCheckConstraint(constraintName, definition, isEnabled);
                result.Add(check);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves check constraint information for a table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @"
select
    CONSTRAINT_NAME as ConstraintName,
    SEARCH_CONDITION as Definition,
    STATUS as EnabledStatus
from SYS.ALL_CONSTRAINTS
where OWNER = :SchemaName and TABLE_NAME = :TableName and CONSTRAINT_TYPE = 'C'";

        /// <summary>
        /// Retrieves foreign keys that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="queryCache">A query cache for the given context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (queryCache == null)
                throw new ArgumentNullException(nameof(queryCache));

            return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
        {
            var queryResult = await DbConnection.QueryAsync<ForeignKeyData>(
                ParentKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                row.ConstraintName,
                row.EnabledStatus,
                row.DeleteAction,
                row.ParentTableSchema,
                row.ParentTableName,
                row.ParentConstraintName,
                KeyType = row.ParentKeyType,
            }).ToList();
            if (foreignKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTableNameOption = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken).ConfigureAwait(false);
                Identifier? resolvedParentTableName = null;

                await parentTableNameOption
                    .BindAsync(async parentTableName =>
                    {
                        resolvedParentTableName = parentTableName;
                        if (fkey.Key.KeyType == Constants.PrimaryKeyType)
                        {
                            var pk = await queryCache.GetPrimaryKeyAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                            return pk.ToAsync();
                        }
                        else
                        {
                            var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentConstraintName);
                            var uniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                            var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

                            return uniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                                ? OptionAsync<IDatabaseKey>.Some(uniqueKeyLookup[parentKeyName.LocalName])
                                : OptionAsync<IDatabaseKey>.None;
                        }
                    })
                    .Map(parentKey =>
                    {
                        var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ConstraintName);
                        var childKeyColumns = fkey
                            .OrderBy(row => row.ColumnPosition)
                            .Where(row => columnLookup.ContainsKey(row.ColumnName))
                            .Select(row => columnLookup[row.ColumnName])
                            .ToList();

                        var isEnabled = fkey.Key.EnabledStatus == Constants.Enabled;
                        var childKey = new OracleDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                        var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                        return new OracleRelationalKey(tableName, childKey, resolvedParentTableName!, parentKey, deleteAction);
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

        private const string ParentKeysQuerySql = @"
select
    ac.CONSTRAINT_NAME as ConstraintName,
    ac.STATUS as EnabledStatus,
    ac.DELETE_RULE as DeleteAction,
    pac.OWNER as ParentTableSchema,
    pac.TABLE_NAME as ParentTableName,
    pac.CONSTRAINT_NAME as ParentConstraintName,
    pac.CONSTRAINT_TYPE as ParentKeyType,
    acc.COLUMN_NAME as ColumnName,
    acc.POSITION as ColumnPosition
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
inner join SYS.ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME
where ac.OWNER = :SchemaName and ac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'R' and pac.CONSTRAINT_TYPE in ('P', 'U')";

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
            var query = await DbConnection.QueryAsync<ColumnData>(
                ColumnsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var columnNames = query
                .Where(row => row.ColumnName != null)
                .Select(row => row.ColumnName!)
                .ToList();
            var notNullableColumnNames = await GetNotNullConstrainedColumnsAsync(tableName, columnNames, cancellationToken).ConfigureAwait(false);
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
                        : Option<INumericPrecision>.None
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var isNullable = !notNullableColumnNames.Contains(row.ColumnName);
                var isComputed = row.IsComputed == Constants.Yes;
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var computedColumnDefinition = isComputed && !row.DefaultValue.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;
                var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;

                var column = isComputed
                    ? new OracleDatabaseComputedColumn(columnName, columnType, isNullable, computedColumnDefinition)
                    : new OracleDatabaseColumn(columnName, columnType, isNullable, defaultValue);

                result.Add(column);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves column definitions.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
select
    COLUMN_NAME as ColumnName,
    DATA_TYPE_OWNER as ColumnTypeSchema,
    DATA_TYPE as ColumnTypeName,
    DATA_LENGTH as DataLength,
    DATA_PRECISION as Precision,
    DATA_SCALE as Scale,
    DATA_DEFAULT as DefaultValue,
    CHAR_LENGTH as CharacterLength,
    CHARACTER_SET_NAME as Collation,
    VIRTUAL_COLUMN as IsComputed
from SYS.ALL_TAB_COLS
where OWNER = :SchemaName and TABLE_NAME = :TableName
order by COLUMN_ID";

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
            var queryResult = await DbConnection.QueryAsync<TriggerData>(
                TriggersQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.ToList();
            if (triggers.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var triggerRow in triggers)
            {
                var triggerName = Identifier.CreateQualifiedIdentifier(triggerRow.TriggerSchema, triggerRow.TriggerName);
                var queryTiming = triggerRow.TriggerType != null && TimingMapping.ContainsKey(triggerRow.TriggerType)
                    ? TimingMapping[triggerRow.TriggerType]
                    : TriggerQueryTiming.After;
                var definition = triggerRow.Definition ?? string.Empty;
                var isEnabled = triggerRow.EnabledStatus == Constants.Enabled;

                var events = TriggerEvent.None;
                var triggerEventPieces = triggerRow.TriggerEvent != null
                    ? triggerRow.TriggerEvent.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>();

                foreach (var triggerEventPiece in triggerEventPieces)
                {
                    if (triggerEventPiece == Constants.Insert)
                        events |= TriggerEvent.Insert;
                    else if (triggerEventPiece == Constants.Update)
                        events |= TriggerEvent.Update;
                    else if (triggerEventPiece == Constants.Delete)
                        events |= TriggerEvent.Delete;
                    else
                        throw new UnsupportedTriggerEventException(tableName, triggerEventPiece);
                }

                var trigger = new DatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves information about any triggers on the table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TriggersQuery => TriggersQuerySql;

        private const string TriggersQuerySql = @"
select
    OWNER as TriggerSchema,
    TRIGGER_NAME as TriggerName,
    TRIGGER_TYPE as TriggerType,
    TRIGGERING_EVENT as TriggerEvent,
    TRIGGER_BODY as Definition,
    STATUS as EnabledStatus
from SYS.ALL_TRIGGERS
where TABLE_OWNER = :SchemaName and TABLE_NAME = :TableName and BASE_OBJECT_TYPE = 'TABLE'";

        /// <summary>
        /// Retrieves the names all of the not-null constrained columns in a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="columnNames">The column names for the given table.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of not-null constrained column names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columnNames"/> are <c>null</c>.</exception>
        protected Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsync(Identifier tableName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnNames == null)
                throw new ArgumentNullException(nameof(columnNames));

            return GetNotNullConstrainedColumnsAsyncCore(tableName, columnNames, cancellationToken);
        }

        private async Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsyncCore(Identifier tableName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
        {
            var checks = await DbConnection.QueryAsync<CheckConstraintData>(
                ChecksQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<string>();

            var columnNotNullConstraints = columnNames
                .Select(name => new KeyValuePair<string, string>(GenerateNotNullDefinition(name), name))
                .ToDictionary();

            return checks
                .Where(c => c.Definition != null && columnNotNullConstraints.ContainsKey(c.Definition) && c.EnabledStatus == Constants.Enabled)
                .Select(c => columnNotNullConstraints[c.Definition!])
                .ToList();
        }

        /// <summary>
        /// Creates a not null constraint definition, used to determine whether a constraint is a <c>NOT NULL</c> constraint.
        /// </summary>
        /// <param name="columnName">A column name.</param>
        /// <returns>A <c>NOT NULL</c> constraint definition for the given column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
        protected string GenerateNotNullDefinition(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            return _notNullDefinitions.GetOrAdd(columnName, colName => "\"" + colName + "\" IS NOT NULL");
        }

        /// <summary>
        /// A mapping from the referential actions as described in Oracle, to a <see cref="ReferentialAction"/> instance.
        /// </summary>
        /// <value>A mapping dictionary.</value>
        protected IReadOnlyDictionary<string, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<string, ReferentialAction>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = ReferentialAction.NoAction,
            ["RESTRICT"] = ReferentialAction.Restrict,
            ["CASCADE"] = ReferentialAction.Cascade,
            ["SET NULL"] = ReferentialAction.SetNull,
            ["SET DEFAULT"] = ReferentialAction.SetDefault
        };

        /// <summary>
        /// A mapping from the trigger query timings as described in Oracle, to a <see cref="TriggerQueryTiming"/> instance.
        /// </summary>
        /// <value>A mapping dictionary.</value>
        protected IReadOnlyDictionary<string, TriggerQueryTiming> TimingMapping { get; } = new Dictionary<string, TriggerQueryTiming>(StringComparer.OrdinalIgnoreCase)
        {
            ["BEFORE STATEMENT"] = TriggerQueryTiming.Before,
            ["BEFORE EACH ROW"] = TriggerQueryTiming.Before,
            ["AFTER STATEMENT"] = TriggerQueryTiming.After,
            ["AFTER EACH ROW"] = TriggerQueryTiming.After,
            ["INSTEAD OF"] = TriggerQueryTiming.InsteadOf,
            ["COMPOUND"] = TriggerQueryTiming.InsteadOf
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

        private static IReadOnlyDictionary<Identifier, IDatabaseKey> GetDatabaseKeyLookup(IReadOnlyCollection<IDatabaseKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var result = new Dictionary<Identifier, IDatabaseKey>(keys.Count);

            foreach (var key in keys)
                key.Name.IfSome(name => result[name.LocalName] = key);

            return result;
        }

        private readonly ConcurrentDictionary<string, string> _notNullDefinitions = new ConcurrentDictionary<string, string>();

        private static class Constants
        {
            public const string Delete = "DELETE";

            public const string Enabled = "ENABLED";

            public const string Insert = "INSERT";

            public const string PrimaryKeyType = "P";

            public const string Unique = "UNIQUE";

            public const string Update = "UPDATE";

            public const string Y = "Y";

            public const string Yes = "YES";
        }

        /// <summary>
        /// A query cache provider for Oracle tables. Ensures that a given query only occurs at most once for a given query context.
        /// </summary>
        protected class OracleTableQueryCache
        {
            private readonly AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache> _tableNames;
            private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache> _columns;
            private readonly AsyncCache<Identifier, Option<IDatabaseKey>, OracleTableQueryCache> _primaryKeys;
            private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, OracleTableQueryCache> _uniqueKeys;
            private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache> _foreignKeys;

            /// <summary>
            /// Initializes a new instance of the <see cref="OracleTableQueryCache"/> class.
            /// </summary>
            /// <param name="tableNameLoader">A table name cache.</param>
            /// <param name="columnLoader">A column cache.</param>
            /// <param name="primaryKeyLoader">A primary key cache.</param>
            /// <param name="uniqueKeyLoader">A unique key cache.</param>
            /// <param name="foreignKeyLoader">A foreign key cache.</param>
            /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/> or <paramref name="foreignKeyLoader"/> are <c>null</c>.</exception>
            public OracleTableQueryCache(
                AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache> tableNameLoader,
                AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache> columnLoader,
                AsyncCache<Identifier, Option<IDatabaseKey>, OracleTableQueryCache> primaryKeyLoader,
                AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, OracleTableQueryCache> uniqueKeyLoader,
                AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache> foreignKeyLoader
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
