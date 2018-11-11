﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using LanguageExt;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabase : RelationalDatabase, ISqliteDatabase
    {
        public SqliteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, string defaultSchema = "main")
            : base(dialect, connection)
        {
            if (defaultSchema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(defaultSchema));

            DatabaseName = Connection.Database;
            DefaultSchema = defaultSchema;
            _versionLoader = new AsyncLazy<string>(LoadDatabaseVersion);
            Pragma = new ConnectionPragma(Dialect, Connection);
        }

        public string ServerName => null; // never not-null

        public string DatabaseName { get; }

        public string DefaultSchema { get; }

        public string DatabaseVersion => _versionLoader.Task.GetAwaiter().GetResult();

        protected ISqliteConnectionPragma Pragma { get; }

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
                    var tableSchemaName = Pragma.DatabaseList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, tableName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (tableSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + tableName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableSchemaName, tableLocalName));
                }
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = TableNameQuery(dbName);
                var tableLocalName = Connection.ExecuteScalar<string>(sql, new { TableName = tableName.LocalName });

                if (tableLocalName != null)
                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, tableLocalName));
            }

            return Option<Identifier>.None;
        }

        protected Task<Option<Identifier>> GetResolvedTableNameAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameAsyncCore(tableName, cancellationToken);
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
                    var dbList = await Pragma.DatabaseListAsync().ConfigureAwait(false);
                    var tableSchemaName = dbList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, tableName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (tableSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + tableName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableSchemaName, tableLocalName));
                }
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
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

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (IsReservedTableName(tableName))
                return Option<IRelationalDatabaseTable>.None;

            if (tableName.Schema != null)
                return LoadTableSync(tableName);

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = LoadTableSync(qualifiedTableName);

                if (table.IsSome)
                    return table;
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        public Task<Option<IRelationalDatabaseTable>> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseTable>> GetTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (IsReservedTableName(tableName))
                return Option<IRelationalDatabaseTable>.None;

            if (tableName.Schema != null)
                return await LoadTableAsync(tableName, cancellationToken).ConfigureAwait(false);

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = await LoadTableAsync(qualifiedTableName, cancellationToken).ConfigureAwait(false);

                if (table.IsSome)
                    return table;
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables
        {
            get
            {
                var qualifiedTableNames = new List<Identifier>();

                var dbNames = Pragma.DatabaseList.OrderBy(d => d.seq).Select(l => l.name).ToList();
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
                    .Where(t => t.IsSome)
                    .Select(t => t.UnwrapSome());
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(qualifiedTableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbNamesQuery = await Pragma.DatabaseListAsync().ConfigureAwait(false);
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

            var tables = qualifiedTableNames
                .Select(name => LoadTableAsync(name, cancellationToken))
                .Where(t => t.IsSome)
                .Select(t => t.UnwrapSome());
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseTable>>(qualifiedTableNames.Count, tables);
        }

        protected virtual string TablesQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' order by name";
        }

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Schema != null)
            {
                return GetResolvedTableName(tableName)
                    .Map<IRelationalDatabaseTable>(name => new SqliteRelationalDatabaseTable(Connection, this, name));
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = GetResolvedTableName(qualifiedTableName)
                    .Map<IRelationalDatabaseTable>(name => new SqliteRelationalDatabaseTable(Connection, this, name));

                if (table.IsSome)
                    return table;
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        protected virtual Task<Option<IRelationalDatabaseTable>> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseTable>> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName.Schema != null)
            {
                var qualifiedTableName = await GetResolvedTableNameAsync(tableName, cancellationToken).ConfigureAwait(false);
                return qualifiedTableName.Map<IRelationalDatabaseTable>(name => new SqliteRelationalDatabaseTable(Connection, this, name));
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var resolvedTableName = await GetResolvedTableNameAsync(qualifiedTableName, cancellationToken).ConfigureAwait(false);

                var table = resolvedTableName.Map<IRelationalDatabaseTable>(name => new SqliteRelationalDatabaseTable(Connection, this, name));
                if (table.IsSome)
                    return table;
            }

            return Option<IRelationalDatabaseTable>.None;
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                var sql = ViewNameQuery(viewName.Schema);
                var viewLocalName = Connection.ExecuteScalar<string>(
                    sql,
                    new { ViewName = viewName.LocalName }
                );

                if (viewLocalName != null)
                {
                    var viewSchemaName = Pragma.DatabaseList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, viewName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (viewSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + viewName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(viewSchemaName, viewLocalName));
                }
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = ViewNameQuery(dbName);
                var viewLocalName = Connection.ExecuteScalar<string>(sql, new { ViewName = viewName.LocalName });

                if (viewLocalName != null)
                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, viewLocalName));
            }

            return Option<Identifier>.None;
        }

        public Task<Option<Identifier>> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetResolvedViewNameAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedViewNameAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
            {
                var sql = ViewNameQuery(viewName.Schema);
                var viewLocalName = await Connection.ExecuteScalarAsync<string>(
                    sql,
                    new { ViewName = viewName.LocalName }
                ).ConfigureAwait(false);

                if (viewLocalName != null)
                {
                    var dbList = await Pragma.DatabaseListAsync().ConfigureAwait(false);
                    var viewSchemaName = dbList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, viewName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (viewSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + viewName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(viewSchemaName, viewLocalName));
                }
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = ViewNameQuery(dbName);
                var viewLocalName = await Connection.ExecuteScalarAsync<string>(sql, new { ViewName = viewName.LocalName }).ConfigureAwait(false);

                if (viewLocalName != null)
                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, viewLocalName));
            }

            return Option<Identifier>.None;
        }

        protected virtual string ViewNameQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
        }

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
                return LoadViewSync(viewName);

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var view = LoadViewSync(qualifiedViewName);

                if (view.IsSome)
                    return view;
            }

            return Option<IRelationalDatabaseView>.None;
        }

        public Task<Option<IRelationalDatabaseView>> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseView>> GetViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
                return await LoadViewAsync(viewName, cancellationToken).ConfigureAwait(false);

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var view = await LoadViewAsync(qualifiedViewName, cancellationToken).ConfigureAwait(false);

                if (view.IsSome)
                    return view;
            }

            return Option<IRelationalDatabaseView>.None;
        }

        public IReadOnlyCollection<IRelationalDatabaseView> Views
        {
            get
            {
                var qualifiedViewNames = new List<Identifier>();

                var dbNames = Pragma.DatabaseList.OrderBy(d => d.seq).Select(d => d.name).ToList();
                foreach (var dbName in dbNames)
                {
                    var sql = ViewsQuery(dbName);
                    var viewNames = Connection.Query<string>(sql)
                        .Where(name => !IsReservedTableName(name))
                        .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                    qualifiedViewNames.AddRange(viewNames);
                }

                var views = qualifiedViewNames
                    .Select(LoadViewSync)
                    .Where(v => v.IsSome)
                    .Select(v => v.UnwrapSome());
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(qualifiedViewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbNamesQuery = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesQuery.OrderBy(d => d.seq).Select(l => l.name).ToList();

            var qualifiedViewNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = ViewsQuery(dbName);
                var queryResult = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
                var viewNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedViewNames.AddRange(viewNames);
            }

            var views = qualifiedViewNames
                .Select(name => LoadViewAsync(name, cancellationToken))
                .Where(v => v.IsSome)
                .Select(v => v.UnwrapSome());
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseView>>(qualifiedViewNames.Count, views);
        }

        protected virtual string ViewsQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' order by name";
        }

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                return GetResolvedViewName(viewName)
                    .Map<IRelationalDatabaseView>(name => new SqliteRelationalDatabaseView(Connection, this, name));
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var view = GetResolvedViewName(qualifiedViewName)
                    .Map<IRelationalDatabaseView>(name => new SqliteRelationalDatabaseView(Connection, this, name));

                if (view.IsSome)
                    return view;
            }

            return Option<IRelationalDatabaseView>.None;
        }

        protected virtual Task<Option<IRelationalDatabaseView>> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
            {
                var qualifiedViewName = await GetResolvedViewNameAsync(viewName, cancellationToken).ConfigureAwait(false);
                return qualifiedViewName.Map<IRelationalDatabaseView>(name => new SqliteRelationalDatabaseView(Connection, this, name));
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var resolvedViewName = await GetResolvedViewNameAsync(qualifiedViewName, cancellationToken).ConfigureAwait(false);

                var view = resolvedViewName.Map<IRelationalDatabaseView>(name => new SqliteRelationalDatabaseView(Connection, this, name));
                if (view.IsSome)
                    return view;
            }

            return Option<IRelationalDatabaseView>.None;
        }

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _missingSequence;
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences { get; } = Array.Empty<IDatabaseSequence>();

        public Task<Option<IDatabaseSequence>> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _missingSequenceTask;
        }

        public Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySequencesTask;

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _missingSynonym;
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; } = Array.Empty<IDatabaseSynonym>();

        public Task<Option<IDatabaseSynonym>> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _missingSynonymTask;
        }

        public Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySynonymsTask;

        public void AttachDatabase(string schemaName, string fileName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var sql = AttachDatabaseQuery(schemaName, fileName);
            Connection.Execute(sql);
        }

        public Task AttachDatabaseAsync(string schemaName, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var sql = AttachDatabaseQuery(schemaName, fileName);
            return Connection.ExecuteAsync(sql);
        }

        protected virtual string AttachDatabaseQuery(string schemaName, string fileName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var quotedSchemaName = Dialect.QuoteIdentifier(schemaName);
            var escapedFileName = fileName.Replace("'", "''");

            return $"ATTACH DATABASE '{ escapedFileName }' AS { quotedSchemaName }";
        }

        public void DetachDatabase(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = DetachDatabaseQuery(schemaName);
            Connection.Execute(sql);
        }

        public Task DetachDatabaseAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = DetachDatabaseQuery(schemaName);
            return Connection.ExecuteAsync(sql);
        }

        protected virtual string DetachDatabaseQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return "DETACH DATABASE " + Dialect.QuoteIdentifier(schemaName);
        }

        public void Vacuum()
        {
            const string sql = "vacuum";
            Connection.Execute(sql);
        }

        public Task VacuumAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "vacuum";
            return Connection.ExecuteAsync(sql);
        }

        public void Vacuum(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = VacuumQuery(schemaName);
            Connection.Execute(sql);
        }

        public Task VacuumAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = VacuumQuery(schemaName);
            return Connection.ExecuteAsync(sql);
        }

        protected virtual string VacuumQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return "vacuum " + Dialect.QuoteIdentifier(schemaName);
        }

        protected static bool IsReservedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return tableName.LocalName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> LoadDatabaseVersion()
        {
            var version = await Connection.ExecuteScalarAsync<string>("select sqlite_version()").ConfigureAwait(false);
            return "SQLite " + version;
        }

        private readonly AsyncLazy<string> _versionLoader;

        private readonly static Option<IDatabaseSequence> _missingSequence = Option<IDatabaseSequence>.None;
        private readonly static Task<Option<IDatabaseSequence>> _missingSequenceTask = Task.FromResult(_missingSequence);
        private readonly static Task<IReadOnlyCollection<Task<IDatabaseSequence>>> _emptySequencesTask = Task.FromResult<IReadOnlyCollection<Task<IDatabaseSequence>>>(Array.Empty<Task<IDatabaseSequence>>());
        private readonly static Option<IDatabaseSynonym> _missingSynonym = Option<IDatabaseSynonym>.None;
        private readonly static Task<Option<IDatabaseSynonym>> _missingSynonymTask = Task.FromResult(_missingSynonym);
        private readonly static Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> _emptySynonymsTask = Task.FromResult<IReadOnlyCollection<Task<IDatabaseSynonym>>>(Array.Empty<Task<IDatabaseSynonym>>());
    }
}
