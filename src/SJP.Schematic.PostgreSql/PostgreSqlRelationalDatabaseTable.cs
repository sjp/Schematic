using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;
using System.Globalization;
using SJP.Schematic.Core.Extensions;
using System.Threading;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public PostgreSqlRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, IDbTypeProvider typeProvider, Identifier tableName, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
            Name = tableName ?? throw new ArgumentNullException(nameof(tableName));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        public Identifier Name { get; }

        protected IRelationalDatabase Database { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadPrimaryKeyAsync(cancellationToken);

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            var primaryKeyColumns = Connection.Query<ConstraintColumnMapping>(PrimaryKeyQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
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

            return new PostgreSqlDatabaseKey(constraintName, DatabaseKeyType.Primary, columns);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync(CancellationToken cancellationToken)
        {
            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(PrimaryKeyQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.OrderBy(row => row.OrdinalPosition).Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new PostgreSqlDatabaseKey(constraintName, DatabaseKeyType.Primary, columns);
        }

        protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

        private const string PrimaryKeyQuerySql = @"
select
    kc.constraint_name as ConstraintName,
    kc.column_name as ColumnName,
    kc.ordinal_position as OrdinalPosition
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @SchemaName and tc.table_name = @TableName
    and tc.constraint_type = 'PRIMARY KEY'";

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseIndex> LoadIndexLookupSync()
        {
            var indexes = Indexes;
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return new IdentifierResolvingDictionary<IDatabaseIndex>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken)
        {
            var indexes = await IndexesAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return new IdentifierResolvingDictionary<IDatabaseIndex>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync()
        {
            var queryResult = Connection.Query<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsPrimary }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var tableColumns = Column;
            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

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

                var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsPrimary }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

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

                var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols);
                result.Add(index);
            }

            return result;
        }

        protected virtual string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @"
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

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey => LoadUniqueKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys => LoadUniqueKeysSync();

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseKey> LoadUniqueKeyLookupSync()
        {
            var uniqueKeys = UniqueKeys;
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return new IdentifierResolvingDictionary<IDatabaseKey>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> LoadUniqueKeyLookupAsync(CancellationToken cancellationToken)
        {
            var uniqueKeys = await UniqueKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return new IdentifierResolvingDictionary<IDatabaseKey>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseKey> LoadUniqueKeysSync()
        {
            var uniqueKeyColumns = Connection.Query<ConstraintColumnMapping>(UniqueKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

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
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new PostgreSqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(CancellationToken cancellationToken)
        {
            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(UniqueKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.OrderBy(row => row.OrdinalPosition).Select(row => tableColumns[row.ColumnName]).ToList(),
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new PostgreSqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual string UniqueKeysQuery => UniqueKeysQuerySql;

        private const string UniqueKeysQuerySql = @"
select
    kc.constraint_name as ConstraintName,
    kc.column_name as ColumnName,
    kc.ordinal_position as OrdinalPosition
from information_schema.table_constraints tc
inner join information_schema.key_column_usage kc
    on tc.constraint_catalog = kc.constraint_catalog
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where tc.table_schema = @SchemaName and tc.table_name = @TableName
    and tc.constraint_type = 'UNIQUE'";

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChildKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            var queryResult = Connection.Query<ChildKeyData>(ChildKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
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

                var relationalKey = new DatabaseRelationalKey(childTableName, childKey, Name, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ChildKeyData>(ChildKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
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
                if (groupedChildKey.Key.ParentKeyType == "p")
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
                var relationalKey = new DatabaseRelationalKey(childTableName, childKey, Name, parentKey, deleteRule, updateRule);

                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual string ChildKeysQuery => ChildKeysQuerySql;

        private const string ChildKeysQuerySql = @"
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

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => LoadCheckLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadCheckLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks => LoadChecksSync();

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChecksAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckLookupSync()
        {
            var checks = Checks;
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count);

            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return new IdentifierResolvingDictionary<IDatabaseCheckConstraint>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckLookupAsync(CancellationToken cancellationToken)
        {
            var checks = await ChecksAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count);

            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return new IdentifierResolvingDictionary<IDatabaseCheckConstraint>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseCheckConstraint> LoadChecksSync()
        {
            var checks = Connection.Query<CheckConstraintData>(ChecksQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;

                var check = new PostgreSqlCheckConstraint(constraintName, definition);
                result.Add(check);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(CancellationToken cancellationToken)
        {
            var checks = await Connection.QueryAsync<CheckConstraintData>(ChecksQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;

                var check = new PostgreSqlCheckConstraint(constraintName, definition);
                result.Add(check);
            }

            return result;
        }

        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @"
select
    c.conname as ConstraintName,
    c.consrc as Definition
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => LoadParentKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => LoadParentKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeyLookupSync()
        {
            var parentKeys = ParentKeys;
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return new IdentifierResolvingDictionary<IDatabaseRelationalKey>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> LoadParentKeyLookupAsync(CancellationToken cancellationToken)
        {
            var parentKeys = await ParentKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return new IdentifierResolvingDictionary<IDatabaseRelationalKey>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            var queryResult = Connection.Query<ForeignKeyData>(ParentKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

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
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentSchemaName, fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

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

                var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = Column;
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var childKey = new PostgreSqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new DatabaseRelationalKey(Name, childKey, parentTableName, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ForeignKeyData>(ParentKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

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
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentSchemaName, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "p")
                {
                    parentKey = await parentTable.PrimaryKeyAsync(cancellationToken).ConfigureAwait(false);
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

                var childKey = new PostgreSqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new DatabaseRelationalKey(Name, childKey, parentTableName, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual string ParentKeysQuery => ParentKeysQuerySql;

        private const string ParentKeysQuerySql = @"
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

        public IReadOnlyList<IDatabaseColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        public IReadOnlyDictionary<Identifier, IDatabaseColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return new IdentifierResolvingDictionary<IDatabaseColumn>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return new IdentifierResolvingDictionary<IDatabaseColumn>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync()
        {
            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier("pg_catalog", row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    MaxLength = row.character_maximum_length > 0
                        ? row.character_maximum_length
                        : row.numeric_precision > 0 ? CreatePrecisionFromBase(row.numeric_precision, row.numeric_precision_radix) : 0,
                    NumericPrecision = row.numeric_precision_radix > 0
                        ? CreatePrecisionWithScaleFromBase(row.numeric_precision, row.numeric_scale, row.numeric_precision_radix)
                        : new NumericPrecision()
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.column_name);

                var isAutoIncrement = !row.serial_sequence_schema_name.IsNullOrWhiteSpace() && !row.serial_sequence_local_name.IsNullOrWhiteSpace();
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var isNullable = row.is_nullable == "YES";

                var column = new DatabaseColumn(columnName, columnType, isNullable, row.column_default, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier("pg_catalog", row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    MaxLength = row.character_maximum_length > 0
                        ? row.character_maximum_length
                        : row.numeric_precision > 0 ? CreatePrecisionFromBase(row.numeric_precision, row.numeric_precision_radix) : 0,
                    NumericPrecision = row.numeric_precision_radix > 0
                        ? CreatePrecisionWithScaleFromBase(row.numeric_precision, row.numeric_scale, row.numeric_precision_radix)
                        : new NumericPrecision()
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.column_name);

                var isAutoIncrement = !row.serial_sequence_schema_name.IsNullOrWhiteSpace() && !row.serial_sequence_local_name.IsNullOrWhiteSpace();
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var isNullable = row.is_nullable == "YES";

                var column = new DatabaseColumn(columnName, columnType, isNullable, row.column_default, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
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

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => LoadTriggerLookupSync();

        public IReadOnlyCollection<IDatabaseTrigger> Triggers => LoadTriggersSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggerLookupAsync(cancellationToken);

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggersAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTrigger> LoadTriggerLookupSync()
        {
            var triggers = Triggers;
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return new IdentifierResolvingDictionary<IDatabaseTrigger>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> LoadTriggerLookupAsync(CancellationToken cancellationToken)
        {
            var triggers = await TriggersAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return new IdentifierResolvingDictionary<IDatabaseTrigger>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseTrigger> LoadTriggersSync()
        {
            var queryResult = Connection.Query<TriggerData>(TriggersQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing,
                row.EnabledFlag
            }).ToList();
            if (triggers.Count == 0)
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
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
                var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<TriggerData>(TriggersQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing,
                row.EnabledFlag
            }).ToList();
            if (triggers.Count == 0)
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
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
                var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual string TriggersQuery => TriggersQuerySql;

        private const string TriggersQuerySql = @"
select
    tr.tgname as TriggerName,
    tgenabled as EnabledFlag,
    itr.action_statement as Definition,
    itr.action_timing as Timing,
    itr.event_manipulation as TriggerEvent
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on t.oid = tr.tgrelid
inner join information_schema.triggers itr on ns.nspname = itr.event_object_schema and itr.event_object_table = t.relname and itr.trigger_name = tr.tgname
where t.relkind = 'r'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";

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
        };
    }
}
