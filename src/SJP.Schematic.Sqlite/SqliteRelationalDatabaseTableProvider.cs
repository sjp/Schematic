using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Sqlite.Parsing;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Sqlite.Query;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public SqliteRelationalDatabaseTableProvider(IDbConnection connection, ISqliteConnectionPragma pragma, IDatabaseDialect dialect, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            ConnectionPragma = pragma ?? throw new ArgumentNullException(nameof(pragma));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected ISqliteConnectionPragma ConnectionPragma { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public virtual async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbNamesQuery = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesQuery.OrderBy(d => d.seq).Select(l => l.name).ToList();

            var qualifiedTableNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = TablesQuery(dbName);
                var queryResult = await Connection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
                var tableNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedTableNames.AddRange(tableNames);
            }

            var tableTasks = qualifiedTableNames
                .Select(name => LoadTableAsyncCore(name, cancellationToken))
                .ToArray();
            return await Task.WhenAll(tableTasks).ConfigureAwait(false);
        }

        protected virtual string TablesQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' order by name";
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetTableAsyncCore(tableName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseTable>> GetTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (IsReservedTableName(tableName))
                return Option<IRelationalDatabaseTable>.None;

            if (tableName.Schema != null)
            {
                return await LoadTable(tableName, cancellationToken)
                    .ToOption()
                    .ConfigureAwait(false);
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = LoadTable(qualifiedTableName, cancellationToken);

                var tableIsSome = await table.IsSome.ConfigureAwait(false);
                if (tableIsSome)
                    return await table.ToOption().ConfigureAwait(false);
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameAsyncCore(tableName, cancellationToken).ToAsync();
        }

        private async Task<Option<Identifier>> GetResolvedTableNameAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (IsReservedTableName(tableName))
                return Option<Identifier>.None;

            if (tableName.Schema != null)
            {
                var sql = TableNameQuery(tableName.Schema);
                var tableLocalName = await Connection.ExecuteScalarAsync<string>(
                    sql,
                    new { TableName = tableName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                if (tableLocalName != null)
                {
                    var dbList = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
                    var tableSchemaName = dbList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, tableName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (tableSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + tableName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableSchemaName, tableLocalName));
                }
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = TableNameQuery(dbName);
                var tableLocalName = await Connection.ExecuteScalarAsync<string>(
                    sql,
                    new { TableName = tableName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                if (tableLocalName != null)
                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, tableLocalName));
            }

            return Option<Identifier>.None;
        }

        protected virtual string TableNameQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
        }

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
            var pragma = GetDatabasePragma(tableName.Schema);
            var parsedTable = await GetParsedTableDefinitionAsync(tableName, cancellationToken).ConfigureAwait(false);

            var columnsTask = LoadColumnsAsync(pragma, parsedTable, tableName, cancellationToken);
            var checksTask = LoadChecksAsync(parsedTable, cancellationToken);
            var triggersTask = LoadTriggersAsync(tableName, cancellationToken);
            await Task.WhenAll(columnsTask, checksTask, triggersTask).ConfigureAwait(false);

            var columns = columnsTask.Result;
            var columnLookup = GetColumnLookup(columns);
            var checks = checksTask.Result;
            var triggers = triggersTask.Result;

            var primaryKeyTask = LoadPrimaryKeyAsync(pragma, parsedTable, tableName, columnLookup, cancellationToken);
            var uniqueKeysTask = LoadUniqueKeysAsync(pragma, parsedTable, tableName, columnLookup, cancellationToken);
            var indexesTask = LoadIndexesAsync(pragma, tableName, columnLookup, cancellationToken);
            await Task.WhenAll(primaryKeyTask, uniqueKeysTask, indexesTask).ConfigureAwait(false);

            var primaryKey = primaryKeyTask.Result;
            var uniqueKeys = uniqueKeysTask.Result;
            var indexes = indexesTask.Result;

            var childKeysTask = LoadChildKeysAsync(tableName, columnLookup, cancellationToken);
            var parentKeysTask = LoadParentKeysAsync(pragma, parsedTable, tableName, columnLookup, cancellationToken);
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

        protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parsedTable == null)
                throw new ArgumentNullException(nameof(parsedTable));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadPrimaryKeyAsyncCore(pragma, parsedTable, tableName, columns, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return Option<IDatabaseKey>.None;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Empty())
                return Option<IDatabaseKey>.None;

            var keyColumns = pkColumns.Select(c => columns[c.name]).ToList();

            var primaryKeyName = parsedTable.PrimaryKey.Bind(c => c.Name.Map(Identifier.CreateQualifiedIdentifier));

            var primaryKey = new SqliteDatabaseKey(primaryKeyName, DatabaseKeyType.Primary, keyColumns);
            return Option<IDatabaseKey>.Some(primaryKey);
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(ISqliteDatabasePragma pragma, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadIndexesAsyncCore(pragma, tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(ISqliteDatabasePragma pragma, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var indexLists = await pragma.IndexListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == Constants.Constraint).ToList();
            if (nonConstraintIndexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                var indexInfo = await pragma.IndexXInfoAsync(indexList.name, cancellationToken).ConfigureAwait(false);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Select(i =>
                    {
                        var order = i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var column = columns[i.name];
                        var expression = Dialect.QuoteName(column.Name);
                        return new DatabaseIndexColumn(expression, column, order);
                    })
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key && i.cid >= 0)
                    .OrderBy(i => i.name)
                    .Select(i => columns[i.name])
                    .ToList();

                var index = new SqliteDatabaseIndex(indexList.name, indexList.unique, indexColumns, includedColumns);
                result.Add(index);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadUniqueKeysAsyncCore(pragma, parsedTable, tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var indexLists = await pragma.IndexListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == Constants.Unique && i.unique)
                .ToList();
            if (ukIndexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);
            var parsedUniqueConstraints = parsedTable.UniqueKeys;

            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = await pragma.IndexXInfoAsync(ukIndexList.name, cancellationToken).ConfigureAwait(false);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .ToList();
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var keyColumns = orderedColumns.Select(i => columns[i.name]).ToList();

                var parsedUniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
                var uniqueConstraint = parsedUniqueConstraint != null
                    ? Option<UniqueKey>.Some(parsedUniqueConstraint)
                    : Option<UniqueKey>.None;
                var keyName = uniqueConstraint.Bind(uc => uc.Name.Map(Identifier.CreateQualifiedIdentifier));

                var uniqueKey = new SqliteDatabaseKey(keyName, DatabaseKeyType.Unique, keyColumns);
                result.Add(uniqueKey);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadChildKeysAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var dbList = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbList
                .Where(d => string.Equals(tableName.Schema, d.name, StringComparison.OrdinalIgnoreCase)) // schema name must match, no cross-schema FKs allowed
                .OrderBy(d => d.seq)
                .Select(l => l.name)
                .ToList();

            var qualifiedChildTableNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = TablesQuery(dbName);
                var queryResult = await Connection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
                var tableNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedChildTableNames.AddRange(tableNames);
            }

            var dbPragmaLookup = new Dictionary<string, ISqliteDatabasePragma>();
            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var foreignKeysCache = new Dictionary<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>>();
            var result = new List<IDatabaseRelationalKey>();

            foreach (var childTableName in qualifiedChildTableNames)
            {
                if (!dbPragmaLookup.TryGetValue(childTableName.Schema, out var dbPragma))
                {
                    dbPragma = GetDatabasePragma(childTableName.Schema);
                    dbPragmaLookup[childTableName.Schema] = dbPragma;
                }

                var childTableParser = await GetParsedTableDefinitionAsync(childTableName, cancellationToken).ConfigureAwait(false);

                if (!columnLookupsCache.TryGetValue(childTableName, out var childKeyColumnLookup))
                {
                    var childKeyColumns = await LoadColumnsAsync(dbPragma, childTableParser, childTableName, cancellationToken).ConfigureAwait(false);
                    childKeyColumnLookup = GetColumnLookup(childKeyColumns);
                    columnLookupsCache[tableName] = childKeyColumnLookup;
                }

                if (!foreignKeysCache.TryGetValue(childTableName, out var childTableParentKeys))
                {
                    childTableParentKeys = await LoadParentKeysAsync(dbPragma, childTableParser, childTableName, childKeyColumnLookup, cancellationToken).ConfigureAwait(false);
                    foreignKeysCache[tableName] = childTableParentKeys;
                }

                var matchingParentKeys = childTableParentKeys
                    .Where(fk => string.Equals(tableName.Schema, fk.ParentTable.Schema, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tableName.LocalName, fk.ParentTable.LocalName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                result.AddRange(matchingParentKeys);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(ParsedTableData parsedTable, CancellationToken cancellationToken)
        {
            if (parsedTable == null)
                throw new ArgumentNullException(nameof(parsedTable));

            var checks = parsedTable.Checks.ToList();
            if (checks.Empty())
                return Empty.Checks;

            var result = new List<IDatabaseCheckConstraint>(checks.Count);

            foreach (var ck in checks)
            {
                var startIndex = ck.Definition.First().Position.Absolute;
                var lastToken = ck.Definition.Last();
                var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                var definition = parsedTable.Definition.Substring(startIndex, endIndex - startIndex);
                var checkName = ck.Name.Map(Identifier.CreateQualifiedIdentifier);
                var check = new SqliteCheckConstraint(checkName, definition);
                result.Add(check);
            }

            return Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(result);
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parsedTable == null)
                throw new ArgumentNullException(nameof(parsedTable));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadParentKeysAsyncCore(pragma, parsedTable, tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await pragma.ForeignKeyListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update }).ToList();
            if (foreignKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var primaryKeyCache = new Dictionary<Identifier, Option<IDatabaseKey>>();
            var uniqueKeyLookupCache = new Dictionary<Identifier, IReadOnlyCollection<IDatabaseKey>>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(tableName.Schema, fkey.Key.ParentTableName);
                Identifier parentTableName = null;
                await GetResolvedTableName(candidateParentTableName, cancellationToken)
                    .BindAsync(async name =>
                    {
                        parentTableName = name;
                        var parentTableParser = await GetParsedTableDefinitionAsync(name, cancellationToken).ConfigureAwait(false);

                        if (!columnLookupsCache.TryGetValue(name, out var parentTableColumnLookup))
                        {
                            var parentTableColumns = await LoadColumnsAsync(pragma, parentTableParser, name, cancellationToken).ConfigureAwait(false);
                            parentTableColumnLookup = GetColumnLookup(parentTableColumns);
                            columnLookupsCache[name] = parentTableColumnLookup;
                        }

                        var rows = fkey.OrderBy(row => row.seq).ToList();
                        var parentColumns = rows.Select(row => parentTableColumnLookup[row.to]).ToList();

                        if (!primaryKeyCache.TryGetValue(name, out var parentPrimaryKey))
                        {
                            parentPrimaryKey = await LoadPrimaryKeyAsync(pragma, parentTableParser, name, parentTableColumnLookup, cancellationToken).ConfigureAwait(false);
                            primaryKeyCache[name] = parentPrimaryKey;
                        }

                        var pkColumnsEqual = parentPrimaryKey
                            .Match(
                                k => k.Columns.Select(col => col.Name).SequenceEqual(parentColumns.Select(col => col.Name)),
                                () => false
                            );
                        if (pkColumnsEqual)
                            return parentPrimaryKey.ToAsync();

                        if (!uniqueKeyLookupCache.TryGetValue(name, out var parentUniqueKeys))
                        {
                            parentUniqueKeys = await LoadUniqueKeysAsync(pragma, parentTableParser, name, parentTableColumnLookup, cancellationToken).ConfigureAwait(false);
                            uniqueKeyLookupCache[name] = parentUniqueKeys;
                        }

                        var parentUniqueKey = parentUniqueKeys.FirstOrDefault(uk =>
                            uk.Columns.Select(ukCol => ukCol.Name)
                                .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                        return parentUniqueKey != null
                            ? OptionAsync<IDatabaseKey>.Some(parentUniqueKey)
                            : OptionAsync<IDatabaseKey>.None;
                    })
                    .Map(key =>
                    {
                        var rows = fkey.OrderBy(row => row.seq).ToList();

                        // don't need to check for the parent schema as cross-schema references are not supported
                        var parsedConstraint = parsedTable.ParentKeys
                            .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                        var parsedConstraintOption = parsedConstraint != null
                            ? Option<ForeignKey>.Some(parsedConstraint)
                            : Option<ForeignKey>.None;

                        var childKeyName = parsedConstraintOption.Bind(fk => fk.Name.Map(Identifier.CreateQualifiedIdentifier));
                        var childKeyColumns = rows.Select(row => columns[row.from]).ToList();

                        var childKey = new SqliteDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                        var deleteAction = GetReferentialAction(fkey.Key.OnDelete);
                        var updateAction = GetReferentialAction(fkey.Key.OnUpdate);

                        return new DatabaseRelationalKey(tableName, childKey, parentTableName, key, deleteAction, updateAction);
                    })
                    .IfSome(key => result.Add(key))
                    .ConfigureAwait(false);
            }

            return result;
        }

        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parsedTable == null)
                throw new ArgumentNullException(nameof(parsedTable));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadColumnsAsyncCore(pragma, parsedTable, tableName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(ISqliteDatabasePragma pragma, ParsedTableData parsedTable, Identifier tableName, CancellationToken cancellationToken)
        {
            var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var result = new List<IDatabaseColumn>();
            var parsedColumns = parsedTable.Columns;

            foreach (var tableInfo in tableInfos)
            {
                var parsedColumnInfo = parsedColumns.First(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                var affinity = AffinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                    : Option<IAutoIncrement>.None;
                var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                    ? Option<string>.Some(tableInfo.dflt_value)
                    : Option<string>.None;

                var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, defaultValue, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            return LoadTriggersAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var triggerQuery = TriggerDefinitionQuery(tableName.Schema);
            var triggerInfos = await Connection.QueryAsync<SqliteMaster>(
                triggerQuery,
                new { TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var triggerSql = triggerInfo.sql;

                var parsedTrigger = _triggerParserCache.GetOrAdd(triggerSql, sql => new Lazy<ParsedTriggerData>(() =>
                {
                    var tokenizeResult = Tokenizer.TryTokenize(sql);
                    if (!tokenizeResult.HasValue)
                        throw new SqliteTriggerParsingException(tableName, triggerInfo.sql, tokenizeResult.ErrorMessage + " at " + tokenizeResult.ErrorPosition.ToString());

                    var tokens = tokenizeResult.Value;
                    return TriggerParser.ParseTokens(tokens);
                })).Value;

                var trigger = new SqliteDatabaseTrigger(triggerInfo.name, triggerSql, parsedTrigger.Timing, parsedTrigger.Event);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual string TriggerDefinitionQuery(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            return $"select * from { Dialect.QuoteIdentifier(schema) }.sqlite_master where type = 'trigger' and tbl_name = @TableName";
        }

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

        protected virtual Task<ParsedTableData> GetParsedTableDefinitionAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetParsedTableDefinitionAsyncCore(tableName, cancellationToken);
        }

        private async Task<ParsedTableData> GetParsedTableDefinitionAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var definitionQuery = TableDefinitionQuery(tableName.Schema);
            var tableSql = await Connection.ExecuteScalarAsync<string>(
                definitionQuery,
                new { TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            return _tableParserCache.GetOrAdd(tableSql, sql => new Lazy<ParsedTableData>(() =>
            {
                var tokenizeResult = Tokenizer.TryTokenize(sql);
                if (!tokenizeResult.HasValue)
                    throw new SqliteTableParsingException(tableName, tableSql, tokenizeResult.ErrorMessage + " at " + tokenizeResult.ErrorPosition.ToString());

                var tokens = tokenizeResult.Value;
                return TableParser.ParseTokens(sql, tokens);
            })).Value;
        }

        protected virtual string TableDefinitionQuery(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            return $"select sql from { Dialect.QuoteIdentifier(schema) }.sqlite_master where type = 'table' and tbl_name = @TableName";
        }

        protected virtual ISqliteDatabasePragma GetDatabasePragma(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            var loader = _dbPragmaCache.GetOrAdd(schema, new Lazy<ISqliteDatabasePragma>(() => new DatabasePragma(Dialect, Connection, schema)));
            return loader.Value;
        }

        protected static bool IsReservedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return tableName.LocalName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase);
        }

        protected Identifier QualifyTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var schema = tableName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(schema, tableName.LocalName);
        }

        protected static ReferentialAction GetReferentialAction(string pragmaUpdateAction)
        {
            if (pragmaUpdateAction.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(pragmaUpdateAction));

            return RelationalUpdateMapping.ContainsKey(pragmaUpdateAction)
                ? RelationalUpdateMapping[pragmaUpdateAction]
                : ReferentialAction.NoAction;
        }

        private readonly ConcurrentDictionary<string, Lazy<ParsedTableData>> _tableParserCache = new ConcurrentDictionary<string, Lazy<ParsedTableData>>();
        private readonly ConcurrentDictionary<string, Lazy<ParsedTriggerData>> _triggerParserCache = new ConcurrentDictionary<string, Lazy<ParsedTriggerData>>();
        private readonly ConcurrentDictionary<string, Lazy<ISqliteDatabasePragma>> _dbPragmaCache = new ConcurrentDictionary<string, Lazy<ISqliteDatabasePragma>>();

        private static readonly IReadOnlyDictionary<string, ReferentialAction> RelationalUpdateMapping = new Dictionary<string, ReferentialAction>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = ReferentialAction.NoAction,
            ["RESTRICT"] = ReferentialAction.Restrict,
            ["SET NULL"] = ReferentialAction.SetNull,
            ["SET DEFAULT"] = ReferentialAction.SetDefault,
            ["CASCADE"] = ReferentialAction.Cascade
        };

        private static readonly SqliteTypeAffinityParser AffinityParser = new SqliteTypeAffinityParser();
        private static readonly SqliteTokenizer Tokenizer = new SqliteTokenizer();
        private static readonly SqliteTableParser TableParser = new SqliteTableParser();
        private static readonly SqliteTriggerParser TriggerParser = new SqliteTriggerParser();

        private static class Constants
        {
            public const string Constraint = "c";

            public const string Unique = "u";
        }
    }
}
