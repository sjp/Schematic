using Dapper;
using Superpower;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SJP.Schema.Core.Utilities;
using SJP.Schema.Core;
using SJP.Schema.Sqlite.Query;
using SJP.Schema.Sqlite.Parsing;

namespace SJP.Schema.Sqlite
{
    public class SqliteRelationalDatabaseTable : IRelationalDatabaseTable, IRelationalDatabaseTableAsync
    {
        public SqliteRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = tableName ?? throw new ArgumentNullException(nameof(tableName));

            _parser = new AsyncLazy<SqliteTableParser>(LoadTableParserAsync);

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
            //var results = new List<Identifier>();


            // TODO: use views and child keys somehow..


            var result = await Task.FromResult(Enumerable.Empty<Identifier>());
            return result;

            //return results;
        }

        private async Task<IEnumerable<Identifier>> LoadDependenciesAsync()
        {
            //var results = new List<Identifier>();

            // TODO: use foreign keys and maybe parse check constraints...?


            var result = await Task.FromResult(Enumerable.Empty<Identifier>());
            return result;

            //return results;
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

        private async Task<IReadOnlyDictionary<string, IDatabaseTableIndex>> LoadIndexLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseTableIndex>();

            var indexes = await IndexesAsync();
            var namedIndexes = indexes.Where(i => i.Name != null);

            foreach (var index in namedIndexes)
                result[index.Name.LocalName] = index;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseTableIndex>> LoadIndexesAsync()
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

        private async Task<IReadOnlyDictionary<string, IDatabaseKey>> LoadUniqueKeyLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseKey>();

            var uniqueKeys = await UniqueKeysAsync();
            var namedUniqueKeys = uniqueKeys.Where(uk => uk.Name != null);

            foreach (var uk in namedUniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseKey>> LoadUniqueKeysAsync()
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

        private Task<IEnumerable<IDatabaseRelationalKey>> LoadChildKeysAsync()
        {
            // will be relatively slow the first time
            // enumerates every table then tries to find all relationships pointing to the table
            // this is not slow in practice (SQLite is fast for most purposes) though...

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

        private async Task<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> LoadCheckConstraintLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseCheckConstraint>();

            var checks = await CheckConstraintsAsync();
            var namedChecks = checks.Where(c => c.Name != null);

            foreach (var check in namedChecks)
                result[check.Name.LocalName] = check;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseCheckConstraint>> LoadCheckConstraintsAsync()
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

        private async Task<IReadOnlyDictionary<string, IDatabaseRelationalKey>> LoadParentKeyLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseRelationalKey>();

            var parentKeys = await ParentKeysAsync();
            var namedParentKeys = parentKeys.Where(fk => fk.ChildKey.Name != null);

            foreach (var parentKey in namedParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IEnumerable<IDatabaseRelationalKey>> LoadParentKeysAsync()
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

        private async Task<IReadOnlyDictionary<string, IDatabaseTableColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseTableColumn>();

            var columns = await ColumnsAsync();
            var namedColumns = columns.Where(c => c.Name != null);

            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result.ToReadOnlyDictionary();
        }

        private async Task<IList<IDatabaseTableColumn>> LoadColumnsAsync()
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

                // TODO: Change sqlite types so that they're unconstrained
                //       This is how the database implements it anyway. Perhaps map some of the builtin type names and affinities
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

        protected SqliteTableParser ParsedDefinition => _parser.Task.Result;

        protected Task<SqliteTableParser> ParsedDefinitionAsync() => _parser.Task;

        protected async Task<SqliteTableParser> LoadTableParserAsync()
        {
            const string sql = "select sql from sqlite_master where type = 'table' and name = @TableName";
            var tableSql = await Connection.ExecuteScalarAsync<string>(sql, new { TableName = Name.LocalName });

            var tokenizer = new SqliteTokenizer();
            var tokens = tokenizer.Tokenize(tableSql);
            return new SqliteTableParser(tokens);
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

        private readonly AsyncLazy<SqliteTableParser> _parser;

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
