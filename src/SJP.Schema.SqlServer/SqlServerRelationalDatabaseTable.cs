using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schema.Core;
using SJP.Schema.Core.Utilities;
using SJP.Schema.SqlServer.Query;

namespace SJP.Schema.SqlServer
{
    public class SqlServerRelationalDatabaseTable : IRelationalDatabaseTable, IRelationalDatabaseTableAsync
    {
        public SqlServerRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Name = tableName ?? throw new ArgumentNullException(nameof(tableName));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            _dependencies = new AsyncLazy<IEnumerable<Identifier>>(LoadDependenciesAsync);
            _dependents = new AsyncLazy<IEnumerable<Identifier>>(LoadDependentsAsync);

            _columnList = new AsyncLazy<IList<IDatabaseTableColumn>>(LoadColumnsAsync);
            _columnLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableColumn>>(LoadColumnLookupAsync);

            _checks = new AsyncLazy<IEnumerable<IDatabaseCheckConstraint>>(LoadCheckConstraintsAsync);
            _checkLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseCheckConstraint>>(LoadCheckConstraintLookupAsync);

            _childKeys = new AsyncLazy<IEnumerable<IDatabaseRelationalKey>>(LoadChildKeysAsync);

            _indexes = new AsyncLazy<IEnumerable<IDatabaseTableIndex>>(LoadIndexesAsync);
            _indexLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableIndex>>(LoadIndexLookupAsync);

            _parentKeys = new AsyncLazy<IEnumerable<IDatabaseRelationalKey>>(LoadParentKeysAsync);
            _parentKeyLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseRelationalKey>>(LoadParentKeyLookupAsync);

            _primaryKey = new AsyncLazy<IDatabaseKey>(LoadPrimaryKeyAsync);

            _triggers = new AsyncLazy<IEnumerable<IDatabaseTrigger>>(LoadTriggersAsync);
            _triggerLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseTrigger>>(LoadTriggerLookupAsync);

            _uniqueKeys = new AsyncLazy<IEnumerable<IDatabaseKey>>(LoadUniqueKeysAsync);
            _uniqueKeyLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseKey>>(LoadUniqueKeyLookupAsync);
        }

        private async Task<IEnumerable<Identifier>> LoadDependentsAsync()
        {
            const string sql = @"
select schema_name(o1.schema_id) as ReferencingSchemaName, o1.name as ReferencingObjectName, o1.type_desc as ReferencingObjectType,
    schema_name(o2.schema_id) as ReferencedSchemaName, o2.name as ReferencedObjectName, o2.type_desc as ReferencedObjectType
from sys.sql_expression_dependencies sed
inner join sys.objects o1 on sed.referencing_id = o1.object_id
inner join sys.objects o2 on sed.referenced_id = o2.object_id
where o2.schema_id = schema_id(@SchemaName) and o2.name = @TableName
    and o2.type = 'U' and sed.referenced_minor_id = 0
UNION
select schema_name(o1.schema_id) as ReferencingSchemaName, o1.name as ReferencingObjectName, o1.type_desc as ReferencingObjectType,
    schema_name(o2.schema_id) as ReferencedSchemaName, o2.name as ReferencedObjectName, o2.type_desc as ReferencedObjectType
from sys.foreign_keys fk
inner join sys.objects o1 on fk.parent_object_id = o1.object_id
inner join sys.objects o2 on fk.referenced_object_id = o2.object_id
where o2.schema_id = schema_id(@SchemaName) and o2.name = @TableName
    and o2.type = 'U'";

            var dependents = await Connection.QueryAsync<DependencyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (dependents.Empty())
                return null;

            return dependents
                .Select(d => new Identifier(d.ReferencingSchemaName, d.ReferencingObjectName))
                .ToList();
        }

        private async Task<IEnumerable<Identifier>> LoadDependenciesAsync()
        {
            const string sql = @"
select schema_name(o1.schema_id) as ReferencingSchemaName, o1.name as ReferencingObjectName, o1.type_desc as ReferencingObjectType,
    schema_name(o2.schema_id) as ReferencedSchemaName, o2.name as ReferencedObjectName, o2.type_desc as ReferencedObjectType
from sys.sql_expression_dependencies sed
inner join sys.objects o1 on sed.referencing_id = o1.object_id
inner join sys.objects o2 on sed.referenced_id = o2.object_id
where o1.schema_id = schema_id(@SchemaName) and o1.name = @TableName
    and o1.type = 'U' and sed.referencing_minor_id = 0
UNION
select schema_name(o1.schema_id) as ReferencingSchemaName, o1.name as ReferencingObjectName, o1.type_desc as ReferencingObjectType,
    schema_name(o2.schema_id) as ReferencedSchemaName, o2.name as ReferencedObjectName, o2.type_desc as ReferencedObjectType
from sys.foreign_keys fk
inner join sys.objects o1 on fk.parent_object_id = o1.object_id
inner join sys.objects o2 on fk.referenced_object_id = o2.object_id
where o1.schema_id = schema_id(@SchemaName) and o1.name = @TableName
    and o1.type = 'U'";

            var dependencies = await Connection.QueryAsync<DependencyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (dependencies.Empty())
                return null;

            return dependencies
                .Select(d => new Identifier(d.ReferencedSchemaName, d.ReferencedObjectName))
                .ToList();
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IDbConnection Connection { get; }

        public IEnumerable<Identifier> Dependencies => _dependencies.Task.Result;

        public Task<IEnumerable<Identifier>> DependenciesAsync() => _dependencies.Task;

        public IEnumerable<Identifier> Dependents => _dependents.Task.Result;

        public Task<IEnumerable<Identifier>> DependentsAsync() => _dependents.Task;

        public IDatabaseKey PrimaryKey => _primaryKey.Task.Result;

        public Task<IDatabaseKey> PrimaryKeyAsync() => _primaryKey.Task;

        public IList<IDatabaseTableColumn> Columns => _columnList.Task.Result;

        public Task<IList<IDatabaseTableColumn>> ColumnsAsync() => _columnList.Task;

        public IReadOnlyDictionary<string, IDatabaseTableColumn> Column => _columnLookup.Task.Result;

        public Task<IReadOnlyDictionary<string, IDatabaseTableColumn>> ColumnAsync() => _columnLookup.Task;

        public IReadOnlyDictionary<string, IDatabaseRelationalKey> ParentKey => _parentKeyLookup.Task.Result;

        public Task<IReadOnlyDictionary<string, IDatabaseRelationalKey>> ParentKeyAsync() => _parentKeyLookup.Task;

        public IEnumerable<IDatabaseRelationalKey> ParentKeys => _parentKeys.Task.Result;

        public Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync() => _parentKeys.Task;

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => _childKeys.Task.Result;

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => _childKeys.Task;

        public IReadOnlyDictionary<string, IDatabaseKey> UniqueKey => _uniqueKeyLookup.Task.Result;

        public Task<IReadOnlyDictionary<string, IDatabaseKey>> UniqueKeyAsync() => _uniqueKeyLookup.Task;

        public IEnumerable<IDatabaseKey> UniqueKeys => _uniqueKeys.Task.Result;

        public Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync() => _uniqueKeys.Task;

        public IReadOnlyDictionary<string, IDatabaseTableIndex> Index => _indexLookup.Task.Result;

        public Task<IReadOnlyDictionary<string, IDatabaseTableIndex>> IndexAsync() => _indexLookup.Task;

        public IEnumerable<IDatabaseTableIndex> Indexes => _indexes.Task.Result;

        public Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync() => _indexes.Task;

        public IReadOnlyDictionary<string, IDatabaseCheckConstraint> CheckConstraint => _checkLookup.Task.Result;

        public Task<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> CheckConstraintAsync() => _checkLookup.Task;

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints => _checks.Task.Result;

        public Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync() => _checks.Task;

        public IReadOnlyDictionary<string, IDatabaseTrigger> Trigger => _triggerLookup.Task.Result;

        public IEnumerable<IDatabaseTrigger> Triggers => _triggers.Task.Result;

        public Task<IReadOnlyDictionary<string, IDatabaseTrigger>> TriggerAsync() => _triggerLookup.Task;

        public Task<IEnumerable<IDatabaseTrigger>> TriggersAsync() => _triggers.Task;

        private async Task<IDatabaseKey> LoadPrimaryKeyAsync()
        {
            const string sql = @"
select kc.name as ConstraintName, c.name as ColumnName
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
inner join sys.indexes i on kc.parent_object_id = i.object_id and kc.unique_index_id = i.index_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id
    and ic.column_id = c.column_id
where
    SCHEMA_NAME(t.schema_id) = @SchemaName and t.name = @TableName
    and kc.type = 'PK'
    and ic.is_included_column = 0
order by ic.column_id";

            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => row.ConstraintName);
            var firstName = groupedByName.First().Key;
            var tableColumns = await ColumnAsync();
            var columns = groupedByName
                .Where(row => row.Key == firstName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            var pkName = new LocalIdentifier(firstName);
            return new SqlServerDatabaseKey(this, pkName, DatabaseKeyType.Primary, columns);
        }

        private async Task<IReadOnlyDictionary<string, IDatabaseTableIndex>> LoadIndexLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseTableIndex>();

            var indexes = await IndexesAsync();
            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseTableIndex>> LoadIndexesAsync()
        {
            const string sql = @"
select i.name as IndexName, i.is_unique as IsUnique, ic.key_ordinal as KeyOrdinal, ic.index_column_id as IndexColumnId, ic.is_included_column as IsIncludedColumn, ic.is_descending_key as IsDescending, c.name as ColumnName
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    order by ic.index_id, ic.key_ordinal, ic.index_column_id";

            var queryResult = await Connection.QueryAsync<IndexColumns>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { IndexName = row.IndexName, IsUnique = row.IsUnique });

            var tableColumns = await ColumnAsync();
            var result = new List<IDatabaseTableIndex>();
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new LocalIdentifier(indexInfo.Key.IndexName);
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

                var index = new SqlServerDatabaseTableIndex(this, indexName, isUnique, indexCols, includedCols);
                result.Add(index);
            }

            return result;
        }

        private async Task<IReadOnlyDictionary<string, IDatabaseKey>> LoadUniqueKeyLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseKey>();

            var uniqueKeys = await UniqueKeysAsync();
            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseKey>> LoadUniqueKeysAsync()
        {
            const string sql = @"
select kc.name as ConstraintName, c.name as ColumnName
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
order by ic.column_id";

            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (uniqueKeyColumns.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => row.ConstraintName);
            var tableColumns = await ColumnAsync();
            var constraintColumns = groupedByName
                .Select(g => new { ConstraintName = g.Key, Columns = g.Select(row => tableColumns[row.ColumnName]) })
                .ToList();

            var result = new List<IDatabaseKey>();
            foreach (var uk in constraintColumns)
            {
                var ukName = new LocalIdentifier(uk.ConstraintName);
                var ukColumns = uk.Columns;
                var uniqueKey = new SqlServerDatabaseKey(this, ukName, DatabaseKeyType.Unique, ukColumns);
                result.Add(uniqueKey);
            }
            return result.ToList();
        }

        private async Task<IEnumerable<IDatabaseRelationalKey>> LoadChildKeysAsync()
        {
            const string sql = @"
select schema_name(child_t.schema_id) as ChildTableSchema, child_t.name as ChildTableName, fk.name as ChildKeyName, c.name as ColumnName, fkc.constraint_column_id as ConstraintColumnId, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteAction, fk.update_referential_action as UpdateAction
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @SchemaName and parent_t.name = @TableName";

            var queryResult = await Connection.QueryAsync<ChildKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
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
            });

            var result = new List<IDatabaseRelationalKey>();
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var rows = groupedChildKey.OrderBy(row => row.ConstraintColumnId);
                var childKeyName = new LocalIdentifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = new Identifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = await Database.TableAsync(childTableName);
                var parentKeyLookup = await childTable.ParentKeyAsync();

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                var tmpColumns = await ColumnAsync(); // trigger column cache
                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PK")
                {
                    parentKey = await PrimaryKeyAsync();
                }
                else
                {
                    var uniqueKeyLookup = await UniqueKeyAsync();
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
                }

                var deleteAction = RelationalActionMapping[groupedChildKey.Key.DeleteAction];
                var updateAction = RelationalActionMapping[groupedChildKey.Key.UpdateAction];
                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteAction, updateAction);

                result.Add(relationalKey);
            }

            return result;
        }

        private async Task<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> LoadCheckConstraintLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseCheckConstraint>();

            var checks = await CheckConstraintsAsync();
            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseCheckConstraint>> LoadCheckConstraintsAsync()
        {
            const string sql = @"
select cc.name as ConstraintName, cc.definition as Definition, c.name as DependentColumnName
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
inner join sys.sql_expression_dependencies sed on sed.referencing_id = cc.object_id
left join sys.columns c on t.object_id = c.object_id and sed.referenced_minor_id = c.column_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var queryResult = await Connection.QueryAsync<CheckConstraintData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseCheckConstraint>();

            var checkConstraints = queryResult.GroupBy(row => new { ConstraintName = row.ConstraintName, Definition = row.Definition });

            var result = new List<IDatabaseCheckConstraint>();
            var tableColumns = await ColumnAsync();
            foreach (var checkRow in checkConstraints)
            {
                var constraintName = new LocalIdentifier(checkRow.Key.ConstraintName);
                var definition = checkRow.Key.Definition;
                var columns = checkRow.Select(row => tableColumns[row.DependentColumnName]).ToList();

                var checkConstraint = new SqlServerCheckConstraint(this, constraintName, definition, columns);
                result.Add(checkConstraint);
            }

            return result;
        }

        private async Task<IReadOnlyDictionary<string, IDatabaseRelationalKey>> LoadParentKeyLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseRelationalKey>();

            var parentKeys = await ParentKeysAsync();
            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseRelationalKey>> LoadParentKeysAsync()
        {
            const string sql = @"
select schema_name(t.schema_id) as ParentTableSchema, t.name as ParentTableName, fk.name as ForeignKeyName, c.name as ColumnName, fkc.constraint_column_id as ConstraintColumnId, kc.name as ParentKeyName, kc.type as KeyType, fk.delete_referential_action as DeleteAction, fk.update_referential_action as UpdateAction
from sys.tables t
inner join sys.foreign_keys fk on t.object_id = fk.parent_object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where t.name = @TableName and schema_name(t.schema_id) = @SchemaName";

            var queryResult = await Connection.QueryAsync<ForeignKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                ForeignKeyName = row.ForeignKeyName,
                ParentTableSchema = row.ParentTableSchema,
                ParentTableName = row.ParentTableName,
                ParentKeyName = row.ParentKeyName,
                KeyType = row.KeyType,
                DeleteAction = row.DeleteAction,
                UpdateAction = row.UpdateAction
            });

            var result = new List<IDatabaseRelationalKey>();
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.ConstraintColumnId);
                var name = new LocalIdentifier(fkey.Key.ForeignKeyName);

                var parentTableName = new Identifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = await Database.TableAsync(parentTableName);
                var parentKeyName = new LocalIdentifier(fkey.Key.ParentKeyName);

                IEnumerable<IDatabaseColumn> parentColumns = await parentTable.ColumnsAsync(); // triggering column load async if not cached
                DatabaseKeyType parentKeyType;
                if (fkey.Key.KeyType == "PK")
                {
                    parentKeyType = DatabaseKeyType.Primary;
                    var parentPk = await parentTable.PrimaryKeyAsync();
                    parentColumns = parentPk.Columns;
                }
                else
                {
                    parentKeyType = DatabaseKeyType.Unique;
                    var uniqueKeys = await parentTable.UniqueKeyAsync();
                    var uk = uniqueKeys[parentKeyName.LocalName];
                    parentColumns = uk.Columns;
                }

                var parentKey = new SqlServerDatabaseKey(this, parentKeyName, parentKeyType, parentColumns);

                var childKeyName = new LocalIdentifier(fkey.Key.ForeignKeyName);
                var childKeyColumnLookup = await ColumnAsync(); // trigger Column load if not already cached
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName]);

                var childKey = new SqlServerDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteAction = RelationalActionMapping[fkey.Key.DeleteAction];
                var updateAction = RelationalActionMapping[fkey.Key.UpdateAction];

                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteAction, updateAction);
                result.Add(relationalKey);
            }

            return result;
        }

        private async Task<IReadOnlyDictionary<string, IDatabaseTableColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseTableColumn>();

            var allColumns = await ColumnsAsync();
            var columns = allColumns.Where(c => c.Name != null);
            foreach (var column in columns)
                result[column.Name.LocalName] = column;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IList<IDatabaseTableColumn>> LoadColumnsAsync()
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

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            var result = new List<IDatabaseTableColumn>();

            foreach (var row in query)
            {
                var columnTypeName = new Identifier(row.ColumnTypeSchema, row.ColumnTypeName);

                IDbType dbType;
                var columnType = new SqlServerColumnDataType(columnTypeName);
                if (columnType.IsNumericType)
                    dbType = new SqlServerNumericColumnDataType(columnTypeName, row.Precision, row.Scale); // new SqlServerDatabaseNumericColumnType(columnTypeName, row.MaxLength, row.Precision, row.Scale);
                else if (columnType.IsStringType)
                    dbType = new SqlServerStringColumnDataType(columnTypeName, row.MaxLength, row.Collation);
                else
                    dbType = columnType;

                var columnName = new LocalIdentifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;

                var column = row.IsComputed
                    ? new SqlServerDatabaseComputedTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue)
                    : new SqlServerDatabaseTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, isAutoIncrement);

                result.Add(column);
            }

            return result;
        }

        private async Task<IReadOnlyDictionary<string, IDatabaseTrigger>> LoadTriggerLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseTrigger>();

            var triggers = await TriggersAsync();
            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseTrigger>> LoadTriggersAsync()
        {
            const string sql = @"
select st.name as TriggerName, sm.definition as Definition, st.is_instead_of_trigger as IsInsteadOfTrigger, te.type_desc as TriggerEvent
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var queryResult = await Connection.QueryAsync<TriggerData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new { TriggerName = row.TriggerName, Definition = row.Definition, IsInsteadOfTrigger = row.IsInsteadOfTrigger });

            var result = new List<IDatabaseTrigger>();
            foreach (var trig in triggers)
            {
                var triggerName = new LocalIdentifier(trig.Key.TriggerName);
                var queryTiming = trig.Key.IsInsteadOfTrigger ? TriggerQueryTiming.Before : TriggerQueryTiming.After;
                var definition = trig.Key.Definition;
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

                var trigger = new SqlServerDatabaseTrigger(this, triggerName, definition, queryTiming, events);
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
        };

        private readonly AsyncLazy<IEnumerable<Identifier>> _dependencies;
        private readonly AsyncLazy<IEnumerable<Identifier>> _dependents;

        private readonly AsyncLazy<IDatabaseKey> _primaryKey;

        private readonly AsyncLazy<IList<IDatabaseTableColumn>> _columnList;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableColumn>> _columnLookup;

        private readonly AsyncLazy<IEnumerable<IDatabaseCheckConstraint>> _checks;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> _checkLookup;

        private readonly AsyncLazy<IEnumerable<IDatabaseRelationalKey>> _parentKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseRelationalKey>> _parentKeyLookup;

        private readonly AsyncLazy<IEnumerable<IDatabaseRelationalKey>> _childKeys;

        private readonly AsyncLazy<IEnumerable<IDatabaseKey>> _uniqueKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseKey>> _uniqueKeyLookup;

        private readonly AsyncLazy<IEnumerable<IDatabaseTableIndex>> _indexes;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableIndex>> _indexLookup;

        private readonly AsyncLazy<IEnumerable<IDatabaseTrigger>> _triggers;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseTrigger>> _triggerLookup;
    }
}
