using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;
using System.Globalization;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public PostgreSqlRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName, IEqualityComparer<Identifier> comparer = null)
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

        public Task<IDatabaseKey> PrimaryKeyAsync() => LoadPrimaryKeyAsync();

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            const string sql = @"
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName, kc.ordinal_position as OrdinalPosition
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @SchemaName and tc.table_name = @TableName
    and tc.constraint_type = 'PRIMARY KEY'";

            var primaryKeyColumns = Connection.Query<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName } );
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var tableColumns = Column;
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.OrderBy(row => row.OrdinalPosition).Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new PostgreSqlDatabaseKey(this, constraintName, DatabaseKeyType.Primary, columns);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync()
        {
            const string sql = @"
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName, kc.ordinal_position as OrdinalPosition
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @SchemaName and tc.table_name = @TableName
    and tc.constraint_type = 'PRIMARY KEY'";

            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var tableColumns = Column;
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.OrderBy(row => row.OrdinalPosition).Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new PostgreSqlDatabaseKey(this, constraintName, DatabaseKeyType.Primary, columns);
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
select
    i.relname as IndexName,
    idx.indisunique as IsUnique,
    idx.indisprimary as IsPrimary,
    pg_catalog.generate_subscripts(idx.indkey, 1) as IndexColumnId,
    pg_catalog.unnest(array(
        select pg_catalog.pg_get_indexdef(idx.indexrelid, k + 1, true)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as IndexColumnExpression,
    pg_catalog.unnest(array(
        select pg_catalog.pg_index_column_has_property(idx.indexrelid, k + 1, 'desc')
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as IsDescending,
    (idx.indexprs is not null) or (idx.indkey::int[] @> array[0]) as IsFunctional
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class t on idx.indrelid = t.oid
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
where
    t.relkind = 'r'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

            var queryResult = Connection.Query<IndexColumns>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsPrimary }).ToList();
            if (indexColumns.Count == 0)
                return Enumerable.Empty<IDatabaseTableIndex>();

            var tableColumns = Column;
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new Identifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.IndexColumnId)
                    .Select(row => new
                    {
                        row.IsDescending,
                        Expression = row.IndexColumnExpression,
                        Column = tableColumns.ContainsKey(row.IndexColumnExpression) ? tableColumns[row.IndexColumnExpression] : null
                    })
                    .Select(row => row.Column != null
                        ? new PostgreSqlDatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending)
                        : new PostgreSqlDatabaseIndexColumn(row.Expression, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var index = new PostgreSqlDatabaseTableIndex(this, indexName, isUnique, indexCols);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseTableIndex>> LoadIndexesAsync()
        {
            const string sql = @"
select
    i.relname as IndexName,
    idx.indisunique as IsUnique,
    idx.indisprimary as IsPrimary,
    pg_catalog.generate_subscripts(idx.indkey, 1) as IndexColumnId,
    pg_catalog.unnest(array(
        select pg_catalog.pg_get_indexdef(idx.indexrelid, k + 1, true)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as IndexColumnExpression,
    pg_catalog.unnest(array(
        select pg_catalog.pg_index_column_has_property(idx.indexrelid, k + 1, 'desc')
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as IsDescending,
    (idx.indexprs is not null) or (idx.indkey::int[] @> array[0]) as IsFunctional
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class t on idx.indrelid = t.oid
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
where
    t.relkind = 'r'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

            var queryResult = await Connection.QueryAsync<IndexColumns>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsPrimary }).ToList();
            if (indexColumns.Count == 0)
                return Enumerable.Empty<IDatabaseTableIndex>();

            var tableColumns = await ColumnAsync().ConfigureAwait(false);
            var result = new List<IDatabaseTableIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new Identifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.IndexColumnId)
                    .Select(row => new
                    {
                        row.IsDescending,
                        Expression = row.IndexColumnExpression,
                        Column = tableColumns.ContainsKey(row.IndexColumnExpression) ? tableColumns[row.IndexColumnExpression] : null
                    })
                    .Select(row => row.Column != null
                        ? new PostgreSqlDatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending)
                        : new PostgreSqlDatabaseIndexColumn(row.Expression, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var index = new PostgreSqlDatabaseTableIndex(this, indexName, isUnique, indexCols);
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
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName, kc.ordinal_position as OrdinalPosition
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @SchemaName and tc.table_name = @TableName
    and tc.constraint_type = 'UNIQUE'";

            var uniqueKeyColumns = Connection.Query<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (uniqueKeyColumns.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var tableColumns = Column;
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.OrderBy(row => row.OrdinalPosition).Select(row => tableColumns[row.ColumnName]).ToList(),
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new PostgreSqlDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseKey>> LoadUniqueKeysAsync()
        {
            const string sql = @"
select kc.constraint_name as ConstraintName, kc.column_name as ColumnName, kc.ordinal_position as OrdinalPosition
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @SchemaName and tc.table_name = @TableName
    and tc.constraint_type = 'UNIQUE'";

            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (uniqueKeyColumns.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var tableColumns = await ColumnAsync().ConfigureAwait(false);
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.OrderBy(row => row.OrdinalPosition).Select(row => tableColumns[row.ColumnName]).ToList(),
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new PostgreSqlDatabaseKey(this, uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => LoadChildKeysAsync();

        protected virtual IEnumerable<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            const string sql = @"
select
    ns.nspname as ChildTableSchema,
    t.relname as ChildTableName,
    c.conname as ChildKeyName,
    pkc.contype as ParentKeyType,
    pkc.conname as ParentKeyName,
    c.confupdtype as UpdateRule,
    c.confdeltype as DeleteRule
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
where pt.relname = @TableName and pns.nspname = @SchemaName";

            var queryResult = Connection.Query<ChildKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

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
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = new Identifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = new Identifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = Database.GetTable(childTableName);
                var parentKeyLookup = childTable.ParentKey;

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "p")
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

                var relationalKey = new PostgreSqlRelationalKey(childKey, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseRelationalKey>> LoadChildKeysAsync()
        {
            const string sql = @"
select
    ns.nspname as ChildTableSchema,
    t.relname as ChildTableName,
    c.conname as ChildKeyName,
    pkc.contype as ParentKeyType,
    pkc.conname as ParentKeyName,
    c.confupdtype as UpdateRule,
    c.confdeltype as DeleteRule
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
where pt.relname = @TableName and pns.nspname = @SchemaName";

            var queryResult = await Connection.QueryAsync<ChildKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

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
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = new Identifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = new Identifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childTable = await Database.GetTableAsync(childTableName).ConfigureAwait(false);
                var parentKeyLookup = await childTable.ParentKeyAsync().ConfigureAwait(false);

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "p")
                {
                    parentKey = await PrimaryKeyAsync().ConfigureAwait(false);
                }
                else
                {
                    var uniqueKeyLookup = await UniqueKeyAsync().ConfigureAwait(false);
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
                }

                var deleteRule = RelationalRuleMapping[groupedChildKey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[groupedChildKey.Key.UpdateRule];
                var relationalKey = new PostgreSqlRelationalKey(childKey, parentKey, deleteRule, updateRule);

                result.Add(relationalKey);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => LoadCheckLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync() => LoadCheckLookupAsync();

        public IEnumerable<IDatabaseCheckConstraint> Checks => LoadChecksSync();

        public Task<IEnumerable<IDatabaseCheckConstraint>> ChecksAsync() => LoadChecksAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            foreach (var check in Checks)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            var checks = await ChecksAsync().ConfigureAwait(false);
            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseCheckConstraint> LoadChecksSync()
        {
            const string sql = @"
select c.conname as ConstraintName, c.consrc as Definition
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

            var checks = Connection.Query<CheckConstraintData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (checks.Empty())
                return Enumerable.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();
            var tableColumns = Column;
            foreach (var checkRow in checks)
            {
                var constraintName = new Identifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;

                var check = new PostgreSqlCheckConstraint(this, constraintName, definition);
                result.Add(check);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseCheckConstraint>> LoadChecksAsync()
        {
            const string sql = @"
select c.conname as ConstraintName, c.consrc as Definition
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

            var checks = await Connection.QueryAsync<CheckConstraintData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (checks.Empty())
                return Enumerable.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();
            var tableColumns = await ColumnAsync().ConfigureAwait(false);
            foreach (var checkRow in checks)
            {
                var constraintName = new Identifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;

                var check = new PostgreSqlCheckConstraint(this, constraintName, definition);
                result.Add(check);
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
select
    c.conname as ChildKeyName,
    tc.attname as ColumnName,
    child_cols.con_index as ConstraintColumnId,
    pns.nspname as ParentSchemaName,
    pt.relname as ParentTableName,
    pkc.contype as ParentKeyType,
    pkc.conname as ParentKeyName,
    c.confupdtype as UpdateRule,
    c.confdeltype as DeleteRule
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
where t.relname = @TableName and ns.nspname = @SchemaName";

            var queryResult = Connection.Query<ForeignKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                row.ChildKeyName,
                row.ParentSchemaName,
                row.ParentTableName,
                row.ParentKeyName,
                KeyType = row.ParentKeyType,
                row.DeleteRule,
                row.UpdateRule
            }).ToList();
            if (foreignKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = new Identifier(fkey.Key.ParentSchemaName, fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);
                var parentKeyName = new Identifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "p")
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

                var childKey = new PostgreSqlDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new PostgreSqlRelationalKey(childKey, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseRelationalKey>> LoadParentKeysAsync()
        {
            const string sql = @"
select
    c.conname as ChildKeyName,
    tc.attname as ColumnName,
    child_cols.con_index as ConstraintColumnId,
    pns.nspname as ParentSchemaName,
    pt.relname as ParentTableName,
    pkc.contype as ParentKeyType,
    pkc.conname as ParentKeyName,
    c.confupdtype as UpdateRule,
    c.confdeltype as DeleteRule
from pg_namespace ns
inner join pg_class t on ns.oid = t.relnamespace
inner join pg_constraint c on c.conrelid = t.oid and c.contype = 'f'
inner join pg_attribute tc on tc.attrelid = t.oid and tc.attnum = any(c.conkey)
inner join pg_catalog.unnest(c.conkey) with ordinality as child_cols(col_index, con_index) on child_cols.col_index = tc.attnum
inner join pg_class pt on pt.oid = c.confrelid
inner join pg_namespace pns on pns.oid = pt.relnamespace
left join pg_depend d1  -- find constraint's dependency on an index
    on d1.objid = c.oid
    and d1.classid = 'pg_constraint'::regclass
    and d1.refclassid = 'pg_class'::regclass
    and d1.refobjsubid = 0
left join pg_depend d2  -- find pkey/unique constraint for that index
    on d2.refclassid = 'pg_constraint'::regclass
    and d2.classid = 'pg_class'::regclass
    and d2.objid = d1.refobjid
    and d2.objsubid = 0
    and d2.deptype = 'i'
left join pg_constraint pkc on pkc.oid = d2.refobjid
    and pkc.contype in ('p', 'u')
    and pkc.conrelid = c.confrelid
where t.relname = @TableName and ns.nspname = @SchemaName";

            var queryResult = await Connection.QueryAsync<ForeignKeyData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                row.ChildKeyName,
                row.ParentSchemaName,
                row.ParentTableName,
                row.ParentKeyName,
                KeyType = row.ParentKeyType,
                row.DeleteRule,
                row.UpdateRule
            }).ToList();
            if (foreignKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = new Identifier(fkey.Key.ParentSchemaName, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName).ConfigureAwait(false);
                var parentKeyName = new Identifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "p")
                {
                    parentKey = await parentTable.PrimaryKeyAsync().ConfigureAwait(false);
                }
                else
                {
                    var uniqueKeys = await parentTable.UniqueKeyAsync().ConfigureAwait(false);
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = new Identifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = await ColumnAsync().ConfigureAwait(false);
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var childKey = new PostgreSqlDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new PostgreSqlRelationalKey(childKey, parentKey, deleteRule, updateRule);
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
    column_name,
    ordinal_position,
    column_default,
    is_nullable,
    data_type,
    character_maximum_length,
    character_octet_length,
    numeric_precision,
    numeric_precision_radix,
    numeric_scale,
    datetime_precision,
    interval_type,
    collation_catalog,
    collation_schema,
    collation_name,
    domain_catalog,
    domain_schema,
    domain_name,
    udt_catalog,
    udt_schema,
    udt_name,
    dtd_identifier,
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(table_name, column_name)))[1] as serial_sequence_schema_name,
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(table_name, column_name)))[2] as serial_sequence_local_name
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";

            var query = Connection.Query<ColumnData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            var result = new List<IDatabaseTableColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier("pg_catalog", row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : new Identifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    MaxLength = row.character_maximum_length > 0 ? row.character_maximum_length
                        : row.numeric_precision > 0 ? CreatePrecisionFromBase(row.numeric_precision, row.numeric_precision_radix) : 0,
                    NumericPrecision = row.numeric_precision_radix > 0
                        ? CreatePrecisionWithScaleFromBase(row.numeric_precision, row.numeric_scale, row.numeric_precision_radix)
                        : new NumericPrecision()
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = new Identifier(row.column_name);

                var isAutoIncrement = !row.serial_sequence_schema_name.IsNullOrWhiteSpace() && !row.serial_sequence_local_name.IsNullOrWhiteSpace();
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var isNullable = row.is_nullable == "YES";

                var column = new PostgreSqlDatabaseTableColumn(this, columnName, columnType, isNullable, row.column_default, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseTableColumn>> LoadColumnsAsync()
        {
            const string sql = @"
select
    column_name,
    ordinal_position,
    column_default,
    is_nullable,
    data_type,
    character_maximum_length,
    character_octet_length,
    numeric_precision,
    numeric_precision_radix,
    numeric_scale,
    datetime_precision,
    interval_type,
    collation_catalog,
    collation_schema,
    collation_name,
    domain_catalog,
    domain_schema,
    domain_name,
    udt_catalog,
    udt_schema,
    udt_name,
    dtd_identifier,
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(table_name, column_name)))[1] as serial_sequence_schema_name,
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(table_name, column_name)))[2] as serial_sequence_local_name
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseTableColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier("pg_catalog", row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : new Identifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    MaxLength = row.character_maximum_length > 0 ? row.character_maximum_length
                        : row.numeric_precision > 0 ? CreatePrecisionFromBase(row.numeric_precision, row.numeric_precision_radix) : 0,
                    NumericPrecision = row.numeric_precision_radix > 0
                        ? CreatePrecisionWithScaleFromBase(row.numeric_precision, row.numeric_scale, row.numeric_precision_radix)
                        : new NumericPrecision()
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = new Identifier(row.column_name);

                var isAutoIncrement = !row.serial_sequence_schema_name.IsNullOrWhiteSpace() && !row.serial_sequence_local_name.IsNullOrWhiteSpace();
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var isNullable = row.is_nullable == "YES";

                var column = new PostgreSqlDatabaseTableColumn(this, columnName, columnType, isNullable, row.column_default, autoIncrement);
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
select tr.tgname as TriggerName, tgenabled as EnabledFlag, itr.action_statement as Definition, itr.action_timing as Timing, itr.event_manipulation as TriggerEvent
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on t.oid = tr.tgrelid
inner join information_schema.triggers itr on ns.nspname = itr.event_object_schema and itr.event_object_table = t.relname and itr.trigger_name = tr.tgname
where t.relkind = 'r'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

            var queryResult = Connection.Query<TriggerData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing,
                row.EnabledFlag
            }).ToList();
            if (triggers.Count == 0)
                return Enumerable.Empty<IDatabaseTrigger>();

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

                var isEnabled = trig.Key.EnabledFlag != "D";
                var trigger = new PostgreSqlDatabaseTrigger(this, triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseTrigger>> LoadTriggersAsync()
        {
            const string sql = @"
select tr.tgname as TriggerName, tgenabled as EnabledFlag, itr.action_statement as Definition, itr.action_timing as Timing, itr.event_manipulation as TriggerEvent
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on t.oid = tr.tgrelid
inner join information_schema.triggers itr on ns.nspname = itr.event_object_schema and itr.event_object_table = t.relname and itr.trigger_name = tr.tgname
where t.relkind = 'r'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

            var queryResult = await Connection.QueryAsync<TriggerData>(sql, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing,
                row.EnabledFlag
            }).ToList();
            if (triggers.Count == 0)
                return Enumerable.Empty<IDatabaseTrigger>();

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

                var isEnabled = trig.Key.EnabledFlag != "D";
                var trigger = new PostgreSqlDatabaseTrigger(this, triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected static int CreatePrecisionFromBase(int precision, int radix)
        {
            if (precision < 0)
                throw new ArgumentOutOfRangeException(nameof(precision));
            if (radix < 0)
                throw new ArgumentOutOfRangeException(nameof(radix));

            var newPrecision = Convert.ToInt64(Math.Pow(precision, radix));
            var newPrecisionStr = newPrecision.ToString(CultureInfo.InvariantCulture);

            return newPrecisionStr.Length;
        }

        protected static NumericPrecision CreatePrecisionWithScaleFromBase(int precision, int scale, int radix)
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

        protected IReadOnlyDictionary<string, Rule> RelationalRuleMapping { get; } = new Dictionary<string, Rule>
        {
            ["a"] = Rule.None,
            ["c"] = Rule.Cascade,
            ["n"] = Rule.SetNull,
            ["d"] = Rule.SetDefault,
            ["r"] = Rule.None // could be changed to restrict
        }.AsReadOnlyDictionary();
    }
}
