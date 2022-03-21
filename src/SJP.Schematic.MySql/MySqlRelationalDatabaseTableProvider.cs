using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Query;
using SJP.Schematic.MySql.QueryResult;

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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
    public MySqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

        _supportsChecks = new AsyncLazy<bool>(LoadHasCheckSupport);
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
    protected MySqlTableQueryCache CreateQueryCache() => new(
        new AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token)),
        new AsyncCache<Identifier, Option<IDatabaseKey>, MySqlTableQueryCache>(LoadPrimaryKeyAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, MySqlTableQueryCache>(LoadUniqueKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache>(LoadParentKeysAsync)
    );

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public virtual async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryResults = await DbConnection.QueryAsync<GetAllTableNamesQueryResult>(
            TablesQuery,
            new GetAllTableNamesQuery { SchemaName = IdentifierDefaults.Schema! },
            cancellationToken
        ).ConfigureAwait(false);
        var tableNames = queryResults
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
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
    TABLE_SCHEMA as `{ nameof(GetAllTableNamesQueryResult.SchemaName) }`,
    TABLE_NAME as `{ nameof(GetAllTableNamesQueryResult.TableName) }`
from information_schema.tables
where TABLE_SCHEMA = @{ nameof(GetAllTableNamesQuery.SchemaName) } order by TABLE_NAME";

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

        tableName = QualifyTableName(tableName);
        var qualifiedTableName = DbConnection.QueryFirstOrNone<GetTableNameQueryResult>(
            TableNameQuery,
            new GetTableNameQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName
            .Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.TableName))
            .ToOption();
    }

    /// <summary>
    /// A SQL query definition that resolves a table name for the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string TableNameQuery => TableNameQuerySql;

    private const string TableNameQuerySql = @$"
select
    table_schema as `{ nameof(GetTableNameQueryResult.SchemaName) }`,
    table_name as `{ nameof(GetTableNameQueryResult.TableName) }`
from information_schema.tables
where table_schema = @{ nameof(GetTableNameQuery.SchemaName) } and table_name = @{ nameof(GetTableNameQuery.TableName) }
limit 1";

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">The query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <c>null</c>.</exception>
    protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var columnsTask = queryCache.GetColumnsAsync(tableName, cancellationToken);
        var checksTask = LoadChecksAsync(tableName, cancellationToken);
        var triggersTask = LoadTriggersAsync(tableName, cancellationToken);
        var indexesTask = LoadIndexesAsync(tableName, queryCache, cancellationToken);
        var primaryKeyTask = queryCache.GetPrimaryKeyAsync(tableName, cancellationToken);
        var uniqueKeysTask = queryCache.GetUniqueKeysAsync(tableName, cancellationToken);
        var parentKeysTask = queryCache.GetForeignKeysAsync(tableName, cancellationToken);
        var childKeysTask = LoadChildKeysAsync(tableName, queryCache, cancellationToken);

        await Task.WhenAll(columnsTask, checksTask, triggersTask, indexesTask, primaryKeyTask, uniqueKeysTask, parentKeysTask, childKeysTask).ConfigureAwait(false);

        var columns = await columnsTask.ConfigureAwait(false);
        var checks = await checksTask.ConfigureAwait(false);
        var triggers = await triggersTask.ConfigureAwait(false);
        var indexes = await indexesTask.ConfigureAwait(false);
        var primaryKey = await primaryKeyTask.ConfigureAwait(false);
        var uniqueKeys = await uniqueKeysTask.ConfigureAwait(false);
        var parentKeys = await parentKeysTask.ConfigureAwait(false);
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
    protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var primaryKeyColumns = await DbConnection.QueryAsync<GetTablePrimaryKeyQueryResult>(
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

        var keyColumns = groupedByName
            .Where(row => string.Equals(row.Key.ConstraintName, constraintName, StringComparison.Ordinal))
            .SelectMany(g => g.Value.ConvertAll(row => columnLookup[row.ColumnName!]))
            .ToList();

        var primaryKey = new MySqlDatabasePrimaryKey(keyColumns);
        return Option<IDatabaseKey>.Some(primaryKey);
    }

    /// <summary>
    /// A SQL query that retrieves information on any primary key defined for a given table.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

    private const string PrimaryKeyQuerySql = @$"
select
    kc.constraint_name as `{ nameof(GetTablePrimaryKeyQueryResult.ConstraintName) }`,
    kc.column_name as `{ nameof(GetTablePrimaryKeyQueryResult.ColumnName) }`
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where t.table_schema = @{ nameof(GetTablePrimaryKeyQuery.SchemaName) } and t.table_name = @{ nameof(GetTablePrimaryKeyQuery.TableName) }
    and tc.constraint_type = 'PRIMARY KEY'
order by kc.ordinal_position";

    /// <summary>
    /// Retrieves indexes that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of indexes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync<GetTableIndexesQueryResult>(
            IndexesQuery,
            new GetTableIndexesQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        if (queryResult.Empty())
            return Array.Empty<IDatabaseIndex>();

        var indexColumns = queryResult.GroupAsDictionary(static row => new { row.IndexName, row.IsNonUnique }).ToList();
        if (indexColumns.Empty())
            return Array.Empty<IDatabaseIndex>();

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseIndex>(indexColumns.Count);

        foreach (var indexInfo in indexColumns)
        {
            var isUnique = !indexInfo.Key.IsNonUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var indexCols = indexInfo.Value
                .OrderBy(static row => row.ColumnOrdinal)
                .Select(row => columnLookup[row.ColumnName])
                .Select(col =>
                {
                    var expression = Dialect.QuoteName(col.Name);
                    return new MySqlDatabaseIndexColumn(expression, col);
                })
                .ToList();

            var index = new MySqlDatabaseIndex(indexName, isUnique, indexCols);
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
    index_name as `{ nameof(GetTableIndexesQueryResult.IndexName) }`,
    non_unique as `{ nameof(GetTableIndexesQueryResult.IsNonUnique) }`,
    seq_in_index as `{ nameof(GetTableIndexesQueryResult.ColumnOrdinal) }`,
    column_name as `{ nameof(GetTableIndexesQueryResult.ColumnName) }`
from information_schema.statistics
where table_schema = @{ nameof(GetTableIndexesQuery.SchemaName) } and table_name = @{ nameof(GetTableIndexesQuery.TableName) }";

    /// <summary>
    /// Retrieves unique keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of unique keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
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
                Columns = g.Value.ConvertAll(row => columnLookup[row.ColumnName!]),
            })
            .ToList();
        if (constraintColumns.Empty())
            return Array.Empty<IDatabaseKey>();

        var result = new List<IDatabaseKey>(constraintColumns.Count);
        foreach (var uk in constraintColumns)
        {
            var uniqueKey = new MySqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
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
    kc.constraint_name as `{ nameof(GetTableUniqueKeysQueryResult.ConstraintName) }`,
    kc.column_name as `{ nameof(GetTableUniqueKeysQueryResult.ColumnName) }`
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where t.table_schema = @{ nameof(GetTableUniqueKeysQuery.SchemaName) } and t.table_name = @{ nameof(GetTableUniqueKeysQuery.TableName) }
    and tc.constraint_type = 'UNIQUE'
order by kc.ordinal_position";

    /// <summary>
    /// Retrieves child keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of child keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
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

        var uniqueKeys = await queryCache.GetUniqueKeysAsync(tableName, cancellationToken).ConfigureAwait(false);
        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

        var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);

        foreach (var groupedChildKey in groupedChildKeys.Select(ck => ck.Key))
        {
            // ensure we have a key to begin with
            IDatabaseKey? parentKey = null;
            if (string.Equals(groupedChildKey.ParentKeyType, Constants.PrimaryKey, StringComparison.Ordinal))
            {
                var pk = await queryCache.GetPrimaryKeyAsync(tableName, cancellationToken).ConfigureAwait(false);
                pk.IfSome(k => parentKey = k);
            }
            else if (uniqueKeyLookup.ContainsKey(groupedChildKey.ParentKeyName))
            {
                parentKey = uniqueKeyLookup[groupedChildKey.ParentKeyName];
            }

            if (parentKey == null)
                continue;

            var candidateChildTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.ChildTableSchema, groupedChildKey.ChildTableName);
            var resolvedName = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken).ConfigureAwait(false);

            await resolvedName
                .BindAsync(async name =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.ChildKeyName);

                    var parentKeys = await queryCache.GetForeignKeysAsync(name, cancellationToken).ConfigureAwait(false);
                    var parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());
                    if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                        return OptionAsync<IDatabaseRelationalKey>.None;

                    var deleteAction = ReferentialActionMapping[groupedChildKey.DeleteAction];
                    var updateAction = ReferentialActionMapping[groupedChildKey.UpdateAction];
                    var relationalKey = new MySqlRelationalKey(name, childKey, tableName, parentKey, deleteAction, updateAction);

                    return OptionAsync<IDatabaseRelationalKey>.Some(relationalKey);
                })
                .IfSome(key => result.Add(key))
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
    t.table_schema as `{ nameof(GetTableChildKeysQueryResult.ChildTableSchema) }`,
    t.table_name as `{ nameof(GetTableChildKeysQueryResult.ChildTableName) }`,
    rc.constraint_name as `{ nameof(GetTableChildKeysQueryResult.ChildKeyName) }`,
    rc.unique_constraint_name as `{ nameof(GetTableChildKeysQueryResult.ParentKeyName) }`,
    ptc.constraint_type as `{ nameof(GetTableChildKeysQueryResult.ParentKeyType) }`,
    rc.delete_rule as `{ nameof(GetTableChildKeysQueryResult.DeleteAction) }`,
    rc.update_rule as `{ nameof(GetTableChildKeysQueryResult.UpdateAction) }`
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where pt.table_schema = @{ nameof(GetTableChildKeysQuery.SchemaName) } and pt.table_name = @{ nameof(GetTableChildKeysQuery.TableName) }";

    /// <summary>
    /// Retrieves check constraints defined on a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of check constraints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        return LoadChecksAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var hasCheckSupport = await _supportsChecks.ConfigureAwait(false);
        if (!hasCheckSupport)
            return Array.Empty<IDatabaseCheckConstraint>();

        var queryResult = await DbConnection.QueryAsync<GetTableCheckConstraintsQueryResult>(
            ChecksQuery,
            new GetTableCheckConstraintsQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);
        if (queryResult.Empty())
            return Array.Empty<IDatabaseCheckConstraint>();

        var result = new List<IDatabaseCheckConstraint>();

        foreach (var row in queryResult)
        {
            var checkName = Identifier.CreateQualifiedIdentifier(row.ConstraintName);
            var isEnabled = string.Equals("YES", row.Enforced, StringComparison.OrdinalIgnoreCase);
            var check = new MySqlCheckConstraint(checkName, row.Definition, isEnabled);
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
    cc.constraint_name as `{ nameof(GetTableCheckConstraintsQueryResult.ConstraintName) }`,
    cc.check_clause as `{ nameof(GetTableCheckConstraintsQueryResult.Definition) }`,
    tc.enforced as `{ nameof(GetTableCheckConstraintsQueryResult.Enforced) }`
from information_schema.table_constraints tc
inner join information_schema.check_constraints cc on tc.table_schema = cc.constraint_schema and tc.constraint_name = cc.constraint_name
where tc.table_schema = @{ nameof(GetTableCheckConstraintsQuery.SchemaName) } and tc.table_name = @{ nameof(GetTableCheckConstraintsQuery.TableName) } and tc.constraint_type = 'CHECK'";

    /// <summary>
    /// Retrieves foreign keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
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
            row.ParentTableSchema,
            row.ParentTableName,
            row.ParentKeyName,
            KeyType = row.ParentKeyType,
            row.DeleteAction,
            row.UpdateAction,
        }).ToList();
        if (foreignKeys.Empty())
            return Array.Empty<IDatabaseRelationalKey>();

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
            var resolvedName = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken).ConfigureAwait(false);

            Identifier? parentTableName = null;

            await resolvedName
                .BindAsync(async name =>
                {
                    parentTableName = name;
                    if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKey, StringComparison.Ordinal))
                    {
                        var primaryKey = await queryCache.GetPrimaryKeyAsync(name, cancellationToken).ConfigureAwait(false);
                        return primaryKey.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                    var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(name, cancellationToken).ConfigureAwait(false);
                    var parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);
                    return parentUniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                        ? OptionAsync<IDatabaseKey>.Some(parentUniqueKeyLookup[parentKeyName.LocalName])
                        : OptionAsync<IDatabaseKey>.None;
                })
                .IfSome(key =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                    var childKeyColumns = fkey.Value
                        .OrderBy(static row => row.ConstraintColumnId)
                        .Select(row => columnLookup[row.ColumnName!])
                        .ToList();

                    var childKey = new MySqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                    var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                    var relationalKey = new DatabaseRelationalKey(tableName, childKey, parentTableName!, key, deleteAction, updateAction);
                    result.Add(relationalKey);
                })
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
    pt.table_schema as `{ nameof(GetTableParentKeysQueryResult.ParentTableSchema) }`,
    pt.table_name as `{ nameof(GetTableParentKeysQueryResult.ParentTableName) }`,
    rc.constraint_name as `{ nameof(GetTableParentKeysQueryResult.ChildKeyName) }`,
    rc.unique_constraint_name as `{ nameof(GetTableParentKeysQueryResult.ParentKeyName) }`,
    kc.column_name as `{ nameof(GetTableParentKeysQueryResult.ColumnName) }`,
    kc.ordinal_position as `{ nameof(GetTableParentKeysQueryResult.ConstraintColumnId) }`,
    ptc.constraint_type as `{ nameof(GetTableParentKeysQueryResult.ParentKeyType) }`,
    rc.delete_rule as `{ nameof(GetTableParentKeysQueryResult.DeleteAction) }`,
    rc.update_rule as `{ nameof(GetTableParentKeysQueryResult.UpdateAction) }`
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where t.table_schema = @{ nameof(GetTableParentKeysQuery.SchemaName) } and t.table_name = @{ nameof(GetTableParentKeysQuery.TableName) }";

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
                TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.CharacterMaxLength,
                NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
            };
            var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var isAutoIncrement = row.ExtraInformation != null
                && row.ExtraInformation.Contains(Constants.AutoIncrement, StringComparison.OrdinalIgnoreCase);
            var autoIncrement = isAutoIncrement
                ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                : Option<IAutoIncrement>.None;
            var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
            var isNullable = !string.Equals(row.IsNullable, Constants.No, StringComparison.OrdinalIgnoreCase);
            var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.DefaultValue)
                : Option<string>.None;
            var computedColumnDefinition = isComputed
                ? Option<string>.Some(row.ComputedColumnDefinition!)
                : Option<string>.None;

            var column = isComputed
                ? new DatabaseComputedColumn(columnName, columnType, isNullable, defaultValue, computedColumnDefinition)
                : new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);

            result.Add(column);
        }

        return result;
    }

    /// <summary>
    /// A SQL query that retrieves column definitions.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ColumnsQuery => ColumnsQuerySql;

    private const string ColumnsQuerySql = @$"
select
    column_name as `{ nameof(GetTableColumnsQueryResult.ColumnName) }`,
    data_type as `{ nameof(GetTableColumnsQueryResult.DataTypeName) }`,
    character_maximum_length as `{ nameof(GetTableColumnsQueryResult.CharacterMaxLength) }`,
    numeric_precision as `{ nameof(GetTableColumnsQueryResult.Precision) }`,
    numeric_scale as `{ nameof(GetTableColumnsQueryResult.Scale) }`,
    datetime_precision as `{ nameof(GetTableColumnsQueryResult.DateTimePrecision) }`,
    collation_name as `{ nameof(GetTableColumnsQueryResult.Collation) }`,
    is_nullable as `{ nameof(GetTableColumnsQueryResult.IsNullable) }`,
    column_default as `{ nameof(GetTableColumnsQueryResult.DefaultValue) }`,
    generation_expression as `{ nameof(GetTableColumnsQueryResult.ComputedColumnDefinition) }`,
    extra as `{ nameof(GetTableColumnsQueryResult.ExtraInformation) }`
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
            row.Timing
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
            foreach (var trigEvent in trig.Value.Select(tr => tr.TriggerEvent))
            {
                if (string.Equals(trigEvent, Constants.Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(trigEvent, Constants.Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(trigEvent, Constants.Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else
                    throw new UnsupportedTriggerEventException(tableName, trigEvent ?? string.Empty);
            }

            var trigger = new MySqlDatabaseTrigger(triggerName, definition, queryTiming, events);
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
    tr.trigger_name as `{ nameof(GetTableTriggersQueryResult.TriggerName) }`,
    tr.action_statement as `{ nameof(GetTableTriggersQueryResult.Definition) }`,
    tr.action_timing as `{ nameof(GetTableTriggersQueryResult.Timing) }`,
    tr.event_manipulation as `{ nameof(GetTableTriggersQueryResult.TriggerEvent) }`
from information_schema.triggers tr
where tr.event_object_schema = @{ nameof(GetTableTriggersQuery.SchemaName) } and tr.event_object_table = @{ nameof(GetTableTriggersQuery.TableName) }";

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
    /// A mapping from the referential actions as described in MySQL, to a <see cref="ReferentialAction"/> instance.
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
        {
            key.Name.IfSome(name => result[name.LocalName] = key);
        }

        return result;
    }

    private Task<bool> LoadHasCheckSupport()
    {
        const string sql = "select count(*) from information_schema.tables where table_schema = 'information_schema' and table_name = 'CHECK_CONSTRAINTS'";
        return DbConnection.ExecuteScalarAsync<bool>(sql, CancellationToken.None);
    }

    private readonly AsyncLazy<bool> _supportsChecks;

    private static class Constants
    {
        public const string AutoIncrement = "auto_increment";

        public const string Delete = "DELETE";

        public const string Insert = "INSERT";

        public const string No = "NO";

        public const string PrimaryKey = "PRIMARY KEY";

        public const string Update = "UPDATE";
    }

    /// <summary>
    /// A query cache provider for MySQL tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class MySqlTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, MySqlTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, MySqlTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache> _foreignKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/> or <paramref name="foreignKeyLoader"/> are <c>null</c>.</exception>
        public MySqlTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache> columnLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, MySqlTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, MySqlTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache> foreignKeyLoader
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