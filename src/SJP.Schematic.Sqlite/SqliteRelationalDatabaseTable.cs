using Dapper;
using Superpower;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Query;
using SJP.Schematic.Sqlite.Parsing;
using System.Threading;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabaseTable : IRelationalDatabaseTable, IRelationalDatabaseTableAsync
    {
        public SqliteRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = tableName.LocalName;
            Comparer = new IdentifierComparer(StringComparer.OrdinalIgnoreCase, Database.DefaultSchema);
        }

        public IRelationalDatabase Database { get; }

        protected IDbConnection Connection { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public Identifier Name { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync() => LoadPrimaryKeyAsync();

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            var sql = $"pragma table_info({ Database.Dialect.QuoteName(Name.LocalName) })";
            var tableInfos = Connection.Query<TableInfo>(sql);
            if (tableInfos.Empty())
                return null;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Count == 0)
                return null;

            var tableColumn = Column;
            var columns = pkColumns.Select(c => tableColumn[c.name]).ToList();

            var parser = ParsedDefinition;
            var pkConstraint = parser.Columns
                .SelectMany(col => col.Constraints)
                .Concat(parser.Constraints)
                .FirstOrDefault(c => c.Type == SqliteTableParser.ConstraintType.Primary);

            var pkStringName = pkConstraint?.Name;
            var primaryKeyName = !pkStringName.IsNullOrWhiteSpace() ? new LocalIdentifier(pkStringName) : null;
            return new SqliteDatabaseKey(this, primaryKeyName, DatabaseKeyType.Primary, columns);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync()
        {
            var sql = $"pragma table_info({ Database.Dialect.QuoteName(Name.LocalName) })";
            var tableInfos = await Connection.QueryAsync<TableInfo>(sql);
            if (tableInfos.Empty())
                return null;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Count == 0)
                return null;

            var tableColumn = await ColumnAsync();
            var columns = pkColumns.Select(c => tableColumn[c.name]).ToList();

            var parser = await ParsedDefinitionAsync();
            var pkConstraint = parser.Columns
                .SelectMany(col => col.Constraints)
                .Concat(parser.Constraints)
                .FirstOrDefault(c => c.Type == SqliteTableParser.ConstraintType.Primary);

            var pkStringName = pkConstraint?.Name;
            var primaryKeyName = !pkStringName.IsNullOrWhiteSpace() ? new LocalIdentifier(pkStringName) : null;
            return new SqliteDatabaseKey(this, primaryKeyName, DatabaseKeyType.Primary, columns);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync() => LoadIndexLookupAsync();

        public IEnumerable<IDatabaseTableIndex> Indexes => LoadIndexesSync();

        public Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync() => LoadIndexesAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTableIndex> LoadIndexLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(Comparer);

            var namedIndexes = Indexes.Where(i => i.Name != null);
            foreach (var index in namedIndexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> LoadIndexLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(Comparer);

            var indexes = await IndexesAsync();
            var namedIndexes = indexes.Where(i => i.Name != null);

            foreach (var index in namedIndexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseTableIndex> LoadIndexesSync()
        {
            var listSql = $"pragma index_list({ Database.Dialect.QuoteName(Name.LocalName) })";
            var indexLists = Connection.Query<IndexList>(listSql);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c");
            if (nonConstraintIndexLists.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var result = new List<IDatabaseTableIndex>();

            foreach (var indexList in nonConstraintIndexLists)
            {
                var infoSql = $"pragma index_xinfo({ Database.Dialect.QuoteName(indexList.name) })";
                var indexInfo = Connection.Query<IndexXInfo>(infoSql);
                var indexColumns = indexInfo
                    .Where(i => i.key)
                    .OrderBy(i => i.cid)
                    .Select(i => new SqliteDatabaseIndexColumn(Column[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key)
                    .OrderBy(i => i.name)
                    .Select(i => Column[i.name])
                    .ToList();

                var index = new SqliteDatabaseTableIndex(this, indexList.name, indexList.unique, indexColumns, includedColumns);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseTableIndex>> LoadIndexesAsync()
        {
            var listSql = $"pragma index_list({ Database.Dialect.QuoteName(Name.LocalName) })";
            var indexLists = await Connection.QueryAsync<IndexList>(listSql);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c");
            if (nonConstraintIndexLists.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var result = new List<IDatabaseTableIndex>();

            foreach (var indexList in nonConstraintIndexLists)
            {
                var infoSql = $"pragma index_xinfo({ Database.Dialect.QuoteName(indexList.name) })";
                var indexInfo = await Connection.QueryAsync<IndexXInfo>(infoSql);
                var indexColumns = indexInfo
                    .Where(i => i.key)
                    .OrderBy(i => i.cid)
                    .Select(i => new SqliteDatabaseIndexColumn(Column[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key)
                    .OrderBy(i => i.name)
                    .Select(i => Column[i.name])
                    .ToList();

                var index = new SqliteDatabaseTableIndex(this, indexList.name, indexList.unique, indexColumns, includedColumns);
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

            var namedUniqueKeys = UniqueKeys.Where(uk => uk.Name != null);
            foreach (var uk in namedUniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> LoadUniqueKeyLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseKey>(Comparer);

            var uniqueKeys = await UniqueKeysAsync();
            var namedUniqueKeys = uniqueKeys.Where(uk => uk.Name != null);

            foreach (var uk in namedUniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseKey> LoadUniqueKeysSync()
        {
            var sql = $"pragma index_list({ Database.Dialect.QuoteName(Name.LocalName) })";
            var indexLists = Connection.Query<IndexList>(sql);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>();

            var parser = ParsedDefinition;
            var parsedUniqueConstraints = parser.Columns
                .SelectMany(col => col.Constraints)
                .Concat(parser.Constraints)
                .Where(c => c.Type == SqliteTableParser.ConstraintType.Unique);

            var tableColumn = Column;
            foreach (var ukIndexList in ukIndexLists)
            {
                var indexSql = $"pragma index_xinfo({ Database.Dialect.QuoteName(ukIndexList.name) })";
                var indexXInfos = Connection.Query<IndexXInfo>(indexSql);
                var orderedColumns = indexXInfos
                    .Where(i => i.cid > 0 && i.key)
                    .OrderBy(i => i.seqno);
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var columns = orderedColumns.Select(i => tableColumn[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints.FirstOrDefault(constraint => constraint.Columns.SequenceEqual(columnNames));
                var stringConstraintName = uniqueConstraint?.Name;

                var keyName = !stringConstraintName.IsNullOrWhiteSpace() ? new LocalIdentifier(stringConstraintName) : null;
                var uniqueKey = new SqliteDatabaseKey(this, keyName, DatabaseKeyType.Unique, columns);
                result.Add(uniqueKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseKey>> LoadUniqueKeysAsync()
        {
            var sql = $"pragma index_list({ Database.Dialect.QuoteName(Name.LocalName) })";
            var indexLists = await Connection.QueryAsync<IndexList>(sql);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>();

            var parser = await ParsedDefinitionAsync();
            var parsedUniqueConstraints = parser.Columns
                .SelectMany(col => col.Constraints)
                .Concat(parser.Constraints)
                .Where(c => c.Type == SqliteTableParser.ConstraintType.Unique);

            var tableColumn = await ColumnAsync();
            foreach (var ukIndexList in ukIndexLists)
            {
                var indexSql = $"pragma index_xinfo({ Database.Dialect.QuoteName(ukIndexList.name) })";
                var indexXInfos = await Connection.QueryAsync<IndexXInfo>(indexSql);
                var orderedColumns = indexXInfos
                    .Where(i => i.cid > 0 && i.key)
                    .OrderBy(i => i.seqno);
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var columns = orderedColumns.Select(i => tableColumn[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints.FirstOrDefault(constraint => constraint.Columns.SequenceEqual(columnNames));
                var stringConstraintName = uniqueConstraint?.Name;

                var keyName = !stringConstraintName.IsNullOrWhiteSpace() ? new LocalIdentifier(stringConstraintName) : null;
                var uniqueKey = new SqliteDatabaseKey(this, keyName, DatabaseKeyType.Unique, columns);
                result.Add(uniqueKey);
            }

            return result;
        }

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => LoadChildKeysAsync();

        protected virtual IEnumerable<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            return Database.Tables
                .SelectMany(t => t.ParentKeys)
                .Where(fk =>
                {
                    var parentTableName = fk.ParentKey.Table.Name.LocalName;
                    return string.Equals(parentTableName, Name.LocalName, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
        }

        protected virtual Task<IEnumerable<IDatabaseRelationalKey>> LoadChildKeysAsync()
        {
            var dbTables = Database.TablesAsync().ToEnumerable();

            IEnumerable<IDatabaseRelationalKey> childKeys = dbTables
                .SelectMany(t => t.ParentKeys)
                .Where(fk =>
                {
                    var parentTableName = fk.ParentKey.Table.Name.LocalName;
                    return string.Equals(parentTableName, Name.LocalName, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            return Task.FromResult(childKeys);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CheckConstraint => LoadCheckConstraintLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckConstraintAsync() => LoadCheckConstraintLookupAsync();

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints => LoadCheckConstraintsSync();

        public Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync() => LoadCheckConstraintsAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckConstraintLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            var namedChecks = CheckConstraints.Where(c => c.Name != null);
            foreach (var check in namedChecks)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckConstraintLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            var checks = await CheckConstraintsAsync();
            var namedChecks = checks.Where(c => c.Name != null);

            foreach (var check in namedChecks)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseCheckConstraint> LoadCheckConstraintsSync()
        {
            var result = new List<IDatabaseCheckConstraint>();

            var parser = ParsedDefinition;
            var checkConstraints = parser
                .Columns.SelectMany(col => col.Constraints)
                .Concat(parser.Constraints);

            foreach (var ck in checkConstraints)
            {
                var definition = ck.Tokens.Select(token => token.ToStringValue()).Join(" ");
                var check = new SqliteCheckConstraint(this, ck.Name, definition);
                result.Add(check);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseCheckConstraint>> LoadCheckConstraintsAsync()
        {
            var result = new List<IDatabaseCheckConstraint>();

            var parser = await ParsedDefinitionAsync();
            var checkConstraints = parser
                .Columns.SelectMany(col => col.Constraints)
                .Concat(parser.Constraints);

            foreach (var ck in checkConstraints)
            {
                var definition = ck.Tokens.Select(token => token.ToStringValue()).Join(" ");
                var check = new SqliteCheckConstraint(this, ck.Name, definition);
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

            var namedParentKeys = ParentKeys.Where(fk => fk.ChildKey.Name != null);
            foreach (var parentKey in namedParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> LoadParentKeyLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(Comparer);

            var parentKeys = await ParentKeysAsync();
            var namedParentKeys = parentKeys.Where(fk => fk.ChildKey.Name != null);

            foreach (var parentKey in namedParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            const string sql = "pragma foreign_key_list(@TableName)";
            var queryResult = Connection.Query<ForeignKeyList>(sql, new { TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update });
            var parser = ParsedDefinition;
            var fkConstraints = parser.Columns
                .SelectMany(col => col.Constraints)
                .Concat(parser.Constraints)
                .Where(c => c.Type == SqliteTableParser.ConstraintType.Foreign)
                .ToList();

            var result = new List<IDatabaseRelationalKey>();
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.seq);

                var parentTableName = new Identifier(fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);

                var parentColumns = parentTable.Columns;
                parentColumns = rows.Select(row => parentTable.Column[row.to]).ToList();

                var parentPrimaryKey = parentTable.PrimaryKey;
                var pkColumnsEqual = parentPrimaryKey.Columns.Select(col => col.Name)
                    .SequenceEqual(parentColumns.Select(col => col.Name));

                IDatabaseKey parentConstraint;
                DatabaseKeyType parentKeyType;
                if (pkColumnsEqual)
                {
                    parentKeyType = DatabaseKeyType.Primary;
                    parentConstraint = parentPrimaryKey;
                }
                else
                {
                    parentKeyType = DatabaseKeyType.Unique;
                    var uniqueKeys = parentTable.UniqueKeys;
                    parentConstraint = uniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                }

                var parentKey = new SqliteDatabaseKey(this, parentConstraint.Name, parentKeyType, parentColumns);

                var parsedConstraint = fkConstraints
                    .Where(fkc => string.Equals(fkc.ForeignKeyTableName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ForeignKeyColumns.SequenceEqual(rows.Select(row => row.from), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? new LocalIdentifier(constraintStringName) : null;
                var childKeyColumnLookup = Column;
                var childKeyColumns = rows.Select(row => childKeyColumnLookup[row.from]);

                var childKey = new SqliteDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteAction = GetRelationalUpdateAction(fkey.Key.OnDelete);
                var updateAction = GetRelationalUpdateAction(fkey.Key.OnUpdate);

                var relationalKey = new SqliteRelationalKey(childKey, parentConstraint, deleteAction, updateAction);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseRelationalKey>> LoadParentKeysAsync()
        {
            const string sql = "pragma foreign_key_list(@TableName)";
            var queryResult = await Connection.QueryAsync<ForeignKeyList>(sql, new { TableName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update });
            var parser = await ParsedDefinitionAsync();
            var fkConstraints = parser.Columns
                .SelectMany(col => col.Constraints)
                .Concat(parser.Constraints)
                .Where(c => c.Type == SqliteTableParser.ConstraintType.Foreign)
                .ToList();

            var result = new List<IDatabaseRelationalKey>();
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.seq);

                var parentTableName = new Identifier(fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName);

                var parentColumns = await parentTable.ColumnsAsync();
                parentColumns = rows.Select(row => parentTable.Column[row.to]).ToList();

                var parentPrimaryKey = await parentTable.PrimaryKeyAsync();
                var pkColumnsEqual = parentPrimaryKey.Columns.Select(col => col.Name)
                    .SequenceEqual(parentColumns.Select(col => col.Name));

                IDatabaseKey parentConstraint;
                DatabaseKeyType parentKeyType;
                if (pkColumnsEqual)
                {
                    parentKeyType = DatabaseKeyType.Primary;
                    parentConstraint = parentPrimaryKey;
                }
                else
                {
                    parentKeyType = DatabaseKeyType.Unique;
                    var uniqueKeys = await parentTable.UniqueKeysAsync();
                    parentConstraint = uniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                }

                var parentKey = new SqliteDatabaseKey(this, parentConstraint.Name, parentKeyType, parentColumns);

                var parsedConstraint = fkConstraints
                    .Where(fkc => string.Equals(fkc.ForeignKeyTableName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ForeignKeyColumns.SequenceEqual(rows.Select(row => row.from), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? new LocalIdentifier(constraintStringName) : null;
                var childKeyColumnLookup = await ColumnAsync();
                var childKeyColumns = rows.Select(row => childKeyColumnLookup[row.from]);

                var childKey = new SqliteDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteAction = GetRelationalUpdateAction(fkey.Key.OnDelete);
                var updateAction = GetRelationalUpdateAction(fkey.Key.OnUpdate);

                var relationalKey = new SqliteRelationalKey(childKey, parentConstraint, deleteAction, updateAction);
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

            var namedColumns = Columns.Where(c => c.Name != null);
            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(Comparer);

            var columns = await ColumnsAsync();
            var namedColumns = columns.Where(c => c.Name != null);

            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyList<IDatabaseTableColumn> LoadColumnsSync()
        {
            var sql = $"pragma table_info({ Database.Dialect.QuoteName(Name.LocalName) })";
            var tableInfos = Connection.Query<TableInfo>(sql);

            var result = new List<IDatabaseTableColumn>();
            if (tableInfos.Empty())
                return result;

            var parser = ParsedDefinition;
            var parsedColumns = parser.Columns;

            foreach (var tableInfo in tableInfos)
            {
                var parsedColumnInfo = parsedColumns.FirstOrDefault(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                IDbType dbType;
                var columnType = new SqliteColumnDataType(columnTypeName);
                if (columnType.IsNumericType)
                    dbType = new SqliteNumericColumnDataType(columnTypeName);
                else if (columnType.IsStringType)
                    dbType = new SqliteStringColumnDataType(columnTypeName, parsedColumnInfo.Collation.ToString());
                else
                    dbType = columnType;

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;

                var column = new SqliteDatabaseTableColumn(this, tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, isAutoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseTableColumn>> LoadColumnsAsync()
        {
            var sql = $"pragma table_info({ Database.Dialect.QuoteName(Name.LocalName) })";
            var tableInfos = await Connection.QueryAsync<TableInfo>(sql);

            var result = new List<IDatabaseTableColumn>();
            if (tableInfos.Empty())
                return result;

            var parser = await ParsedDefinitionAsync();
            var parsedColumns = parser.Columns;

            foreach (var tableInfo in tableInfos)
            {
                var parsedColumnInfo = parsedColumns.FirstOrDefault(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                IDbType dbType;
                var columnType = new SqliteColumnDataType(columnTypeName);
                if (columnType.IsNumericType)
                    dbType = new SqliteNumericColumnDataType(columnTypeName);
                else if (columnType.IsStringType)
                    dbType = new SqliteStringColumnDataType(columnTypeName, parsedColumnInfo.Collation.ToString());
                else
                    dbType = columnType;

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;

                var column = new SqliteDatabaseTableColumn(this, tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, isAutoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => LoadTriggerLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync() => LoadTriggerLookupAsync();

        public IEnumerable<IDatabaseTrigger> Triggers => LoadTriggersSync();

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

            var triggers = await TriggersAsync();
            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseTrigger> LoadTriggersSync()
        {
            const string sql = "select * from sqlite_master where type = 'trigger' and tbl_name = @TableName";
            var triggerInfos = Connection.Query<SqliteMaster>(sql, new { TableName = Name.LocalName });

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var tokenizer = new SqliteTokenizer();
                var tokens = tokenizer.Tokenize(triggerInfo.sql);
                var parser = new SqliteTriggerParser(tokens);

                var trigger = new SqliteDatabaseTrigger(this, triggerInfo.name, triggerInfo.sql, parser.Timing, parser.Event);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseTrigger>> LoadTriggersAsync()
        {
            const string sql = "select * from sqlite_master where type = 'trigger' and tbl_name = @TableName";
            var triggerInfos = await Connection.QueryAsync<SqliteMaster>(sql, new { TableName = Name.LocalName });

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var tokenizer = new SqliteTokenizer();
                var tokens = tokenizer.Tokenize(triggerInfo.sql);
                var parser = new SqliteTriggerParser(tokens);

                var trigger = new SqliteDatabaseTrigger(this, triggerInfo.name, triggerInfo.sql, parser.Timing, parser.Event);
                result.Add(trigger);
            }

            return result;
        }

        protected SqliteTableParser ParsedDefinition => LoadTableParserSync();

        protected Task<SqliteTableParser> ParsedDefinitionAsync() => LoadTableParserAsync();

        protected SqliteTableParser LoadTableParserSync()
        {
            string tableSql = null;

            try
            {
                _rwLock.EnterReadLock();
                const string sql = "select sql from sqlite_master where type = 'table' and name = @TableName";
                tableSql = Connection.ExecuteScalar<string>(sql, new { TableName = Name.LocalName });
                if (tableSql == _createTableSql)
                    return _parser;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            try
            {
                _rwLock.EnterWriteLock();
                _createTableSql = tableSql;
                var tokenizer = new SqliteTokenizer();
                var tokens = tokenizer.Tokenize(_createTableSql);
                _parser = new SqliteTableParser(tokens);
                return _parser;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        protected async Task<SqliteTableParser> LoadTableParserAsync()
        {
            string tableSql = null;

            try
            {
                _rwLock.EnterReadLock();
                const string sql = "select sql from sqlite_master where type = 'table' and name = @TableName";
                tableSql = await Connection.ExecuteScalarAsync<string>(sql, new { TableName = Name.LocalName });
                if (tableSql == _createTableSql)
                    return _parser;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            try
            {
                _rwLock.EnterWriteLock();
                _createTableSql = tableSql;
                var tokenizer = new SqliteTokenizer();
                var tokens = tokenizer.Tokenize(_createTableSql);
                _parser = new SqliteTableParser(tokens);
                return _parser;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        protected static RelationalKeyUpdateAction GetRelationalUpdateAction(string pragmaUpdateAction)
        {
            if (pragmaUpdateAction.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(pragmaUpdateAction));

            return _relationalUpdateMapping.ContainsKey(pragmaUpdateAction)
                ? _relationalUpdateMapping[pragmaUpdateAction]
                : RelationalKeyUpdateAction.NoAction;
        }

        private static readonly IReadOnlyDictionary<string, RelationalKeyUpdateAction> _relationalUpdateMapping = new Dictionary<String, RelationalKeyUpdateAction>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = RelationalKeyUpdateAction.NoAction,
            ["RESTRICT"] = RelationalKeyUpdateAction.NoAction,
            ["SET NULL"] = RelationalKeyUpdateAction.SetNull,
            ["SET DEFAULT"] = RelationalKeyUpdateAction.SetDefault,
            ["CASCADE"] = RelationalKeyUpdateAction.Cascade
        };

        private string _createTableSql;
        private SqliteTableParser _parser;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
    }
}
