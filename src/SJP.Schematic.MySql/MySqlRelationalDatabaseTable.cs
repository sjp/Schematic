using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.MySql.Query;
using SJP.Schematic.Core.Extensions;
using System.Threading;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public MySqlRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName, IEqualityComparer<Identifier> comparer = null)
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

            Name = new Identifier(serverName, databaseName, schemaName, tableName.LocalName);
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
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where
    t.table_schema = @SchemaName and t.table_name = @TableName
    and tc.constraint_type = 'PRIMARY KEY'
order by kc.ordinal_position";

            var primaryKeyColumns = Connection.Query<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var tableColumns = Column;
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new MySqlDatabasePrimaryKey(this, columns);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where
    t.table_schema = @SchemaName and t.table_name = @TableName
    and tc.constraint_type = 'PRIMARY KEY'
order by kc.ordinal_position";

            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new MySqlDatabasePrimaryKey(this, columns);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseTableIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseTableIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTableIndex> LoadIndexLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(Comparer);

            foreach (var index in Indexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken)
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(Comparer);

            var indexes = await IndexesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyCollection<IDatabaseTableIndex> LoadIndexesSync()
        {
            const string sql = @"
select
    index_name as IndexName,
    non_unique as IsNonUnique,
    seq_in_index as ColumnOrdinal,
    column_name as ColumnName
from information_schema.statistics
where table_schema = @SchemaName and table_name = @TableName";

            var queryResult = Connection.Query<IndexColumns>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsNonUnique }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseTableIndex>();

            var tableColumns = Column;
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = !indexInfo.Key.IsNonUnique;
                var indexName = new Identifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnOrdinal)
                    .Select(row => tableColumns[row.ColumnName])
                    .Select(col => new MySqlDatabaseIndexColumn(col))
                    .ToList();

                var index = new MySqlDatabaseTableIndex(this, indexName, isUnique, indexCols);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseTableIndex>> LoadIndexesAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select
    index_name as IndexName,
    non_unique as IsNonUnique,
    seq_in_index as ColumnOrdinal,
    column_name as ColumnName
from information_schema.statistics
where table_schema = @SchemaName and table_name = @TableName";

            var queryResult = await Connection.QueryAsync<IndexColumns>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsNonUnique }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseTableIndex>();

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);

            foreach (var indexInfo in indexColumns)
            {
                var isUnique = !indexInfo.Key.IsNonUnique;
                var indexName = new Identifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnOrdinal)
                    .Select(row => tableColumns[row.ColumnName])
                    .Select(col => new MySqlDatabaseIndexColumn(col))
                    .ToList();

                var index = new MySqlDatabaseTableIndex(this, indexName, isUnique, indexCols);
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
            var result = new Dictionary<Identifier, IDatabaseKey>(Comparer);

            foreach (var uk in UniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> LoadUniqueKeyLookupAsync(CancellationToken cancellationToken)
        {
            var result = new Dictionary<Identifier, IDatabaseKey>(Comparer);

            var uniqueKeys = await UniqueKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyCollection<IDatabaseKey> LoadUniqueKeysSync()
        {
            const string sql = @"
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where
    t.table_schema = @SchemaName and t.table_name = @TableName
    and tc.constraint_type = 'UNIQUE'
order by kc.ordinal_position";

            var uniqueKeyColumns = Connection.Query<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var tableColumns = Column;
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]),
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new MySqlDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where
    t.table_schema = @SchemaName and t.table_name = @TableName
    and tc.constraint_type = 'UNIQUE'
order by kc.ordinal_position";

            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]),
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new MySqlDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChildKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            const string sql = @"
select
    t.table_schema as ChildTableSchema,
    t.table_name as ChildTableName,
    pt.table_schema as ParentTableSchema,
    pt.table_name as ParentTableName,
    rc.constraint_name as ChildKeyName,
    rc.unique_constraint_name as ParentKeyName,
    ptc.constraint_type as ParentKeyType,
    rc.delete_rule as DeleteRule,
    rc.update_rule as UpdateRule
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where pt.table_schema = @SchemaName and pt.table_name = @TableName";

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
                var childKeyName = new Identifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = new Identifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = Database.GetTable(childTableName);
                var parentKeyLookup = childTable.ParentKey;

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PRIMARY KEY")
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

                var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select
    t.table_schema as ChildTableSchema,
    t.table_name as ChildTableName,
    pt.table_schema as ParentTableSchema,
    pt.table_name as ParentTableName,
    rc.constraint_name as ChildKeyName,
    rc.unique_constraint_name as ParentKeyName,
    ptc.constraint_type as ParentKeyType,
    rc.delete_rule as DeleteRule,
    rc.update_rule as UpdateRule
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where pt.table_schema = @SchemaName and pt.table_name = @TableName";

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
                var childKeyName = new Identifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = new Identifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = await Database.GetTableAsync(childTableName, cancellationToken).ConfigureAwait(false);
                var parentKeyLookup = await childTable.ParentKeyAsync(cancellationToken).ConfigureAwait(false);

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PRIMARY KEY")
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
                var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);

                result.Add(relationalKey);
            }

            return result;
        }

        // checks are parsed but not supported by MySQL
        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>>(new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer));

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks => Array.Empty<IDatabaseCheckConstraint>();

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyChecks;

        private readonly static Task<IReadOnlyCollection<IDatabaseCheckConstraint>> _emptyChecks = Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(
            Array.Empty<IDatabaseCheckConstraint>()
        );

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => LoadParentKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => LoadParentKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeyLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(Comparer);

            foreach (var parentKey in ParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> LoadParentKeyLookupAsync(CancellationToken cancellationToken)
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(Comparer);

            var parentKeys = await ParentKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            const string sql = @"
select
    pt.table_schema as ParentTableSchema,
    pt.table_name as ParentTableName,
    rc.constraint_name as ChildKeyName,
    rc.unique_constraint_name as ParentKeyName,
    kc.column_name as ColumnName,
    kc.ordinal_position as ConstraintColumnId,
    ptc.constraint_type as ParentKeyType,
    rc.delete_rule as DeleteRule,
    rc.update_rule as UpdateRule
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where t.table_schema = @SchemaName and t.table_name = @TableName";

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
                row.UpdateRule
            }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = new Identifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);
                var parentKeyName = new Identifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "PRIMARY KEY")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = parentTable.UniqueKey;
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = new Identifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = Column;
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var childKey = new MySqlDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select
    pt.table_schema as ParentTableSchema,
    pt.table_name as ParentTableName,
    rc.constraint_name as ChildKeyName,
    rc.unique_constraint_name as ParentKeyName,
    kc.column_name as ColumnName,
    kc.ordinal_position as ConstraintColumnId,
    ptc.constraint_type as ParentKeyType,
    rc.delete_rule as DeleteRule,
    rc.update_rule as UpdateRule
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where t.table_schema = @SchemaName and t.table_name = @TableName";

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
            }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = new Identifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                var parentKeyName = new Identifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "PRIMARY KEY")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = await parentTable.UniqueKeyAsync(cancellationToken).ConfigureAwait(false);
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = new Identifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = await ColumnAsync(cancellationToken).ConfigureAwait(false);
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var childKey = new MySqlDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);
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
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(Comparer);

            var columns = Columns.Where(c => c.Name != null);
            foreach (var column in columns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(Comparer);

            var allColumns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var columns = allColumns.Where(c => c.Name != null);
            foreach (var column in columns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyList<IDatabaseTableColumn> LoadColumnsSync()
        {
            const string sql = @"
select
    column_name as ColumnName,
    data_type as DataTypeName,
    character_maximum_length as CharacterMaxLength,
    numeric_precision as `Precision`,
    numeric_scale as `Scale`,
    datetime_precision as `DateTimePrecision`,
    collation_name as Collation,
    is_nullable as IsNullable,
    column_default as DefaultValue,
    generation_expression as ComputedColumnDefinition,
    extra as ExtraInformation
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";

            var query = Connection.Query<ColumnData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            var result = new List<IDatabaseTableColumn>();

            foreach (var row in query)
            {
                var precision = row.DateTimePrecision > 0
                    ? new NumericPrecision(row.DateTimePrecision, 0)
                    : new NumericPrecision(row.Precision, row.Scale);

                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : new Identifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = precision
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = new Identifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;

                var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = isComputed
                    ? new MySqlDatabaseComputedTableColumn(this, columnName, columnType, isNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new MySqlDatabaseTableColumn(this, columnName, columnType, isNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseTableColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select
    column_name as ColumnName,
    data_type as DataTypeName,
    character_maximum_length as CharacterMaxLength,
    numeric_precision as `Precision`,
    numeric_scale as `Scale`,
    datetime_precision as `DateTimePrecision`,
    collation_name as Collation,
    is_nullable as IsNullable,
    column_default as DefaultValue,
    generation_expression as ComputedColumnDefinition,
    extra as ExtraInformation
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseTableColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : new Identifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = new Identifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;

                var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = isComputed
                    ? new MySqlDatabaseComputedTableColumn(this, columnName, columnType, isNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new MySqlDatabaseTableColumn(this, columnName, columnType, isNullable, row.DefaultValue, autoIncrement);

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
            var result = new Dictionary<Identifier, IDatabaseTrigger>(Comparer);

            foreach (var trigger in Triggers)
                result[trigger.Name.LocalName] = trigger;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> LoadTriggerLookupAsync(CancellationToken cancellationToken)
        {
            var result = new Dictionary<Identifier, IDatabaseTrigger>(Comparer);

            var triggers = await TriggersAsync(cancellationToken).ConfigureAwait(false);
            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyCollection<IDatabaseTrigger> LoadTriggersSync()
        {
            const string sql = @"
select tr.trigger_name as TriggerName, tr.action_statement as Definition, tr.action_timing as Timing, tr.event_manipulation as TriggerEvent
from information_schema.triggers tr
where tr.event_object_schema = @SchemaName and tr.event_object_table = @TableName";

            var queryResult = Connection.Query<TriggerData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing
            }).ToList();
            if (triggers.Count == 0)
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = new Identifier(trig.Key.TriggerName);
                var queryTiming = Enum.TryParse(trig.Key.Timing, true, out TriggerQueryTiming timing) ? timing : TriggerQueryTiming.Before;
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

                var trigger = new MySqlDatabaseTrigger(this, triggerName, definition, queryTiming, events);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select tr.trigger_name as TriggerName, tr.action_statement as Definition, tr.action_timing as Timing, tr.event_manipulation as TriggerEvent
from information_schema.triggers tr
where tr.event_object_schema = @SchemaName and tr.event_object_table = @TableName";

            var queryResult = await Connection.QueryAsync<TriggerData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing
            }).ToList();
            if (triggers.Count == 0)
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = new Identifier(trig.Key.TriggerName);
                var queryTiming = Enum.TryParse(trig.Key.Timing, true, out TriggerQueryTiming timing) ? timing : TriggerQueryTiming.Before;
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

                var trigger = new MySqlDatabaseTrigger(this, triggerName, definition, queryTiming, events);
                result.Add(trigger);
            }

            return result;
        }

        protected IReadOnlyDictionary<string, Rule> RelationalRuleMapping { get; } = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = Rule.None,
            ["RESTRICT"] = Rule.None,
            ["CASCADE"] = Rule.Cascade,
            ["SET NULL"] = Rule.SetNull,
            ["SET DEFAULT"] = Rule.SetDefault
        }.AsReadOnlyDictionary();
    }
}
