using Dapper;
using Superpower;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Query;
using SJP.Schematic.Sqlite.Parsing;
using SJP.Schematic.Sqlite.Pragma;
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
            Comparer = new IdentifierComparer(StringComparer.OrdinalIgnoreCase, defaultSchema: Database.DefaultSchema);

            var schemaName = tableName.Schema ?? database.DefaultSchema;
            var localName = tableName.LocalName;

            Name = new Identifier(schemaName, localName);
            Pragma = new DatabasePragma(Database.Dialect, connection, schemaName);
        }

        public IRelationalDatabase Database { get; }

        protected IDbConnection Connection { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected DatabasePragma Pragma { get; }

        public Identifier Name { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync() => LoadPrimaryKeyAsync();

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            var tableInfos = Pragma.TableInfo(Name);
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
            var pkConstraint = parser.PrimaryKey;

            var pkStringName = pkConstraint?.Name;
            var primaryKeyName = !pkStringName.IsNullOrWhiteSpace() ? new LocalIdentifier(pkStringName) : null;
            return new SqliteDatabaseKey(this, primaryKeyName, DatabaseKeyType.Primary, columns);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync()
        {
            var tableInfos = await Pragma.TableInfoAsync(Name).ConfigureAwait(false);
            if (tableInfos.Empty())
                return null;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Count == 0)
                return null;

            var tableColumn = await ColumnAsync().ConfigureAwait(false);
            var columns = pkColumns.Select(c => tableColumn[c.name]).ToList();

            var parser = await ParsedDefinitionAsync().ConfigureAwait(false);
            var pkConstraint = parser.PrimaryKey;

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

            var indexes = await IndexesAsync().ConfigureAwait(false);
            var namedIndexes = indexes.Where(i => i.Name != null);

            foreach (var index in namedIndexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseTableIndex> LoadIndexesSync()
        {
            var indexLists = Pragma.IndexList(Name);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c").ToList();
            if (nonConstraintIndexLists.Count == 0)
                return Enumerable.Empty<IDatabaseTableIndex>();

            var result = new List<IDatabaseTableIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                var indexInfo = Pragma.IndexXInfo(indexList.name);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Select(i => new SqliteDatabaseIndexColumn(Column[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key && i.cid >= 0)
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
            var indexLists = await Pragma.IndexListAsync(Name).ConfigureAwait(false);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseTableIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c").ToList();
            if (nonConstraintIndexLists.Count == 0)
                return Enumerable.Empty<IDatabaseTableIndex>();

            var result = new List<IDatabaseTableIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                var indexInfo = await Pragma.IndexXInfoAsync(indexList.name).ConfigureAwait(false);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Select(i => new SqliteDatabaseIndexColumn(Column[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key && i.cid >= 0)
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

            var uniqueKeys = await UniqueKeysAsync().ConfigureAwait(false);
            var namedUniqueKeys = uniqueKeys.Where(uk => uk.Name != null);

            foreach (var uk in namedUniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseKey> LoadUniqueKeysSync()
        {
            var indexLists = Pragma.IndexList(Name);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Count == 0)
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);

            var parser = ParsedDefinition;
            var parsedUniqueConstraints = parser.UniqueKeys;

            var tableColumn = Column;
            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = Pragma.IndexXInfo(ukIndexList.name);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno);
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var columns = orderedColumns.Select(i => tableColumn[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
                var stringConstraintName = uniqueConstraint?.Name;

                var keyName = !stringConstraintName.IsNullOrWhiteSpace() ? new LocalIdentifier(stringConstraintName) : null;
                var uniqueKey = new SqliteDatabaseKey(this, keyName, DatabaseKeyType.Unique, columns);
                result.Add(uniqueKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseKey>> LoadUniqueKeysAsync()
        {
            var indexLists = await Pragma.IndexListAsync(Name).ConfigureAwait(false);
            if (indexLists.Empty())
                return Enumerable.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Count == 0)
                return Enumerable.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);

            var parser = await ParsedDefinitionAsync().ConfigureAwait(false);
            var parsedUniqueConstraints = parser.UniqueKeys;

            var tableColumn = await ColumnAsync().ConfigureAwait(false);
            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = await Pragma.IndexXInfoAsync(ukIndexList.name).ConfigureAwait(false);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno);
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var columns = orderedColumns.Select(i => tableColumn[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
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
                .Where(t => string.Equals(t.Name.Schema, Name.Schema, StringComparison.OrdinalIgnoreCase))
                .SelectMany(t => t.ParentKeys)
                .Where(fk => Comparer.Equals(Name, fk.ParentKey.Table.Name))
                .ToList();
        }

        protected virtual async Task<IEnumerable<IDatabaseRelationalKey>> LoadChildKeysAsync()
        {
            var dbTables = await Database.TablesAsync().ConfigureAwait(false);

            var childKeys = await dbTables
                .Where(t => string.Equals(t.Name.Schema, Name.Schema, StringComparison.OrdinalIgnoreCase))
                .SelectMany(t => t.ParentKeys.ToAsyncEnumerable())
                .Where(fk => Comparer.Equals(Name, fk.ParentKey.Table.Name))
                .ToList()
                .ConfigureAwait(false);

            return childKeys;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => LoadCheckLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync() => LoadCheckLookupAsync();

        public IEnumerable<IDatabaseCheckConstraint> Checks => LoadChecksSync();

        public Task<IEnumerable<IDatabaseCheckConstraint>> ChecksAsync() => LoadChecksAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            var namedChecks = Checks.Where(c => c.Name != null);
            foreach (var check in namedChecks)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(Comparer);

            var checks = await ChecksAsync().ConfigureAwait(false);
            var namedChecks = checks.Where(c => c.Name != null);

            foreach (var check in namedChecks)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseCheckConstraint> LoadChecksSync()
        {
            var parser = ParsedDefinition;
            var checks = parser.Checks.ToList();
            if (checks.Count == 0)
                return Enumerable.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>(checks.Count);

            foreach (var ck in checks)
            {
                var startIndex = ck.Definition.First().Position.Absolute;
                var lastToken = ck.Definition.Last();
                var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                var definition = parser.Definition.Substring(startIndex, endIndex - startIndex);
                var check = new SqliteCheckConstraint(this, ck.Name, definition);
                result.Add(check);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseCheckConstraint>> LoadChecksAsync()
        {
            var parser = await ParsedDefinitionAsync().ConfigureAwait(false);
            var checks = parser.Checks.ToList();
            if (checks.Count == 0)
                return Enumerable.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>(checks.Count);

            foreach (var ck in checks)
            {
                var startIndex = ck.Definition.First().Position.Absolute;
                var lastToken = ck.Definition.Last();
                var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                var definition = parser.Definition.Substring(startIndex, endIndex - startIndex);
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

            var parentKeys = await ParentKeysAsync().ConfigureAwait(false);
            var namedParentKeys = parentKeys.Where(fk => fk.ChildKey.Name != null);

            foreach (var parentKey in namedParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IEnumerable<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            var queryResult = Pragma.ForeignKeyList(Name);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update }).ToList();
            if (foreignKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var parser = ParsedDefinition;
            var fkConstraints = parser.ParentKeys;

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.seq);

                var parentTableName = new Identifier(Name.Schema, fkey.Key.ParentTableName);
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

                // don't need to check for the parent schema as cross-schema references are not supported
                var parsedConstraint = fkConstraints
                    .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint?.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? new LocalIdentifier(constraintStringName) : null;
                var childKeyColumnLookup = Column;
                var childKeyColumns = rows.Select(row => childKeyColumnLookup[row.from]).ToList();

                var childKey = new SqliteDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = GetRelationalUpdateRule(fkey.Key.OnDelete);
                var updateRule = GetRelationalUpdateRule(fkey.Key.OnUpdate);

                var relationalKey = new SqliteRelationalKey(childKey, parentConstraint, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseRelationalKey>> LoadParentKeysAsync()
        {
            var queryResult = await Pragma.ForeignKeyListAsync(Name).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update }).ToList();
            if (foreignKeys.Count == 0)
                return Enumerable.Empty<IDatabaseRelationalKey>();

            var parser = await ParsedDefinitionAsync().ConfigureAwait(false);
            var fkConstraints = parser.ParentKeys;

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.seq);

                var parentTableName = new Identifier(Name.Schema, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName).ConfigureAwait(false);

                var parentColumns = await parentTable.ColumnsAsync().ConfigureAwait(false);
                parentColumns = rows.Select(row => parentTable.Column[row.to]).ToList();

                var parentPrimaryKey = await parentTable.PrimaryKeyAsync().ConfigureAwait(false);
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
                    var uniqueKeys = await parentTable.UniqueKeysAsync().ConfigureAwait(false);
                    parentConstraint = uniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                }

                var parentKey = new SqliteDatabaseKey(this, parentConstraint.Name, parentKeyType, parentColumns);

                // don't need to check for the parent schema as cross-schema references are not supported
                var parsedConstraint = fkConstraints
                    .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint?.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? new LocalIdentifier(constraintStringName) : null;
                var childKeyColumnLookup = await ColumnAsync().ConfigureAwait(false);
                var childKeyColumns = rows.Select(row => childKeyColumnLookup[row.from]).ToList();

                var childKey = new SqliteDatabaseKey(this, childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = GetRelationalUpdateRule(fkey.Key.OnDelete);
                var updateRule = GetRelationalUpdateRule(fkey.Key.OnUpdate);

                var relationalKey = new SqliteRelationalKey(childKey, parentConstraint, deleteRule, updateRule);
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

            var columns = await ColumnsAsync().ConfigureAwait(false);
            var namedColumns = columns.Where(c => c.Name != null);

            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyList<IDatabaseTableColumn> LoadColumnsSync()
        {
            var tableInfos = Pragma.TableInfo(Name);
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
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var column = new SqliteDatabaseTableColumn(this, tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseTableColumn>> LoadColumnsAsync()
        {
            var tableInfos = await Pragma.TableInfoAsync(Name).ConfigureAwait(false);

            var result = new List<IDatabaseTableColumn>();
            if (tableInfos.Empty())
                return result;

            var parser = await ParsedDefinitionAsync().ConfigureAwait(false);
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
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var column = new SqliteDatabaseTableColumn(this, tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, autoIncrement);
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

            var triggers = await TriggersAsync().ConfigureAwait(false);
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
                var tokenizeResult = tokenizer.TryTokenize(triggerInfo.sql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the TRIGGER statement: " + triggerInfo.sql);

                var tokens = tokenizeResult.Value;
                var parser = new SqliteTriggerParser(tokens);

                var trigger = new SqliteDatabaseTrigger(this, triggerInfo.name, triggerInfo.sql, parser.Timing, parser.Event);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseTrigger>> LoadTriggersAsync()
        {
            const string sql = "select * from sqlite_master where type = 'trigger' and tbl_name = @TableName";
            var triggerInfos = await Connection.QueryAsync<SqliteMaster>(sql, new { TableName = Name.LocalName }).ConfigureAwait(false);

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var tokenizer = new SqliteTokenizer();
                var tokenizeResult = tokenizer.TryTokenize(triggerInfo.sql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the TRIGGER statement: " + triggerInfo.sql);

                var tokens = tokenizeResult.Value;
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
                var tokenizeResult = tokenizer.TryTokenize(_createTableSql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the CREATE TABLE statement: " + _createTableSql);

                var tokens = tokenizeResult.Value;
                _parser = new SqliteTableParser(tokens, tableSql);
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
                tableSql = await Connection.ExecuteScalarAsync<string>(sql, new { TableName = Name.LocalName }).ConfigureAwait(false);
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
                var tokenizeResult = tokenizer.TryTokenize(_createTableSql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the CREATE TABLE statement: " + _createTableSql);

                var tokens = tokenizeResult.Value;
                _parser = new SqliteTableParser(tokens, tableSql);
                return _parser;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        protected static Rule GetRelationalUpdateRule(string pragmaUpdateRule)
        {
            if (pragmaUpdateRule.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(pragmaUpdateRule));

            return _relationalUpdateMapping.ContainsKey(pragmaUpdateRule)
                ? _relationalUpdateMapping[pragmaUpdateRule]
                : Rule.None;
        }

        private static readonly IReadOnlyDictionary<string, Rule> _relationalUpdateMapping = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = Rule.None,
            ["RESTRICT"] = Rule.None,
            ["SET NULL"] = Rule.SetNull,
            ["SET DEFAULT"] = Rule.SetDefault,
            ["CASCADE"] = Rule.Cascade
        };

        private string _createTableSql;
        private SqliteTableParser _parser;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
    }
}
