using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabaseTable : IRelationalDatabaseTable, IRelationalDatabaseTableAsync
    {
        public SqlServerRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName, IEqualityComparer<Identifier> comparer = null)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            var serverName = tableName.Server ?? database.ServerName;
            var databaseName = tableName.Database ?? database.DatabaseName;
            var schemaName = tableName.Schema ?? database.DefaultSchema;

            Name = new Identifier(serverName, databaseName, schemaName, tableName.LocalName);
            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, serverName, databaseName, schemaName);
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IDbConnection Connection { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync() => LoadPrimaryKeyAsync();

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            const string sql = @"
select kc.name as ConstraintName, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where
    schema_name(t.schema_id) = @SchemaName and t.name = @TableName
    and kc.type = 'PK'
    and ic.is_included_column = 0
order by ic.key_ordinal";

            var primaryKeyColumns = Connection.Query<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { ConstraintName = row.ConstraintName, IsDisabled = row.IsDisabled } );
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            var isEnabled = !firstRow.Key.IsDisabled;

            var tableColumns = Column;
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new SqlServerDatabaseKey(this, constraintName, DatabaseKeyType.Primary, columns, isEnabled);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync()
        {
            const string sql = @"
select kc.name as ConstraintName, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where
    schema_name(t.schema_id) = @SchemaName and t.name = @TableName
    and kc.type = 'PK'
    and ic.is_included_column = 0
order by ic.key_ordinal";

            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { ConstraintName = row.ConstraintName, IsDisabled = row.IsDisabled });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            var isEnabled = !firstRow.Key.IsDisabled;

            var tableColumns = Column;
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new SqlServerDatabaseKey(this, constraintName, DatabaseKeyType.Primary, columns, isEnabled);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync() => LoadIndexLookupAsync();

        public IEnumerable<IDatabaseTableIndex> Indexes => LoadIndexesSync();

        public Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync() => LoadIndexesAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTableIndex> LoadIndexLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(Comparer);

            foreach (var index in Indexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> LoadIndexLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(Comparer);

            var indexes = await IndexesAsync().ConfigureAwait(false);
            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseTableIndex> LoadIndexesSync()
        {
            const string sql = @"
select i.name as IndexName, i.is_unique as IsUnique, ic.key_ordinal as KeyOrdinal, ic.index_column_id as IndexColumnId, ic.is_included_column as IsIncludedColumn, ic.is_descending_key as IsDescending, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    order by ic.index_id, ic.key_ordinal, ic.index_column_id";

            var queryResult = Connection.Query<IndexColumns>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { IndexName = row.IndexName, IsUnique = row.IsUnique, IsDisabled = row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Enumerable.Empty<IDatabaseTableIndex>();

            var tableColumns = Column;
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new LocalIdentifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { IsDescending = row.IsDescending, Column = tableColumns[row.ColumnName] })
                    .Select(row => new SqlServerDatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedCols = indexInfo
                    .Where(row => row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.ColumnName) // matches SSMS behaviour
                    .Select(row => tableColumns[row.ColumnName])
                    .ToList();

                var index = new SqlServerDatabaseTableIndex(this, indexName, isUnique, indexCols, includedCols, isEnabled);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseTableIndex>> LoadIndexesAsync()
        {
            const string sql = @"
select i.name as IndexName, i.is_unique as IsUnique, ic.key_ordinal as KeyOrdinal, ic.index_column_id as IndexColumnId, ic.is_included_column as IsIncludedColumn, ic.is_descending_key as IsDescending, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    order by ic.index_id, ic.key_ordinal, ic.index_column_id";

            var queryResult = await Connection.QueryAsync<IndexColumns>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { IndexName = row.IndexName, IsUnique = row.IsUnique, IsDisabled = row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Enumerable.Empty<IDatabaseTableIndex>();

            var tableColumns = await ColumnAsync().ConfigureAwait(false);
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new LocalIdentifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { IsDescending = row.IsDescending, Column = tableColumns[row.ColumnName] })
                    .Select(row => new SqlServerDatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedCols = indexInfo
                    .Where(row => row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.ColumnName) // matches SSMS behaviour
                    .Select(row => tableColumns[row.ColumnName])
                    .ToList();

                var index = new SqlServerDatabaseTableIndex(this, indexName, isUnique, indexCols, includedCols, isEnabled);
                result.Add(index);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey => LoadUniqueKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync() => LoadUniqueKeyLookupAsync();

        public IEnumerable<IDatabaseKey> UniqueKeys => LoadUniqueKeysSync();

        public Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync() => LoadUniqueKeysAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseKey> LoadUniqueKeyLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseKey>(Comparer);

            foreach (var uk in UniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> LoadUniqueKeyLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseKey>(Comparer);

            var uniqueKeys = await UniqueKeysAsync().ConfigureAwait(false);
            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseKey> LoadUniqueKeysSync()
        {
            const string sql = @"
select kc.name as ConstraintName, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id
    and ic.column_id = c.column_id
where
    schema_name(t.schema_id) = @SchemaName and t.name = @TableName
    and kc.type = 'UQ'
    and ic.is_included_column = 0
order by ic.key_ordinal";

            var uniqueKeyColumns = Connection.Query<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (uniqueKeyColumns.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { ConstraintName = row.ConstraintName, IsDisabled = row.IsDisabled });
            var tableColumns = Column;
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    ConstraintName = g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]),
                    IsEnabled = !g.Key.IsDisabled
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new SqlServerDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseKey>> LoadUniqueKeysAsync()
        {
            const string sql = @"
select kc.name as ConstraintName, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id
    and ic.column_id = c.column_id
where
    schema_name(t.schema_id) = @SchemaName and t.name = @TableName
    and kc.type = 'UQ'
    and ic.is_included_column = 0
order by ic.key_ordinal";

            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (uniqueKeyColumns.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { ConstraintName = row.ConstraintName, IsDisabled = row.IsDisabled });
            var tableColumns = Column;
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    ConstraintName = g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]),
                    IsEnabled = !g.Key.IsDisabled
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new SqlServerDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
                result.Add(uniqueKey);
            }
            return result;
        }

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => LoadChildKeysAsync();

        protected virtual IEnumerable<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            const string sql = @"
select schema_name(child_t.schema_id) as ChildTableSchema, child_t.name as ChildTableName, fk.name as ChildKeyName, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteAction, fk.update_referential_action as UpdateAction
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @SchemaName and parent_t.name = @TableName";

            var queryResult = Connection.Query<ChildKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var groupedChildKeys = queryResult.GroupBy(row =>
            new
            {
                ChildTableSchema = row.ChildTableSchema,
                ChildTableName = row.ChildTableName,
                ChildKeyName = row.ChildKeyName,
                ParentKeyName = row.ParentKeyName,
                ParentKeyType = row.ParentKeyType,
                DeleteAction = row.DeleteAction,
                UpdateAction = row.UpdateAction
            }).ToList();
            if (groupedChildKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = new LocalIdentifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = new Identifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = Database.GetTable(childTableName);
                var parentKeyLookup = childTable.ParentKey;

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PK")
                {
                    parentKey = PrimaryKey;
                }
                else
                {
                    var uniqueKeyLookup = UniqueKey;
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
                }

                var deleteAction = RelationalActionMapping[groupedChildKey.Key.DeleteAction];
                var updateAction = RelationalActionMapping[groupedChildKey.Key.UpdateAction];

                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteAction, updateAction);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseRelationalKey>> LoadChildKeysAsync()
        {
            const string sql = @"
select schema_name(child_t.schema_id) as ChildTableSchema, child_t.name as ChildTableName, fk.name as ChildKeyName, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteAction, fk.update_referential_action as UpdateAction
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @SchemaName and parent_t.name = @TableName";

            var queryResult = await Connection.QueryAsync<ChildKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var groupedChildKeys = queryResult.GroupBy(row =>
            new
            {
                ChildTableSchema = row.ChildTableSchema,
                ChildTableName = row.ChildTableName,
                ChildKeyName = row.ChildKeyName,
                ParentKeyName = row.ParentKeyName,
                ParentKeyType = row.ParentKeyType,
                DeleteAction = row.DeleteAction,
                UpdateAction = row.UpdateAction
            }).ToList();
            if (groupedChildKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = new LocalIdentifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = new Identifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = await Database.GetTableAsync(childTableName).ConfigureAwait(false);
                var parentKeyLookup = await childTable.ParentKeyAsync().ConfigureAwait(false);

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PK")
                {
                    parentKey = await PrimaryKeyAsync().ConfigureAwait(false);
                }
                else
                {
                    var uniqueKeyLookup = await UniqueKeyAsync().ConfigureAwait(false);
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
                }

                var deleteAction = RelationalActionMapping[groupedChildKey.Key.DeleteAction];
                var updateAction = RelationalActionMapping[groupedChildKey.Key.UpdateAction];
                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteAction, updateAction);

                result.Add(relationalKey);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CheckConstraint => LoadCheckConstraintLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckConstraintAsync() => LoadCheckConstraintLookupAsync();

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints => LoadCheckConstraintsSync();

        public Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync() => LoadCheckConstraintsAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckConstraintLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            foreach (var check in CheckConstraints)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckConstraintLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            var checks = await CheckConstraintsAsync().ConfigureAwait(false);
            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseCheckConstraint> LoadCheckConstraintsSync()
        {
            const string sql = @"
select cc.name as ConstraintName, cc.definition as Definition, cc.is_disabled as IsDisabled
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var checkConstraints = Connection.Query<CheckConstraintData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (checkConstraints.Empty())
                return Enumerable.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();
            var tableColumns = Column;
            foreach (var checkRow in checkConstraints)
            {
                var constraintName = new LocalIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = !checkRow.IsDisabled;

                var checkConstraint = new SqlServerCheckConstraint(this, constraintName, definition, isEnabled);
                result.Add(checkConstraint);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseCheckConstraint>> LoadCheckConstraintsAsync()
        {
            const string sql = @"
select cc.name as ConstraintName, cc.definition as Definition, cc.is_disabled as IsDisabled
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var checkConstraints = await Connection.QueryAsync<CheckConstraintData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (checkConstraints.Empty())
                return Enumerable.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();
            var tableColumns = await ColumnAsync().ConfigureAwait(false);
            foreach (var checkRow in checkConstraints)
            {
                var constraintName = new LocalIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = !checkRow.IsDisabled;

                var checkConstraint = new SqlServerCheckConstraint(this, constraintName, definition, isEnabled);
                result.Add(checkConstraint);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => LoadParentKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync() => LoadParentKeyLookupAsync();

        public IEnumerable<IDatabaseRelationalKey> ParentKeys => LoadParentKeysSync();

        public Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync() => LoadParentKeysAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeyLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(Comparer);

            foreach (var parentKey in ParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> LoadParentKeyLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(Comparer);

            var parentKeys = await ParentKeysAsync().ConfigureAwait(false);
            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            const string sql = @"
select schema_name(parent_t.schema_id) as ParentTableSchema, parent_t.name as ParentTableName, fk.name as ChildKeyName, c.name as ColumnName, fkc.constraint_column_id as ConstraintColumnId, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteAction, fk.update_referential_action as UpdateAction, fk.is_disabled as IsDisabled
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(child_t.schema_id) = @SchemaName and child_t.name = @TableName";

            var queryResult = Connection.Query<ForeignKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                ChildKeyName = row.ChildKeyName,
                ParentTableSchema = row.ParentTableSchema,
                ParentTableName = row.ParentTableName,
                ParentKeyName = row.ParentKeyType,
                KeyType = row.ParentKeyType,
                DeleteAction = row.DeleteAction,
                UpdateAction = row.UpdateAction,
                IsDisabled = row.IsDisabled
            }).ToList();
            if (foreignKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.ConstraintColumnId);

                var parentTableName = new Identifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);
                var parentKeyName = new LocalIdentifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "PK")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = parentTable.UniqueKey;
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = new LocalIdentifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = Column;
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var isEnabled = !fkey.Key.IsDisabled;
                var childKey = new SqlServerDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                var deleteAction = RelationalActionMapping[fkey.Key.DeleteAction];
                var updateAction = RelationalActionMapping[fkey.Key.UpdateAction];

                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteAction, updateAction);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseRelationalKey>> LoadParentKeysAsync()
        {
            const string sql = @"
select schema_name(parent_t.schema_id) as ParentTableSchema, parent_t.name as ParentTableName, fk.name as ChildKeyName, c.name as ColumnName, fkc.constraint_column_id as ConstraintColumnId, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteAction, fk.update_referential_action as UpdateAction, fk.is_disabled as IsDisabled
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(child_t.schema_id) = @SchemaName and child_t.name = @TableName";

            var queryResult = await Connection.QueryAsync<ForeignKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                ChildKeyName = row.ChildKeyName,
                ParentTableSchema = row.ParentTableSchema,
                ParentTableName = row.ParentTableName,
                ParentKeyName = row.ParentKeyType,
                KeyType = row.ParentKeyType,
                DeleteAction = row.DeleteAction,
                UpdateAction = row.UpdateAction,
                IsDisabled = row.IsDisabled
            }).ToList();
            if (foreignKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.ConstraintColumnId);

                var parentTableName = new Identifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName).ConfigureAwait(false);
                var parentKeyName = new LocalIdentifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "PK")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = await parentTable.UniqueKeyAsync().ConfigureAwait(false);
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = new LocalIdentifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = await ColumnAsync().ConfigureAwait(false);
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var isEnabled = !fkey.Key.IsDisabled;
                var childKey = new SqlServerDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                var deleteAction = RelationalActionMapping[fkey.Key.DeleteAction];
                var updateAction = RelationalActionMapping[fkey.Key.UpdateAction];

                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteAction, updateAction);
                result.Add(relationalKey);
            }

            return result;
        }

        public IReadOnlyList<IDatabaseTableColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync() => LoadColumnsAsync();

        public IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync() => LoadColumnLookupAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTableColumn> LoadColumnLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(Comparer);

            var columns = Columns.Where(c => c.Name != null);
            foreach (var column in columns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(Comparer);

            var allColumns = await ColumnsAsync().ConfigureAwait(false);
            var columns = allColumns.Where(c => c.Name != null);
            foreach (var column in columns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyList<IDatabaseTableColumn> LoadColumnsSync()
        {
            const string sql = @"
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
where schema_name(t.schema_id) = @SchemaName
    and t.name = @TableName
    order by c.column_id";

            var query = Connection.Query<ColumnData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            var result = new List<IDatabaseTableColumn>();

            foreach (var row in query)
            {
                var columnTypeName = new Identifier(row.ColumnTypeSchema, row.ColumnTypeName);

                IDbType dbType;
                var columnType = new SqlServerColumnDataType(columnTypeName);
                if (columnType.IsNumericType)
                    dbType = new SqlServerNumericColumnDataType(columnTypeName, row.Precision, row.Scale);
                else if (columnType.IsStringType)
                    dbType = new SqlServerStringColumnDataType(columnTypeName, row.MaxLength, row.Collation);
                else
                    dbType = columnType;

                var columnName = new LocalIdentifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;

                var column = row.IsComputed
                    ? new SqlServerDatabaseComputedTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new SqlServerDatabaseTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, isAutoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseTableColumn>> LoadColumnsAsync()
        {
            const string sql = @"
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
where schema_name(t.schema_id) = @SchemaName
    and t.name = @TableName
    order by c.column_id";

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseTableColumn>();

            foreach (var row in query)
            {
                var columnTypeName = new Identifier(row.ColumnTypeSchema, row.ColumnTypeName);

                IDbType dbType;
                var columnType = new SqlServerColumnDataType(columnTypeName);
                if (columnType.IsNumericType)
                    dbType = new SqlServerNumericColumnDataType(columnTypeName, row.Precision, row.Scale);
                else if (columnType.IsStringType)
                    dbType = new SqlServerStringColumnDataType(columnTypeName, row.MaxLength, row.Collation);
                else
                    dbType = columnType;

                var columnName = new LocalIdentifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;

                var column = row.IsComputed
                    ? new SqlServerDatabaseComputedTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new SqlServerDatabaseTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, isAutoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => LoadTriggerLookupSync();

        public IEnumerable<IDatabaseTrigger> Triggers => LoadTriggersSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync() => LoadTriggerLookupAsync();

        public Task<IEnumerable<IDatabaseTrigger>> TriggersAsync() => LoadTriggersAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTrigger> LoadTriggerLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseTrigger>(Comparer);

            foreach (var trigger in Triggers)
                result[trigger.Name.LocalName] = trigger;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> LoadTriggerLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseTrigger>(Comparer);

            var triggers = await TriggersAsync().ConfigureAwait(false);
            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseTrigger> LoadTriggersSync()
        {
            const string sql = @"
select st.name as TriggerName, sm.definition as Definition, st.is_instead_of_trigger as IsInsteadOfTrigger, te.type_desc as TriggerEvent, st.is_disabled as IsDisabled
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var queryResult = Connection.Query<TriggerData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                TriggerName = row.TriggerName,
                Definition = row.Definition,
                IsInsteadOfTrigger = row.IsInsteadOfTrigger,
                IsDisabled = row.IsDisabled
            }).ToList();
            if (triggers.Count == 0)
                return Enumerable.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = new LocalIdentifier(trig.Key.TriggerName);
                var queryTiming = trig.Key.IsInsteadOfTrigger ? TriggerQueryTiming.Before : TriggerQueryTiming.After;
                var definition = trig.Key.Definition;
                var isEnabled = !trig.Key.IsDisabled;

                var events = TriggerEvent.None;
                foreach (var trigEvent in trig)
                {
                    if (trigEvent.TriggerEvent == "INSERT")
                        events |= TriggerEvent.Insert;
                    else if (trigEvent.TriggerEvent == "UPDATE")
                        events |= TriggerEvent.Update;
                    else if (trigEvent.TriggerEvent == "DELETE")
                        events |= TriggerEvent.Delete;
                    else
                        throw new Exception("Found an unsupported trigger event name. Expected one of INSERT, UPDATE, DELETE, got: " + trigEvent.TriggerEvent);
                }

                var trigger = new SqlServerDatabaseTrigger(this, triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseTrigger>> LoadTriggersAsync()
        {
            const string sql = @"
select st.name as TriggerName, sm.definition as Definition, st.is_instead_of_trigger as IsInsteadOfTrigger, te.type_desc as TriggerEvent, st.is_disabled as IsDisabled
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var queryResult = await Connection.QueryAsync<TriggerData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                TriggerName = row.TriggerName,
                Definition = row.Definition,
                IsInsteadOfTrigger = row.IsInsteadOfTrigger,
                IsDisabled = row.IsDisabled
            }).ToList();
            if (triggers.Count == 0)
                return Enumerable.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = new LocalIdentifier(trig.Key.TriggerName);
                var queryTiming = trig.Key.IsInsteadOfTrigger ? TriggerQueryTiming.Before : TriggerQueryTiming.After;
                var definition = trig.Key.Definition;
                var isEnabled = !trig.Key.IsDisabled;

                var events = TriggerEvent.None;
                foreach (var trigEvent in trig)
                {
                    if (trigEvent.TriggerEvent == "INSERT")
                        events |= TriggerEvent.Insert;
                    else if (trigEvent.TriggerEvent == "UPDATE")
                        events |= TriggerEvent.Update;
                    else if (trigEvent.TriggerEvent == "DELETE")
                        events |= TriggerEvent.Delete;
                    else
                        throw new Exception("Found an unsupported trigger event name. Expected one of INSERT, UPDATE, DELETE, got: " + trigEvent.TriggerEvent);
                }

                var trigger = new SqlServerDatabaseTrigger(this, triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected IReadOnlyDictionary<int, RelationalKeyUpdateAction> RelationalActionMapping { get; } = new Dictionary<int, RelationalKeyUpdateAction>
        {
            [0] = RelationalKeyUpdateAction.NoAction,
            [1] = RelationalKeyUpdateAction.Cascade,
            [2] = RelationalKeyUpdateAction.SetNull,
            [3] = RelationalKeyUpdateAction.SetDefault
        }.AsReadOnlyDictionary();
    }
}
