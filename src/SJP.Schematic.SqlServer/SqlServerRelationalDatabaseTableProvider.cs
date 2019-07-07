using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public SqlServerRelationalDatabaseTableProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
            Dialect = new SqlServerDialect(connection);
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDatabaseDialect Dialect { get; }

        public virtual async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery, cancellationToken).ConfigureAwait(false);
            var tableTasks = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .Select(QualifyTableName)
                .Select(name => LoadTableAsyncCore(name, cancellationToken))
                .ToList();
            return await Task.WhenAll(tableTasks).ConfigureAwait(false);
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @"
select schema_name(schema_id) as SchemaName, name as ObjectName
from sys.tables
where is_ms_shipped = 0
order by schema_name(schema_id), name";

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return LoadTable(candidateTableName, cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = QualifyTableName(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.tables
where schema_id = schema_id(@SchemaName) and name = @TableName and is_ms_shipped = 0";

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return GetResolvedTableName(candidateTableName, cancellationToken)
                .MapAsync(name => LoadTableAsyncCore(name, cancellationToken));
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var columnsTask = LoadColumnsAsync(tableName, cancellationToken);
            var checksTask = LoadChecksAsync(tableName, cancellationToken);
            var triggersTask = LoadTriggersAsync(tableName, cancellationToken);
            await Task.WhenAll(columnsTask, checksTask, triggersTask).ConfigureAwait(false);

            var columns = columnsTask.Result;
            var columnLookup = GetColumnLookup(columns);
            var checks = checksTask.Result;
            var triggers = triggersTask.Result;

            var primaryKeyTask = LoadPrimaryKeyAsync(tableName, columnLookup, cancellationToken);
            var uniqueKeysTask = LoadUniqueKeysAsync(tableName, columnLookup, cancellationToken);
            var indexesTask = LoadIndexesAsync(tableName, columnLookup, cancellationToken);
            await Task.WhenAll(primaryKeyTask, uniqueKeysTask, indexesTask).ConfigureAwait(false);

            var primaryKey = primaryKeyTask.Result;
            var uniqueKeys = uniqueKeysTask.Result;
            var indexes = indexesTask.Result;

            var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

            var childKeysTask = LoadChildKeysAsync(tableName, columnLookup, primaryKey, uniqueKeyLookup, cancellationToken);
            var parentKeysTask = LoadParentKeysAsync(tableName, columnLookup, cancellationToken);
            await Task.WhenAll(childKeysTask, parentKeysTask).ConfigureAwait(false);

            var childKeys = childKeysTask.Result;
            var parentKeys = parentKeysTask.Result;

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

        protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadPrimaryKeyAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(
                PrimaryKeyQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (primaryKeyColumns.Empty())
                return Option<IDatabaseKey>.None;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName, row.IsDisabled });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            var isEnabled = !firstRow.Key.IsDisabled;

            var keyColumns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => columns[row.ColumnName]))
                .ToList();

            var primaryKey = new SqlServerDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, isEnabled);
            return Option<IDatabaseKey>.Some(primaryKey);
        }

        protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

        private const string PrimaryKeyQuerySql = @"
select
    kc.name as ConstraintName,
    c.name as ColumnName,
    i.is_disabled as IsDisabled
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName and t.is_ms_shipped = 0
    and kc.type = 'PK' and ic.is_included_column = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.key_ordinal";

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
            var queryResult = await Connection.QueryAsync<IndexColumns>(
                IndexesQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
            if (indexColumns.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { row.IsDescending, Column = columns[row.ColumnName] })
                    .Select(row =>
                    {
                        var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var column = row.Column;
                        var expression = Dialect.QuoteName(column.Name);
                        return new DatabaseIndexColumn(expression, column, order);
                    })
                    .ToList();

                var includedCols = indexInfo
                    .Where(row => row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.ColumnName) // matches SSMS behaviour
                    .Select(row => columns[row.ColumnName])
                    .ToList();

                var index = new DatabaseIndex(indexName, isUnique, indexCols, includedCols, isEnabled);
                result.Add(index);
            }

            return result;
        }

        protected virtual string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @"
select
    i.name as IndexName,
    i.is_unique as IsUnique,
    ic.key_ordinal as KeyOrdinal,
    ic.index_column_id as IndexColumnId,
    ic.is_included_column as IsIncludedColumn,
    ic.is_descending_key as IsDescending,
    c.name as ColumnName,
    i.is_disabled as IsDisabled
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName and t.is_ms_shipped = 0
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.index_id, ic.key_ordinal, ic.index_column_id";

        protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadUniqueKeysAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(
                UniqueKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName, row.IsDisabled });
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => columns[row.ColumnName]).ToList(),
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

        protected virtual string UniqueKeysQuery => UniqueKeysQuerySql;

        private const string UniqueKeysQuerySql = @"
select
    kc.name as ConstraintName,
    c.name as ColumnName,
    i.is_disabled as IsDisabled
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where
    schema_name(t.schema_id) = @SchemaName and t.name = @TableName and t.is_ms_shipped = 0
    and kc.type = 'UQ'
    and ic.is_included_column = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore
order by ic.key_ordinal";

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, Option<IDatabaseKey> primaryKey, IReadOnlyDictionary<Identifier, IDatabaseKey> uniqueKeys, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (uniqueKeys == null)
                throw new ArgumentNullException(nameof(uniqueKeys));

            return LoadChildKeysAsyncCore(tableName, columns, primaryKey, uniqueKeys, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, Option<IDatabaseKey> primaryKey, IReadOnlyDictionary<Identifier, IDatabaseKey> uniqueKeys, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ChildKeyData>(
                ChildKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var groupedChildKeys = queryResult.GroupBy(row =>
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

            var tableNameCache = new Dictionary<Identifier, Identifier> { [Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName)] = tableName };
            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var foreignKeyLookupCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();
            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);

            foreach (var groupedChildKey in groupedChildKeys)
            {
                // ensure we have a key to begin with
                IDatabaseKey parentKey = null;
                if (groupedChildKey.Key.ParentKeyType == Constants.PrimaryKeyType)
                    primaryKey.IfSome(k => parentKey = k);
                else if (uniqueKeys.ContainsKey(groupedChildKey.Key.ParentKeyName))
                    parentKey = uniqueKeys[groupedChildKey.Key.ParentKeyName];
                if (parentKey == null)
                    continue;

                var candidateChildTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTableNameOption = tableNameCache.ContainsKey(candidateChildTableName)
                    ? OptionAsync<Identifier>.Some(tableNameCache[candidateChildTableName])
                    : GetResolvedTableName(candidateChildTableName, cancellationToken);

                await childTableNameOption
                    .BindAsync(async childTableName =>
                    {
                        tableNameCache[candidateChildTableName] = childTableName;
                        var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                        if (!columnLookupsCache.TryGetValue(childTableName, out var childKeyColumnLookup))
                        {
                            var childKeyColumns = await LoadColumnsAsync(childTableName, cancellationToken).ConfigureAwait(false);
                            childKeyColumnLookup = GetColumnLookup(childKeyColumns);
                            columnLookupsCache[tableName] = childKeyColumnLookup;
                        }

                        if (!foreignKeyLookupCache.TryGetValue(childTableName, out var parentKeyLookup))
                        {
                            var parentKeys = await LoadParentKeysAsync(childTableName, childKeyColumnLookup, cancellationToken).ConfigureAwait(false);
                            parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());
                            foreignKeyLookupCache[tableName] = parentKeyLookup;
                        }

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

        protected virtual string ChildKeysQuery => ChildKeysQuerySql;

        private const string ChildKeysQuerySql = @"
select
    schema_name(child_t.schema_id) as ChildTableSchema,
    child_t.name as ChildTableName,
    fk.name as ChildKeyName,
    kc.name as ParentKeyName,
    kc.type as ParentKeyType,
    fk.delete_referential_action as DeleteAction,
    fk.update_referential_action as UpdateAction
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @SchemaName and parent_t.name = @TableName
    and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0";

        protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            var checks = await Connection.QueryAsync<CheckConstraintData>(
                ChecksQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = !checkRow.IsDisabled;

                var check = new DatabaseCheckConstraint(constraintName, definition, isEnabled);
                result.Add(check);
            }

            return result;
        }

        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @"
select
    cc.name as ConstraintName,
    cc.definition as Definition,
    cc.is_disabled as IsDisabled
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName and t.is_ms_shipped = 0";

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadParentKeysAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ForeignKeyData>(
                ParentKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
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

            var tableNameCache = new Dictionary<Identifier, Identifier> { [Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName)] = tableName };
            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var primaryKeyCache = new Dictionary<Identifier, Option<IDatabaseKey>>();
            var uniqueKeyLookupCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTableNameOption = tableNameCache.ContainsKey(candidateParentTableName)
                    ? OptionAsync<Identifier>.Some(tableNameCache[candidateParentTableName])
                    : GetResolvedTableName(candidateParentTableName, cancellationToken);

                await parentTableNameOption
                    .BindAsync(async parentTableName =>
                    {
                        tableNameCache[candidateParentTableName] = parentTableName;

                        if (fkey.Key.KeyType == Constants.PrimaryKeyType)
                        {
                            if (primaryKeyCache.TryGetValue(parentTableName, out var pk))
                                return pk.ToAsync();

                            if (!columnLookupsCache.TryGetValue(parentTableName, out var parentColumnLookup))
                            {
                                var parentColumns = await LoadColumnsAsync(parentTableName, cancellationToken)
                                    .ConfigureAwait(false);
                                parentColumnLookup = GetColumnLookup(parentColumns);
                                columnLookupsCache[parentTableName] = parentColumnLookup;
                            }

                            var parentKeyOption =
                                await LoadPrimaryKeyAsync(parentTableName, parentColumnLookup, cancellationToken)
                                    .ConfigureAwait(false);
                            primaryKeyCache[parentTableName] = parentKeyOption;
                            return parentKeyOption.ToAsync();
                        }
                        else
                        {
                            var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);
                            if (uniqueKeyLookupCache.TryGetValue(parentTableName, out var uks) && uks.ContainsKey(parentKeyName.LocalName))
                                return OptionAsync<IDatabaseKey>.Some(uks[parentKeyName.LocalName]);

                            if (!columnLookupsCache.TryGetValue(parentTableName, out var parentColumnLookup))
                            {
                                var parentColumns = await LoadColumnsAsync(parentTableName, cancellationToken)
                                    .ConfigureAwait(false);
                                parentColumnLookup = GetColumnLookup(parentColumns);
                                columnLookupsCache[parentTableName] = parentColumnLookup;
                            }

                            var parentUniqueKeys =
                                await LoadUniqueKeysAsync(parentTableName, parentColumnLookup, cancellationToken)
                                    .ConfigureAwait(false);
                            var parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);
                            uniqueKeyLookupCache[parentTableName] = parentUniqueKeyLookup;

                            return parentUniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                                ? OptionAsync<IDatabaseKey>.Some(parentUniqueKeyLookup[parentKeyName.LocalName])
                                : OptionAsync<IDatabaseKey>.None;
                        }
                    })
                    .Map((Func<IDatabaseKey, DatabaseRelationalKey>)(parentKey =>
                    {
                        var parentTableName = tableNameCache[candidateParentTableName];
                        var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                        var childKeyColumns = fkey
                            .OrderBy(row => row.ConstraintColumnId)
                            .Select(row => columns[row.ColumnName])
                            .ToList();

                        var isEnabled = !fkey.Key.IsDisabled;
                        var childKey = new SqlServerDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                        var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                        var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                        return new DatabaseRelationalKey(tableName, childKey, parentTableName, parentKey, deleteAction, updateAction);
                    }))
                    .IfSome(relationalKey => result.Add(relationalKey))
                    .ConfigureAwait(false);
            }

            return result;
        }

        protected virtual string ParentKeysQuery => ParentKeysQuerySql;

        private const string ParentKeysQuerySql = @"
select
    schema_name(parent_t.schema_id) as ParentTableSchema,
    parent_t.name as ParentTableName,
    fk.name as ChildKeyName,
    c.name as ColumnName,
    fkc.constraint_column_id as ConstraintColumnId,
    kc.name as ParentKeyName,
    kc.type as ParentKeyType,
    fk.delete_referential_action as DeleteAction,
    fk.update_referential_action as UpdateAction,
    fk.is_disabled as IsDisabled
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(child_t.schema_id) = @SchemaName and child_t.name = @TableName
     and child_t.is_ms_shipped = 0 and parent_t.is_ms_shipped = 0";

        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadColumnsAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(
                ColumnsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.MaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value))
                    : Option<IAutoIncrement>.None;
                var defaultValue = row.HasDefaultValue
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

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
select
    c.name as ColumnName,
    schema_name(st.schema_id) as ColumnTypeSchema,
    st.name as ColumnTypeName,
    c.max_length as MaxLength,
    c.precision as Precision,
    c.scale as Scale,
    c.collation_name as Collation,
    c.is_computed as IsComputed,
    c.is_nullable as IsNullable,
    dc.parent_column_id as HasDefaultValue,
    dc.definition as DefaultValue,
    cc.definition as ComputedColumnDefinition,
    (convert(bigint, ic.seed_value)) as IdentitySeed,
    (convert(bigint, ic.increment_value)) as IdentityIncrement
from sys.tables t
inner join sys.columns c on t.object_id = c.object_id
left join sys.default_constraints dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id
left join sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
left join sys.identity_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id
left join sys.types st on c.user_type_id = st.user_type_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName and t.is_ms_shipped = 0
order by c.column_id";

        protected virtual Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            return LoadTriggersAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<TriggerData>(
                TriggersQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
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
                foreach (var trigEvent in trig)
                {
                    if (trigEvent.TriggerEvent == Constants.Insert)
                        events |= TriggerEvent.Insert;
                    else if (trigEvent.TriggerEvent == Constants.Update)
                        events |= TriggerEvent.Update;
                    else if (trigEvent.TriggerEvent == Constants.Delete)
                        events |= TriggerEvent.Delete;
                    else
                        throw new UnsupportedTriggerEventException(tableName, trigEvent.TriggerEvent);
                }

                var trigger = new DatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual string TriggersQuery => TriggersQuerySql;

        private const string TriggersQuerySql = @"
select
    st.name as TriggerName,
    sm.definition as Definition,
    st.is_instead_of_trigger as IsInsteadOfTrigger,
    te.type_desc as TriggerEvent,
    st.is_disabled as IsDisabled
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName and t.is_ms_shipped = 0";

        protected Identifier QualifyTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var schema = tableName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
        }

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
    }
}
