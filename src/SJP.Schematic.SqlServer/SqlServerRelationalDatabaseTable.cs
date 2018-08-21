using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;
using System.Threading;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public SqlServerRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName, IEqualityComparer<Identifier> comparer = null)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            var dialect = database.Dialect;
            if (dialect == null)
                throw new ArgumentException("The given database does not contain a valid dialect.", nameof(database));

            var typeProvider = dialect.TypeProvider;
            TypeProvider = typeProvider ?? throw new ArgumentException("The given database's dialect does not have a valid type provider.", nameof(database));

            var serverName = tableName.Server ?? database.ServerName;
            var databaseName = tableName.Database ?? database.DatabaseName;
            var schemaName = tableName.Schema ?? database.DefaultSchema;

            Name = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, tableName.LocalName);
            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, serverName, databaseName, schemaName);
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadPrimaryKeyAsync(cancellationToken);

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

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName, row.IsDisabled } );
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

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync(CancellationToken cancellationToken)
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

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName, row.IsDisabled });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            var isEnabled = !firstRow.Key.IsDisabled;

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new SqlServerDatabaseKey(this, constraintName, DatabaseKeyType.Primary, columns, isEnabled);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseTableIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseTableIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTableIndex> LoadIndexLookupSync()
        {
            var indexes = Indexes;
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(indexes.Count, Comparer);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken)
        {
            var indexes = await IndexesAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(indexes.Count, Comparer);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseTableIndex> LoadIndexesSync()
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
                return Array.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseTableIndex>();

            var tableColumns = Column;
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { row.IsDescending, Column = tableColumns[row.ColumnName] })
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

        protected virtual async Task<IReadOnlyCollection<IDatabaseTableIndex>> LoadIndexesAsync(CancellationToken cancellationToken)
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
                return Array.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseTableIndex>();

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { row.IsDescending, Column = tableColumns[row.ColumnName] })
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

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys => LoadUniqueKeysSync();

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseKey> LoadUniqueKeyLookupSync()
        {
            var uniqueKeys = UniqueKeys;
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count, Comparer);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> LoadUniqueKeyLookupAsync(CancellationToken cancellationToken)
        {
            var uniqueKeys = await UniqueKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count, Comparer);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseKey> LoadUniqueKeysSync()
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
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName, row.IsDisabled });
            var tableColumns = Column;
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]).ToList(),
                    IsEnabled = !g.Key.IsDisabled
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new SqlServerDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(CancellationToken cancellationToken)
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
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName, row.IsDisabled });
            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]).ToList(),
                    IsEnabled = !g.Key.IsDisabled
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new SqlServerDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
                result.Add(uniqueKey);
            }
            return result;
        }

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChildKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            const string sql = @"
select schema_name(child_t.schema_id) as ChildTableSchema, child_t.name as ChildTableName, fk.name as ChildKeyName, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteRule, fk.update_referential_action as UpdateRule
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @SchemaName and parent_t.name = @TableName";

            var queryResult = Connection.Query<ChildKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
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
                row.DeleteRule,
                row.UpdateRule
            }).ToList();
            if (groupedChildKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
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

                var deleteRule = RelationalRuleMapping[groupedChildKey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[groupedChildKey.Key.UpdateRule];

                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select schema_name(child_t.schema_id) as ChildTableSchema, child_t.name as ChildTableName, fk.name as ChildKeyName, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteRule, fk.update_referential_action as UpdateRule
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(parent_t.schema_id) = @SchemaName and parent_t.name = @TableName";

            var queryResult = await Connection.QueryAsync<ChildKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
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
                row.DeleteRule,
                row.UpdateRule
            }).ToList();
            if (groupedChildKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = await Database.GetTableAsync(childTableName, cancellationToken).ConfigureAwait(false);
                var parentKeyLookup = await childTable.ParentKeyAsync(cancellationToken).ConfigureAwait(false);

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PK")
                {
                    parentKey = await PrimaryKeyAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var uniqueKeyLookup = await UniqueKeyAsync(cancellationToken).ConfigureAwait(false);
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
                }

                var deleteRule = RelationalRuleMapping[groupedChildKey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[groupedChildKey.Key.UpdateRule];
                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule);

                result.Add(relationalKey);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => LoadCheckLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadCheckLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks => LoadChecksSync();

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChecksAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckLookupSync()
        {
            var checks = Checks;
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count, Comparer);

            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckLookupAsync(CancellationToken cancellationToken)
        {
            var checks = await ChecksAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count, Comparer);

            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseCheckConstraint> LoadChecksSync()
        {
            const string sql = @"
select cc.name as ConstraintName, cc.definition as Definition, cc.is_disabled as IsDisabled
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var checks = Connection.Query<CheckConstraintData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = !checkRow.IsDisabled;

                var check = new SqlServerCheckConstraint(this, constraintName, definition, isEnabled);
                result.Add(check);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select cc.name as ConstraintName, cc.definition as Definition, cc.is_disabled as IsDisabled
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where schema_name(t.schema_id) = @SchemaName and t.name = @TableName";

            var checks = await Connection.QueryAsync<CheckConstraintData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = !checkRow.IsDisabled;

                var check = new SqlServerCheckConstraint(this, constraintName, definition, isEnabled);
                result.Add(check);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => LoadParentKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => LoadParentKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeyLookupSync()
        {
            var parentKeys = ParentKeys;
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count, Comparer);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> LoadParentKeyLookupAsync(CancellationToken cancellationToken)
        {
            var parentKeys = await ParentKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count, Comparer);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            const string sql = @"
select schema_name(parent_t.schema_id) as ParentTableSchema, parent_t.name as ParentTableName, fk.name as ChildKeyName, c.name as ColumnName, fkc.constraint_column_id as ConstraintColumnId, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteRule, fk.update_referential_action as UpdateRule, fk.is_disabled as IsDisabled
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(child_t.schema_id) = @SchemaName and child_t.name = @TableName";

            var queryResult = Connection.Query<ForeignKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                row.ChildKeyName,
                row.ParentTableSchema,
                row.ParentTableName,
                row.ParentKeyName,
                KeyType = row.ParentKeyType,
                row.DeleteRule,
                row.UpdateRule,
                row.IsDisabled
            }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

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

                var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = Column;
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var isEnabled = !fkey.Key.IsDisabled;
                var childKey = new SqlServerDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select schema_name(parent_t.schema_id) as ParentTableSchema, parent_t.name as ParentTableName, fk.name as ChildKeyName, c.name as ColumnName, fkc.constraint_column_id as ConstraintColumnId, kc.name as ParentKeyName, kc.type as ParentKeyType, fk.delete_referential_action as DeleteRule, fk.update_referential_action as UpdateRule, fk.is_disabled as IsDisabled
from sys.tables parent_t
inner join sys.foreign_keys fk on parent_t.object_id = fk.referenced_object_id
inner join sys.tables child_t on fk.parent_object_id = child_t.object_id
inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
inner join sys.columns c on fkc.parent_column_id = c.column_id and c.object_id = fkc.parent_object_id
inner join sys.key_constraints kc on kc.unique_index_id = fk.key_index_id and kc.parent_object_id = fk.referenced_object_id
where schema_name(child_t.schema_id) = @SchemaName and child_t.name = @TableName";

            var queryResult = await Connection.QueryAsync<ForeignKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                row.ChildKeyName,
                row.ParentTableSchema,
                row.ParentTableName,
                row.ParentKeyName,
                KeyType = row.ParentKeyType,
                row.DeleteRule,
                row.UpdateRule,
                row.IsDisabled
            }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "PK")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = await parentTable.UniqueKeyAsync(cancellationToken).ConfigureAwait(false);
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = await ColumnAsync(cancellationToken).ConfigureAwait(false);
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var isEnabled = !fkey.Key.IsDisabled;
                var childKey = new SqlServerDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        public IReadOnlyList<IDatabaseTableColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        public IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTableColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(columns.Count, Comparer);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(columns.Count, Comparer);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
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
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = row.IsComputed
                    ? new SqlServerDatabaseComputedTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new SqlServerDatabaseTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseTableColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
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
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = row.IsComputed
                    ? new SqlServerDatabaseComputedTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new SqlServerDatabaseTableColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => LoadTriggerLookupSync();

        public IReadOnlyCollection<IDatabaseTrigger> Triggers => LoadTriggersSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggerLookupAsync(cancellationToken);

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggersAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTrigger> LoadTriggerLookupSync()
        {
            var triggers = Triggers;
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count, Comparer);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> LoadTriggerLookupAsync(CancellationToken cancellationToken)
        {
            var triggers = await TriggersAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count, Comparer);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseTrigger> LoadTriggersSync()
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
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.IsInsteadOfTrigger,
                row.IsDisabled
            }).ToList();
            if (triggers.Count == 0)
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

        protected virtual async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(CancellationToken cancellationToken)
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
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.IsInsteadOfTrigger,
                row.IsDisabled
            }).ToList();
            if (triggers.Count == 0)
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

        protected IReadOnlyDictionary<int, Rule> RelationalRuleMapping { get; } = new Dictionary<int, Rule>
        {
            [0] = Rule.None,
            [1] = Rule.Cascade,
            [2] = Rule.SetNull,
            [3] = Rule.SetDefault
        };
    }
}
