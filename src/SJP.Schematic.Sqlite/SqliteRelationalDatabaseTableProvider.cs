﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
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
        public SqliteRelationalDatabaseTableProvider(IDbConnection connection, ISqliteConnectionPragma pragma, IDatabaseDialect dialect, IDatabaseIdentifierDefaults identifierDefaults, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            ConnectionPragma = pragma ?? throw new ArgumentNullException(nameof(pragma));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
        }

        protected IDbConnection Connection { get; }

        protected ISqliteConnectionPragma ConnectionPragma { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IDatabaseIdentifierDefaults IdentifierDefaults { get; }

        protected IDbTypeProvider TypeProvider { get; }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables
        {
            get
            {
                var qualifiedTableNames = new List<Identifier>();

                var dbNames = ConnectionPragma.DatabaseList.OrderBy(d => d.seq).Select(l => l.name).ToList();
                foreach (var dbName in dbNames)
                {
                    var sql = TablesQuery(dbName);
                    var tableNames = Connection.Query<string>(sql)
                        .Where(name => !IsReservedTableName(name))
                        .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                    qualifiedTableNames.AddRange(tableNames);
                }

                var tables = qualifiedTableNames
                    .Select(LoadTableSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(qualifiedTableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbNamesQuery = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesQuery.OrderBy(d => d.seq).Select(l => l.name).ToList();

            var qualifiedTableNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = TablesQuery(dbName);
                var queryResult = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
                var tableNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedTableNames.AddRange(tableNames);
            }

            var tables = await qualifiedTableNames
                .Select(name => LoadTableAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return tables.ToList();
        }

        protected virtual string TablesQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' order by name";
        }

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (IsReservedTableName(tableName))
                return Option<IRelationalDatabaseTable>.None;

            if (tableName.Schema != null)
                return LoadTableSync(tableName);

            var dbNames = ConnectionPragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = LoadTableSync(qualifiedTableName);

                if (table.IsSome)
                    return table;
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        public OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
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
                return await LoadTableAsync(tableName, cancellationToken)
                    .ToOption()
                    .ConfigureAwait(false);
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = LoadTableAsync(qualifiedTableName, cancellationToken);

                var tableIsSome = await table.IsSome.ConfigureAwait(false);
                if (tableIsSome)
                    return await table.ToOption().ConfigureAwait(false);
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        protected Option<Identifier> GetResolvedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (IsReservedTableName(tableName))
                return Option<Identifier>.None;

            if (tableName.Schema != null)
            {
                var sql = TableNameQuery(tableName.Schema);
                var tableLocalName = Connection.ExecuteScalar<string>(
                    sql,
                    new { TableName = tableName.LocalName }
                );

                if (tableLocalName != null)
                {
                    var tableSchemaName = ConnectionPragma.DatabaseList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, tableName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (tableSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + tableName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableSchemaName, tableLocalName));
                }
            }

            var dbNames = ConnectionPragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = TableNameQuery(dbName);
                var tableLocalName = Connection.ExecuteScalar<string>(sql, new { TableName = tableName.LocalName });

                if (tableLocalName != null)
                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, tableLocalName));
            }

            return Option<Identifier>.None;
        }

        protected OptionAsync<Identifier> GetResolvedTableNameAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
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
                    new { TableName = tableName.LocalName }
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
                var tableLocalName = await Connection.ExecuteScalarAsync<string>(sql, new { TableName = tableName.LocalName }).ConfigureAwait(false);

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

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            var resolvedTableNameOption = GetResolvedTableName(candidateTableName);
            if (resolvedTableNameOption.IsNone)
                return Option<IRelationalDatabaseTable>.None;

            var resolvedTableName = resolvedTableNameOption.UnwrapSome();
            var pragma = new DatabasePragma(Dialect, Connection, resolvedTableName.Schema);
            var parser = GetParsedTableDefinitionSync(resolvedTableName);

            var columns = LoadColumnsSync(pragma, parser, resolvedTableName);
            var columnLookup = GetColumnLookup(columns);
            var primaryKey = LoadPrimaryKeySync(pragma, parser, resolvedTableName, columnLookup);
            var uniqueKeys = LoadUniqueKeysSync(pragma, parser, resolvedTableName, columnLookup);
            var indexes = LoadIndexesSync(pragma, resolvedTableName, columnLookup);
            var checks = LoadChecksSync(parser);
            var triggers = LoadTriggersSync(resolvedTableName);

            var childKeys = LoadChildKeysSync(resolvedTableName, columnLookup, parser);
            var parentKeys = LoadParentKeysSync(pragma, parser, resolvedTableName, columnLookup);

            var table = new RelationalDatabaseTable(
                resolvedTableName,
                columns,
                primaryKey,
                uniqueKeys,
                parentKeys,
                childKeys,
                indexes,
                checks,
                triggers
            );

            return Option<IRelationalDatabaseTable>.Some(table);
        }

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return LoadTableAsyncCore(candidateTableName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseTable>> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var candidateTableName = QualifyTableName(tableName);
            var resolvedTableNameOption = GetResolvedTableNameAsync(candidateTableName);
            var resolvedTableNameOptionIsNone = await resolvedTableNameOption.IsNone.ConfigureAwait(false);
            if (resolvedTableNameOptionIsNone)
                return Option<IRelationalDatabaseTable>.None;

            var resolvedTableName = await resolvedTableNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var pragma = new DatabasePragma(Dialect, Connection, resolvedTableName.Schema);
            var parser = await GetParsedTableDefinitionAsync(resolvedTableName, cancellationToken).ConfigureAwait(false);

            var columnsTask = LoadColumnsAsync(pragma, parser, resolvedTableName, cancellationToken);
            var checksTask = LoadChecksAsync(parser, cancellationToken);
            var triggersTask = LoadTriggersAsync(resolvedTableName, cancellationToken);
            await Task.WhenAll(columnsTask, checksTask, triggersTask).ConfigureAwait(false);

            var columns = columnsTask.Result;
            var columnLookup = GetColumnLookup(columns);
            var checks = checksTask.Result;
            var triggers = triggersTask.Result;

            var primaryKeyTask = LoadPrimaryKeyAsync(pragma, parser, resolvedTableName, columnLookup, cancellationToken);
            var uniqueKeysTask = LoadUniqueKeysAsync(pragma, parser, resolvedTableName, columnLookup, cancellationToken);
            var indexesTask = LoadIndexesAsync(pragma, resolvedTableName, columnLookup, cancellationToken);
            await Task.WhenAll(primaryKeyTask, uniqueKeysTask, indexesTask).ConfigureAwait(false);

            var primaryKey = primaryKeyTask.Result;
            var uniqueKeys = uniqueKeysTask.Result;
            var indexes = indexesTask.Result;

            var childKeysTask = LoadChildKeysAsync(resolvedTableName, columnLookup, parser, cancellationToken);
            var parentKeysTask = LoadParentKeysAsync(pragma, parser, resolvedTableName, columnLookup, cancellationToken);
            await Task.WhenAll(childKeysTask, parentKeysTask).ConfigureAwait(false);

            var childKeys = childKeysTask.Result;
            var parentKeys = parentKeysTask.Result;

            var table = new RelationalDatabaseTable(
                resolvedTableName,
                columns,
                primaryKey,
                uniqueKeys,
                parentKeys,
                childKeys,
                indexes,
                checks,
                triggers
            );

            return Option<IRelationalDatabaseTable>.Some(table);
        }

        protected virtual IDatabaseKey LoadPrimaryKeySync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var tableInfos = pragma.TableInfo(tableName);
            if (tableInfos.Empty())
                return null;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Empty())
                return null;

            var keyColumns = pkColumns.Select(c => columns[c.name]).ToList();

            var pkConstraint = parser.PrimaryKey;
            var pkStringName = pkConstraint?.Name;
            var primaryKeyName = !pkStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(pkStringName) : null;
            return new SqliteDatabaseKey(primaryKeyName, DatabaseKeyType.Primary, keyColumns);
        }

        protected virtual Task<IDatabaseKey> LoadPrimaryKeyAsync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadPrimaryKeyAsyncCore(pragma, parser, tableName, columns, cancellationToken);
        }

        private async Task<IDatabaseKey> LoadPrimaryKeyAsyncCore(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return null;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Empty())
                return null;

            var keyColumns = pkColumns.Select(c => columns[c.name]).ToList();

            var pkConstraint = parser.PrimaryKey;
            var pkStringName = pkConstraint?.Name;
            var primaryKeyName = !pkStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(pkStringName) : null;
            return new SqliteDatabaseKey(primaryKeyName, DatabaseKeyType.Primary, keyColumns);
        }

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync(ISqliteDatabasePragma pragma, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var indexLists = pragma.IndexList(tableName);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c").ToList();
            if (nonConstraintIndexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                var indexInfo = pragma.IndexXInfo(indexList.name);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Select(i => new DatabaseIndexColumn(columns[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
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

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c").ToList();
            if (nonConstraintIndexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                var indexInfo = await pragma.IndexXInfoAsync(indexList.name, cancellationToken).ConfigureAwait(false);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Select(i => new DatabaseIndexColumn(columns[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
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

        protected virtual IReadOnlyCollection<IDatabaseKey> LoadUniqueKeysSync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var indexLists = pragma.IndexList(tableName);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);
            var parsedUniqueConstraints = parser.UniqueKeys;

            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = pragma.IndexXInfo(ukIndexList.name);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .ToList();
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var keyColumns = orderedColumns.Select(i => columns[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
                var stringConstraintName = uniqueConstraint?.Name;

                var keyName = !stringConstraintName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(stringConstraintName) : null;
                var uniqueKey = new SqliteDatabaseKey(keyName, DatabaseKeyType.Unique, keyColumns);
                result.Add(uniqueKey);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadUniqueKeysAsyncCore(pragma, parser, tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var indexLists = await pragma.IndexListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);
            var parsedUniqueConstraints = parser.UniqueKeys;

            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = await pragma.IndexXInfoAsync(ukIndexList.name, cancellationToken).ConfigureAwait(false);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .ToList();
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var keyColumns = orderedColumns.Select(i => columns[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
                var stringConstraintName = uniqueConstraint?.Name;

                var keyName = !stringConstraintName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(stringConstraintName) : null;
                var uniqueKey = new SqliteDatabaseKey(keyName, DatabaseKeyType.Unique, keyColumns);
                result.Add(uniqueKey);
            }

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeysSync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, SqliteTableParser parser)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            var dbNames = ConnectionPragma.DatabaseList
                .Where(d => string.Equals(tableName.Schema, d.name, StringComparison.OrdinalIgnoreCase)) // schema name must match, no cross-schema FKs allowed
                .OrderBy(d => d.seq)
                .Select(l => l.name)
                .ToList();

            var qualifiedChildTableNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = TablesQuery(dbName);
                var queryResult = Connection.Query<string>(sql);
                var tableNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedChildTableNames.AddRange(tableNames);
            }

            var dbPragmaLookup = new Dictionary<string, ISqliteDatabasePragma>();
            var tableParserLookup = new Dictionary<Identifier, SqliteTableParser> { [tableName] = parser };
            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var foreignKeysCache = new Dictionary<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>>();
            var result = new List<IDatabaseRelationalKey>();

            foreach (var childTableName in qualifiedChildTableNames)
            {
                if (!dbPragmaLookup.TryGetValue(childTableName.Schema, out var dbPragma))
                {
                    dbPragma = new DatabasePragma(Dialect, Connection, childTableName.Schema);
                    dbPragmaLookup[childTableName.Schema] = dbPragma;
                }

                if (!tableParserLookup.TryGetValue(childTableName, out var childTableParser))
                {
                    childTableParser = GetParsedTableDefinitionSync(childTableName);
                    tableParserLookup[childTableName] = childTableParser;
                }

                if (!columnLookupsCache.TryGetValue(childTableName, out var childKeyColumnLookup))
                {
                    var childKeyColumns = LoadColumnsSync(dbPragma, childTableParser, childTableName);
                    childKeyColumnLookup = GetColumnLookup(childKeyColumns);
                    columnLookupsCache[tableName] = childKeyColumnLookup;
                }

                if (!foreignKeysCache.TryGetValue(childTableName, out var childTableParentKeys))
                {
                    childTableParentKeys = LoadParentKeysSync(dbPragma, childTableParser, childTableName, childKeyColumnLookup);
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

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, SqliteTableParser parser, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            return LoadChildKeysAsyncCore(tableName, columns, parser, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, SqliteTableParser parser, CancellationToken cancellationToken)
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
                var queryResult = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
                var tableNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedChildTableNames.AddRange(tableNames);
            }

            var dbPragmaLookup = new Dictionary<string, ISqliteDatabasePragma>();
            var tableParserLookup = new Dictionary<Identifier, SqliteTableParser> { [tableName] = parser };
            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var foreignKeysCache = new Dictionary<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>>();
            var result = new List<IDatabaseRelationalKey>();

            foreach (var childTableName in qualifiedChildTableNames)
            {
                if (!dbPragmaLookup.TryGetValue(childTableName.Schema, out var dbPragma))
                {
                    dbPragma = new DatabasePragma(Dialect, Connection, childTableName.Schema);
                    dbPragmaLookup[childTableName.Schema] = dbPragma;
                }

                if (!tableParserLookup.TryGetValue(childTableName, out var childTableParser))
                {
                    childTableParser = await GetParsedTableDefinitionAsync(childTableName, cancellationToken).ConfigureAwait(false);
                    tableParserLookup[childTableName] = childTableParser;
                }

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

        protected virtual IReadOnlyCollection<IDatabaseCheckConstraint> LoadChecksSync(SqliteTableParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            var checks = parser.Checks.ToList();
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>(checks.Count);

            foreach (var ck in checks)
            {
                var startIndex = ck.Definition.First().Position.Absolute;
                var lastToken = ck.Definition.Last();
                var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                var definition = parser.Definition.Substring(startIndex, endIndex - startIndex);
                var check = new SqliteCheckConstraint(ck.Name, definition);
                result.Add(check);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(SqliteTableParser parser, CancellationToken cancellationToken)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            return Task.FromResult(LoadChecksSync(parser));
        }

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadParentKeysSync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var queryResult = pragma.ForeignKeyList(tableName);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update }).ToList();
            if (foreignKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var tableParserLookup = new Dictionary<Identifier, SqliteTableParser> { [tableName] = parser };
            var primaryKeyCache = new Dictionary<Identifier, IDatabaseKey>();
            var uniqueKeyLookupCache = new Dictionary<Identifier, IReadOnlyCollection<IDatabaseKey>>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(tableName.Schema, fkey.Key.ParentTableName);
                var parentTableNameOption = GetResolvedTableName(candidateParentTableName);
                if (parentTableNameOption.IsNone)
                    throw new Exception("Could not find parent table with name: " + candidateParentTableName.ToString());

                var parentTableName = parentTableNameOption.UnwrapSome();
                if (!tableParserLookup.TryGetValue(parentTableName, out var parentTableParser))
                {
                    parentTableParser = GetParsedTableDefinitionSync(parentTableName);
                    tableParserLookup[parentTableName] = parentTableParser;
                }

                if (!columnLookupsCache.TryGetValue(parentTableName, out var parentTableColumnLookup))
                {
                    var parentTableColumns = LoadColumnsSync(pragma, parentTableParser, parentTableName);
                    parentTableColumnLookup = GetColumnLookup(parentTableColumns);
                    columnLookupsCache[parentTableName] = parentTableColumnLookup;
                }

                var rows = fkey.OrderBy(row => row.seq);
                var parentColumns = rows.Select(row => parentTableColumnLookup[row.to]).ToList();

                if (!primaryKeyCache.TryGetValue(parentTableName, out var parentPrimaryKey))
                {
                    parentPrimaryKey = LoadPrimaryKeySync(pragma, parentTableParser, parentTableName, parentTableColumnLookup);
                    primaryKeyCache[parentTableName] = parentPrimaryKey;
                }

                var pkColumnsEqual = parentPrimaryKey != null &&
                    parentPrimaryKey.Columns.Select(col => col.Name).SequenceEqual(parentColumns.Select(col => col.Name));

                IDatabaseKey parentConstraint;
                if (pkColumnsEqual)
                {
                    parentConstraint = parentPrimaryKey;
                }
                else
                {
                    if (!uniqueKeyLookupCache.TryGetValue(parentTableName, out var parentUniqueKeys))
                    {
                        parentUniqueKeys = LoadUniqueKeysSync(pragma, parentTableParser, parentTableName, parentTableColumnLookup);
                        uniqueKeyLookupCache[parentTableName] = parentUniqueKeys;
                    }

                    parentConstraint = parentUniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                }

                // don't need to check for the parent schema as cross-schema references are not supported
                var parsedConstraint = parser.ParentKeys
                    .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint?.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(constraintStringName) : null;
                var childKeyColumns = rows.Select(row => columns[row.from]).ToList();

                var childKey = new SqliteDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = GetRelationalUpdateRule(fkey.Key.OnDelete);
                var updateRule = GetRelationalUpdateRule(fkey.Key.OnUpdate);

                var relationalKey = new DatabaseRelationalKey(tableName, childKey, parentTableName, parentConstraint, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadParentKeysAsyncCore(pragma, parser, tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await pragma.ForeignKeyListAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update }).ToList();
            if (foreignKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var tableParserLookup = new Dictionary<Identifier, SqliteTableParser> { [tableName] = parser };
            var primaryKeyCache = new Dictionary<Identifier, IDatabaseKey>();
            var uniqueKeyLookupCache = new Dictionary<Identifier, IReadOnlyCollection<IDatabaseKey>>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(tableName.Schema, fkey.Key.ParentTableName);
                var parentTableNameOption = GetResolvedTableNameAsync(candidateParentTableName);
                var parentTableNameOptionIsNone = await parentTableNameOption.IsNone.ConfigureAwait(false);
                if (parentTableNameOptionIsNone)
                    throw new Exception("Could not find parent table with name: " + candidateParentTableName.ToString());

                var parentTableName = await parentTableNameOption.UnwrapSomeAsync().ConfigureAwait(false);
                if (!tableParserLookup.TryGetValue(parentTableName, out var parentTableParser))
                {
                    parentTableParser = await GetParsedTableDefinitionAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                    tableParserLookup[parentTableName] = parentTableParser;
                }

                if (!columnLookupsCache.TryGetValue(parentTableName, out var parentTableColumnLookup))
                {
                    var parentTableColumns = await LoadColumnsAsync(pragma, parentTableParser, parentTableName, cancellationToken).ConfigureAwait(false);
                    parentTableColumnLookup = GetColumnLookup(parentTableColumns);
                    columnLookupsCache[parentTableName] = parentTableColumnLookup;
                }

                var rows = fkey.OrderBy(row => row.seq);
                var parentColumns = rows.Select(row => parentTableColumnLookup[row.to]).ToList();

                if (!primaryKeyCache.TryGetValue(parentTableName, out var parentPrimaryKey))
                {
                    parentPrimaryKey = await LoadPrimaryKeyAsync(pragma, parentTableParser, parentTableName, parentTableColumnLookup, cancellationToken).ConfigureAwait(false);
                    primaryKeyCache[parentTableName] = parentPrimaryKey;
                }

                var pkColumnsEqual = parentPrimaryKey != null &&
                    parentPrimaryKey.Columns.Select(col => col.Name).SequenceEqual(parentColumns.Select(col => col.Name));

                IDatabaseKey parentConstraint;
                if (pkColumnsEqual)
                {
                    parentConstraint = parentPrimaryKey;
                }
                else
                {
                    if (!uniqueKeyLookupCache.TryGetValue(parentTableName, out var parentUniqueKeys))
                    {
                        parentUniqueKeys = await LoadUniqueKeysAsync(pragma, parentTableParser, parentTableName, parentTableColumnLookup, cancellationToken).ConfigureAwait(false);
                        uniqueKeyLookupCache[parentTableName] = parentUniqueKeys;
                    }

                    parentConstraint = parentUniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                }

                // don't need to check for the parent schema as cross-schema references are not supported
                var parsedConstraint = parser.ParentKeys
                    .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint?.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(constraintStringName) : null;
                var childKeyColumns = rows.Select(row => columns[row.from]).ToList();

                var childKey = new SqliteDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = GetRelationalUpdateRule(fkey.Key.OnDelete);
                var updateRule = GetRelationalUpdateRule(fkey.Key.OnUpdate);

                var relationalKey = new DatabaseRelationalKey(tableName, childKey, parentTableName, parentConstraint, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableInfos = pragma.TableInfo(tableName);
            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var result = new List<IDatabaseColumn>();
            var parsedColumns = parser.Columns;

            foreach (var tableInfo in tableInfos)
            {
                var parsedColumnInfo = parsedColumns.FirstOrDefault(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadColumnsAsyncCore(pragma, parser, tableName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(ISqliteDatabasePragma pragma, SqliteTableParser parser, Identifier tableName, CancellationToken cancellationToken)
        {
            var tableInfos = await pragma.TableInfoAsync(tableName, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var result = new List<IDatabaseColumn>();
            var parsedColumns = parser.Columns;

            foreach (var tableInfo in tableInfos)
            {
                var parsedColumnInfo = parsedColumns.FirstOrDefault(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual IReadOnlyCollection<IDatabaseTrigger> LoadTriggersSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var triggerQuery = TriggerDefinitionQuery(tableName.Schema);
            var triggerInfos = Connection.Query<SqliteMaster>(triggerQuery, new { TableName = tableName.LocalName });

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var tokenizer = new SqliteTokenizer();
                var tokenizeResult = tokenizer.TryTokenize(triggerInfo.sql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the TRIGGER statement: " + triggerInfo.sql);

                var tokens = tokenizeResult.Value;
                var parser = new SqliteTriggerParser(tokens);

                var trigger = new SqliteDatabaseTrigger(triggerInfo.name, triggerInfo.sql, parser.Timing, parser.Event);
                result.Add(trigger);
            }

            return result;
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
            var triggerInfos = await Connection.QueryAsync<SqliteMaster>(triggerQuery, new { TableName = tableName.LocalName }).ConfigureAwait(false);

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var tokenizer = new SqliteTokenizer();
                var tokenizeResult = tokenizer.TryTokenize(triggerInfo.sql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the TRIGGER statement: " + triggerInfo.sql);

                var tokens = tokenizeResult.Value;
                var parser = new SqliteTriggerParser(tokens);

                var trigger = new SqliteDatabaseTrigger(triggerInfo.name, triggerInfo.sql, parser.Timing, parser.Event);
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

        protected virtual SqliteTableParser GetParsedTableDefinitionSync(Identifier tableName)
        {
            var definitionQuery = TableDefinitionQuery(tableName.Schema);
            var tableSql = Connection.ExecuteScalar<string>(definitionQuery, new { TableName = tableName.LocalName });
            var tokenizer = new SqliteTokenizer();
            var tokenizeResult = tokenizer.TryTokenize(tableSql);
            if (!tokenizeResult.HasValue)
                throw new Exception("Unable to parse the CREATE TABLE statement: " + tableSql);

            var tokens = tokenizeResult.Value;
            return new SqliteTableParser(tokens, tableSql);
        }

        protected virtual Task<SqliteTableParser> GetParsedTableDefinitionAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetParsedTableDefinitionAsyncCore(tableName, cancellationToken);
        }

        private async Task<SqliteTableParser> GetParsedTableDefinitionAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var definitionQuery = TableDefinitionQuery(tableName.Schema);
            var tableSql = await Connection.ExecuteScalarAsync<string>(definitionQuery, new { TableName = tableName.LocalName }).ConfigureAwait(false);
            var tokenizer = new SqliteTokenizer();
            var tokenizeResult = tokenizer.TryTokenize(tableSql);
            if (!tokenizeResult.HasValue)
                throw new Exception("Unable to parse the CREATE TABLE statement: " + tableSql);

            var tokens = tokenizeResult.Value;
            return new SqliteTableParser(tokens, tableSql);
        }

        protected virtual string TableDefinitionQuery(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            return $"select sql from { Dialect.QuoteIdentifier(schema) }.sqlite_master where type = 'table' and tbl_name = @TableName";
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

        protected static Rule GetRelationalUpdateRule(string pragmaUpdateRule)
        {
            if (pragmaUpdateRule.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(pragmaUpdateRule));

            return _relationalUpdateMapping.ContainsKey(pragmaUpdateRule)
                ? _relationalUpdateMapping[pragmaUpdateRule]
                : Rule.None;
        }

        private readonly static IReadOnlyDictionary<string, Rule> _relationalUpdateMapping = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = Rule.None,
            ["RESTRICT"] = Rule.None,
            ["SET NULL"] = Rule.SetNull,
            ["SET DEFAULT"] = Rule.SetDefault,
            ["CASCADE"] = Rule.Cascade
        };

        private readonly static SqliteTypeAffinityParser _affinityParser = new SqliteTypeAffinityParser();
    }
}
