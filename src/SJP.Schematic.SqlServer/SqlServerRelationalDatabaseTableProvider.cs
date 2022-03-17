﻿using System;
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
using SJP.Schematic.SqlServer.Query;
using SJP.Schematic.SqlServer.QueryResult;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A database table provider for SQL Server.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class SqlServerRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
    public SqlServerRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection that is specific to a given SQL Server database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

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
    protected SqlServerTableQueryCache CreateQueryCache() => new(
        new AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token)),
        new AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache>(LoadPrimaryKeyAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache>(LoadUniqueKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache>(LoadParentKeysAsync)
    );

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public virtual async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryResults = await DbConnection.QueryAsync<GetAllTableNamesQueryResult>(TablesQuery, cancellationToken).ConfigureAwait(false);
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
select schema_name(schema_id) as [{ nameof(GetAllTableNamesQueryResult.SchemaName) }], name as [{ nameof(GetAllTableNamesQueryResult.TableName) }]
from sys.tables
where is_ms_shipped = 0
order by schema_name(schema_id), name";

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

        var candidateTableName = QualifyTableName(tableName);
        return LoadTable(candidateTableName, CreateQueryCache(), cancellationToken);
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
select top 1 schema_name(schema_id) as [{ nameof(GetTableNameQueryResult.SchemaName) }], name as [{ nameof(GetTableNameQueryResult.TableName) }]
from sys.tables
where schema_id = schema_id(@{ nameof(GetTableNameQuery.SchemaName) }) and name = @{ nameof(GetTableNameQuery.TableName) } and is_ms_shipped = 0";

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">The query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <c>null</c>.</exception>
    protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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
        var uniqueKeys = await uniqueKeysTask.ConfigureAwait(false);
        var indexes = await indexesTask.ConfigureAwait(false);
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
    protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var primaryKeyColumns = await DbConnection.QueryAsync<GetTablePrimaryQueryResult>(
            PrimaryKeyQuery,
            new GetTablePrimaryKeyQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        if (primaryKeyColumns.Empty())
            return Option<IDatabaseKey>.None;

        var groupedByName = primaryKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDisabled });
        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;
        if (constraintName == null)
            return Option<IDatabaseKey>.None;

        var isEnabled = !firstRow.Key.IsDisabled;

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var keyColumns = groupedByName
            .Where(row => string.Equals(row.Key.ConstraintName, constraintName, StringComparison.Ordinal))
            .SelectMany(g => g.Value
                .Where(row => columnLookup.ContainsKey(row.ColumnName))
                .Select(row => columnLookup[row.ColumnName]))
            .ToList();

        var primaryKey = new SqlServerDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, isEnabled);
        return Option<IDatabaseKey>.Some(primaryKey);
    }

    /// <summary>
    /// A SQL query that retrieves information on any primary key defined for a given table.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

    private const string PrimaryKeyQuerySql = @$"
select
    kc.name as [{ nameof(GetTablePrimaryQueryResult.ConstraintName) }],
    c.name as [{ nameof(GetTablePrimaryQueryResult.ColumnName) }],
    i.is_disabled as [{ nameof(GetTablePrimaryQueryResult.IsDisabled) }]
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @{ nameof(GetTablePrimaryKeyQuery.SchemaName) } and t.name = @{ nameof(GetTablePrimaryKeyQuery.TableName) } and t.is_ms_shipped = 0
    and kc.type = 'PK' and ic.is_included_column = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.key_ordinal";

    /// <summary>
    /// Retrieves indexes that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of indexes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync<GetTableIndexesQueryResult>(
            IndexesQuery,
            new GetTableIndexesQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        if (queryResult.Empty())
            return Array.Empty<IDatabaseIndex>();

        var indexColumns = queryResult.GroupAsDictionary(static row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
        if (indexColumns.Empty())
            return Array.Empty<IDatabaseIndex>();

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = indexInfo.Key.IsUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
            var isEnabled = !indexInfo.Key.IsDisabled;

            var indexCols = indexInfo.Value
                .Where(row => !row.IsIncludedColumn && columnLookup.ContainsKey(row.ColumnName))
                .OrderBy(static row => row.KeyOrdinal)
                .ThenBy(static row => row.IndexColumnId)
                .Select(row => new { row.IsDescending, Column = columnLookup[row.ColumnName] })
                .Select(row =>
                {
                    var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var column = row.Column;
                    var expression = Dialect.QuoteName(column.Name);
                    return new DatabaseIndexColumn(expression, column, order);
                })
                .ToList();

            var includedCols = indexInfo.Value
                .Where(row => row.IsIncludedColumn && columnLookup.ContainsKey(row.ColumnName))
                .OrderBy(static row => row.KeyOrdinal)
                .ThenBy(static row => row.ColumnName) // matches SSMS behaviour
                .Select(row => columnLookup[row.ColumnName])
                .ToList();

            var index = new DatabaseIndex(indexName, isUnique, indexCols, includedCols, isEnabled);
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
    i.name as [{ nameof(GetTableIndexesQueryResult.IndexName) }],
    i.is_unique as [{ nameof(GetTableIndexesQueryResult.IsUnique) }],
    ic.key_ordinal as [{ nameof(GetTableIndexesQueryResult.KeyOrdinal) }],
    ic.index_column_id as [{ nameof(GetTableIndexesQueryResult.IndexColumnId) }],
    ic.is_included_column as [{ nameof(GetTableIndexesQueryResult.IsIncludedColumn) }],
    ic.is_descending_key as [{ nameof(GetTableIndexesQueryResult.IsDescending) }],
    c.name as [{ nameof(GetTableIndexesQueryResult.ColumnName) }],
    i.is_disabled as [{ nameof(GetTableIndexesQueryResult.IsDisabled) }]
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @{ nameof(GetTableIndexesQuery.SchemaName) } and t.name = @{ nameof(GetTableIndexesQuery.TableName) } and t.is_ms_shipped = 0
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.index_id, ic.key_ordinal, ic.index_column_id";

    /// <summary>
    /// Retrieves unique keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of unique keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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

        var groupedByName = uniqueKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDisabled });
        var constraintColumns = groupedByName
            .Select(g => new
            {
                g.Key.ConstraintName,
                Columns = g.Value
                    .Where(row => columnLookup.ContainsKey(row.ColumnName))
                    .Select(row => columnLookup[row.ColumnName])
                    .ToList(),
                IsEnabled = !g.Key.IsDisabled
            })
            .ToList();
        if (constraintColumns.Empty())
            return Array.Empty<IDatabaseKey>();

        var result = new List<IDatabaseKey>(constraintColumns.Count);
        foreach (var uk in constraintColumns)
        {
            var uniqueKey = new SqlServerDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
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
    kc.name as [{ nameof(GetTableUniqueKeysQueryResult.ConstraintName) }],
    c.name as [{ nameof(GetTableUniqueKeysQueryResult.ColumnName) }],
    i.is_disabled as [{ nameof(GetTableUniqueKeysQueryResult.IsDisabled) }]
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where
    schema_name(t.schema_id) = @{ nameof(GetTableUniqueKeysQuery.SchemaName) } and t.name = @{ nameof(GetTableUniqueKeysQuery.TableName) } and t.is_ms_shipped = 0
    and kc.type = 'UQ'
    and ic.is_included_column = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.key_ordinal";

    /// <summary>
    /// Retrieves child keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of child keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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
            var childTableNameOption = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken).ConfigureAwait(false);

            await childTableNameOption
                .BindAsync(async childTableName =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                    var parentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken).ConfigureAwait(false);
                    var parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());

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
    schema_name(child_t.schema_id) as [{ nameof(GetTableChildKeysQueryResult.ChildTableSchema) }],
    child_t.name as [{ nameof(GetTableChildKeysQueryResult.ChildTableName) }],
    fk.name as [{ nameof(GetTableChildKeysQueryResult.ChildKeyName) }],
    kc.name as [{ nameof(GetTableChildKeysQueryResult.ParentKeyName) }],
    kc.type as [{ nameof(GetTableChildKeysQueryResult.ParentKeyType) }],
    fk.delete_referential_action as [{ nameof(GetTableChildKeysQueryResult.DeleteAction) }],
    fk.update_referential_action as [{ nameof(GetTableChildKeysQueryResult.UpdateAction) }]
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @{ nameof(GetTableChildKeysQuery.SchemaName) } and parent_t.name = @{ nameof(GetTableChildKeysQuery.TableName) }
    and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0";

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
            if (checkRow.ConstraintName == null || checkRow.Definition == null)
                continue;

            var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
            var definition = checkRow.Definition;
            var isEnabled = !checkRow.IsDisabled;

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

    private const string ChecksQuerySql = @$"
select
    cc.name as [{ nameof(GetTableChecksQueryResult.ConstraintName) }],
    cc.definition as [{ nameof(GetTableChecksQueryResult.Definition) }],
    cc.is_disabled as [{ nameof(GetTableChecksQueryResult.IsDisabled) }]
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where schema_name(t.schema_id) = @{ nameof(GetTableChecksQuery.SchemaName) } and t.name = @{ nameof(GetTableChecksQuery.TableName) } and t.is_ms_shipped = 0";

    /// <summary>
    /// Retrieves foreign keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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
            row.IsDisabled
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
                    if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                    {
                        var primaryKey = await queryCache.GetPrimaryKeyAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                        return primaryKey.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);
                    var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                    var parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);

                    return parentUniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                        ? OptionAsync<IDatabaseKey>.Some(parentUniqueKeyLookup[parentKeyName.LocalName])
                        : OptionAsync<IDatabaseKey>.None;
                })
                .Map(parentKey =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                    var childKeyColumns = fkey.Value
                        .Where(row => columnLookup.ContainsKey(row.ColumnName))
                        .OrderBy(static row => row.ConstraintColumnId)
                        .Select(row => columnLookup[row.ColumnName])
                        .ToList();

                    var isEnabled = !fkey.Key.IsDisabled;
                    var childKey = new SqlServerDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                    var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                    return new DatabaseRelationalKey(tableName, childKey, resolvedParentTableName!, parentKey, deleteAction, updateAction);
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
    schema_name(parent_t.schema_id) as [{ nameof(GetTableParentKeysQueryResult.ParentTableSchema) }],
    parent_t.name as [{ nameof(GetTableParentKeysQueryResult.ParentTableName) }],
    fk.name as [{ nameof(GetTableParentKeysQueryResult.ChildKeyName) }],
    c.name as [{ nameof(GetTableParentKeysQueryResult.ColumnName) }],
    fkc.constraint_column_id as [{ nameof(GetTableParentKeysQueryResult.ConstraintColumnId) }],
    kc.name as [{ nameof(GetTableParentKeysQueryResult.ParentKeyName) }],
    kc.type as [{ nameof(GetTableParentKeysQueryResult.ParentKeyType) }],
    fk.delete_referential_action as [{ nameof(GetTableParentKeysQueryResult.DeleteAction) }],
    fk.update_referential_action as [{ nameof(GetTableParentKeysQueryResult.UpdateAction) }],
    fk.is_disabled as [{ nameof(GetTableParentKeysQueryResult.IsDisabled) }]
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(child_t.schema_id) = @{ nameof(GetTableParentKeysQuery.SchemaName) } and child_t.name = @{ nameof(GetTableParentKeysQuery.TableName) }
     and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0";

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
                TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.MaxLength,
                NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
            };
            var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var autoIncrement = row.IdentityIncrement
                .Match(
                    incr => row.IdentitySeed.Match(seed => new AutoIncrement(seed, incr), () => Option<IAutoIncrement>.None),
                    static () => Option<IAutoIncrement>.None
                );
            var defaultValue = row.HasDefaultValue && row.DefaultValue != null
                ? Option<string>.Some(row.DefaultValue)
                : Option<string>.None;
            var computedColumnDefinition = !row.ComputedColumnDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.ComputedColumnDefinition)
                : Option<string>.None;

            var column = row.IsComputed
                ? new DatabaseComputedColumn(columnName, columnType, row.IsNullable, defaultValue, computedColumnDefinition)
                : new DatabaseColumn(columnName, columnType, row.IsNullable, defaultValue, autoIncrement);

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
    c.name as [{ nameof(GetTableColumnsQueryResult.ColumnName) }],
    schema_name(st.schema_id) as [{ nameof(GetTableColumnsQueryResult.ColumnTypeSchema) }],
    st.name as [{ nameof(GetTableColumnsQueryResult.ColumnTypeName) }],
    c.max_length as [{ nameof(GetTableColumnsQueryResult.MaxLength) }],
    c.precision as [{ nameof(GetTableColumnsQueryResult.Precision) }],
    c.scale as [{ nameof(GetTableColumnsQueryResult.Scale) }],
    c.collation_name as [{ nameof(GetTableColumnsQueryResult.Collation) }],
    c.is_computed as [{ nameof(GetTableColumnsQueryResult.IsComputed) }],
    c.is_nullable as [{ nameof(GetTableColumnsQueryResult.IsNullable) }],
    dc.parent_column_id as [{ nameof(GetTableColumnsQueryResult.HasDefaultValue) }],
    dc.definition as [{ nameof(GetTableColumnsQueryResult.DefaultValue) }],
    cc.definition as [{ nameof(GetTableColumnsQueryResult.ComputedColumnDefinition) }],
    (convert(bigint, ic.seed_value)) as [{ nameof(GetTableColumnsQueryResult.IdentitySeed) }],
    (convert(bigint, ic.increment_value)) as [{ nameof(GetTableColumnsQueryResult.IdentityIncrement) }]
from sys.tables t
inner join sys.columns c on t.object_id = c.object_id
left join sys.default_constraints dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id
left join sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
left join sys.identity_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id
left join sys.types st on c.user_type_id = st.user_type_id
where schema_name(t.schema_id) = @{ nameof(GetTableColumnsQuery.SchemaName) } and t.name = @{ nameof(GetTableColumnsQuery.TableName) } and t.is_ms_shipped = 0
order by c.column_id";

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
            row.IsInsteadOfTrigger,
            row.IsDisabled
        }).ToList();
        if (triggers.Empty())
            return Array.Empty<IDatabaseTrigger>();

        var result = new List<IDatabaseTrigger>(triggers.Count);
        foreach (var trig in triggers)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
            var queryTiming = trig.Key.IsInsteadOfTrigger ? TriggerQueryTiming.InsteadOf : TriggerQueryTiming.After;
            var definition = trig.Key.Definition;
            var isEnabled = !trig.Key.IsDisabled;

            var events = TriggerEvent.None;
            foreach (var triggerEvent in trig.Value.Select(t => t.TriggerEvent))
            {
                if (string.Equals(triggerEvent, Constants.Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(triggerEvent, Constants.Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(triggerEvent, Constants.Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else
                    throw new UnsupportedTriggerEventException(tableName, triggerEvent);
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

    private const string TriggersQuerySql = @$"
select
    st.name as [{ nameof(GetTableTriggersQueryResult.TriggerName) }],
    sm.definition as [{ nameof(GetTableTriggersQueryResult.Definition) }],
    st.is_instead_of_trigger as [{ nameof(GetTableTriggersQueryResult.IsInsteadOfTrigger) }],
    te.type_desc as [{ nameof(GetTableTriggersQueryResult.TriggerEvent) }],
    st.is_disabled as [{ nameof(GetTableTriggersQueryResult.IsDisabled) }]
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @{ nameof(GetTableTriggersQuery.SchemaName) } and t.name = @{ nameof(GetTableTriggersQuery.TableName) } and t.is_ms_shipped = 0";

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
    /// A mapping from the referential actions as described in SQL Server, to a <see cref="ReferentialAction"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<int, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<int, ReferentialAction>
    {
        [0] = ReferentialAction.NoAction,
        [1] = ReferentialAction.Cascade,
        [2] = ReferentialAction.SetNull,
        [3] = ReferentialAction.SetDefault
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
            key.Name.IfSome(name => result[name.LocalName] = key);

        return result;
    }

    private static class Constants
    {
        public const string Delete = "DELETE";

        public const string Insert = "INSERT";

        public const string PrimaryKeyType = "PK";

        public const string Update = "UPDATE";
    }

    /// <summary>
    /// A query cache provider for SQL Server tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class SqlServerTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache> _foreignKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/> or <paramref name="foreignKeyLoader"/> are <c>null</c>.</exception>
        public SqlServerTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache> columnLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache> foreignKeyLoader
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
