using System;
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
using Microsoft.Extensions.Logging;

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

        private ILogger<SqliteRelationalDatabase> Logger { get; } = Logging.Factory.CreateLogger<SqliteRelationalDatabase>();

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (IsReservedTableName(tableName))
                return false;

            if (tableName.Schema != null)
            {
                var sql = TableExistsQuery(tableName.Schema);
                return Connection.ExecuteScalar<int>(
                    sql,
                    new { TableName = tableName.LocalName }
                ) > 0;
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = TableExistsQuery(dbName);
                var tableCount = Connection.ExecuteScalar<int>(sql, new { TableName = tableName.LocalName });

                if (tableCount > 0)
                    return true;
            }

            return false;
        }

        public Task<bool> TableExistsAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TableExistsAsyncCore(tableName, cancellationToken);
        }

        private async Task<bool> TableExistsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (IsReservedTableName(tableName))
                return false;

            if (tableName.Schema != null)
            {
                var sql = TableExistsQuery(tableName.Schema);
                return await Connection.ExecuteScalarAsync<int>(
                    sql,
                    new { TableName = tableName.LocalName }
                ).ConfigureAwait(false) > 0;
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = TableExistsQuery(dbName);
                var tableCount = await Connection.ExecuteScalarAsync<int>(sql, new { TableName = tableName.LocalName }).ConfigureAwait(false);

                if (tableCount > 0)
                    return true;
            }

            return false;
        }

        protected virtual string TableExistsQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select count(*) from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (IsReservedTableName(tableName))
                return null;

            if (tableName.Schema != null)
                return LoadTableSync(tableName);

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = LoadTableSync(qualifiedTableName);

                if (table != null)
                    return table;
            }

            return null;
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<IRelationalDatabaseTable> GetTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (IsReservedTableName(tableName))
                return null;

            if (tableName.Schema != null)
                return await LoadTableAsync(tableName, cancellationToken).ConfigureAwait(false);

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = await LoadTableAsync(qualifiedTableName, cancellationToken).ConfigureAwait(false);

                if (table != null)
                    return table;
            }

            return null;
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

                var tables = qualifiedTableNames.Select(LoadTableSync);
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

            var tables = qualifiedTableNames.Select(name => LoadTableAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseTable>>(qualifiedTableNames.Count, tables);
        }

        protected virtual string TablesQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'table' order by name";
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Schema != null)
            {
                return TableExists(tableName)
                    ? new SqliteRelationalDatabaseTable(Connection, this, tableName)
                    : null;
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = TableExists(qualifiedTableName)
                    ? new SqliteRelationalDatabaseTable(Connection, this, qualifiedTableName)
                    : null;

                if (table != null)
                    return table;
            }

            return null;
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName.Schema != null)
            {
                var exists = await TableExistsAsync(tableName, cancellationToken).ConfigureAwait(false);
                return exists
                    ? new SqliteRelationalDatabaseTable(Connection, this, tableName)
                    : null;
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = Identifier.CreateQualifiedIdentifier(dbName, tableName.LocalName);
                var table = await TableExistsAsync(qualifiedTableName, cancellationToken).ConfigureAwait(false)
                    ? new SqliteRelationalDatabaseTable(Connection, this, qualifiedTableName)
                    : null;

                if (table != null)
                    return table;
            }

            return null;
        }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                var sql = ViewExistsQuery(viewName.Schema);
                return Connection.ExecuteScalar<int>(
                    sql,
                    new { ViewName = viewName.LocalName }
                ) > 0;
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = ViewExistsQuery(dbName);
                var viewCount = Connection.ExecuteScalar<int>(sql, new { ViewName = viewName.LocalName });

                if (viewCount > 0)
                    return true;
            }

            return false;
        }

        public Task<bool> ViewExistsAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ViewExistsAsyncCore(viewName, cancellationToken);
        }

        private async Task<bool> ViewExistsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
            {
                var sql = ViewExistsQuery(viewName.Schema);
                return await Connection.ExecuteScalarAsync<int>(
                    sql,
                    new { ViewName = viewName.LocalName }
                ).ConfigureAwait(false) > 0;
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = ViewExistsQuery(dbName);
                var viewCount = await Connection.ExecuteScalarAsync<int>(sql, new { ViewName = viewName.LocalName }).ConfigureAwait(false);

                if (viewCount > 0)
                    return true;
            }

            return false;
        }

        protected virtual string ViewExistsQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select count(*) from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
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

                if (view != null)
                    return view;
            }

            return null;
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<IRelationalDatabaseView> GetViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
                return await LoadViewAsync(viewName, cancellationToken).ConfigureAwait(false);

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var view = await LoadViewAsync(qualifiedViewName, cancellationToken).ConfigureAwait(false);

                if (view != null)
                    return view;
            }

            return null;
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

                var views = qualifiedViewNames.Select(LoadViewSync);
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

            var views = qualifiedViewNames.Select(name => LoadViewAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseView>>(qualifiedViewNames.Count, views);
        }

        protected virtual string ViewsQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' order by name";
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                var exists = ViewExists(viewName);
                return exists
                    ? new SqliteRelationalDatabaseView(Connection, this, viewName)
                    : null;
            }

            var dbNames = Pragma.DatabaseList.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var view = ViewExists(qualifiedViewName)
                    ? new SqliteRelationalDatabaseView(Connection, this, qualifiedViewName)
                    : null;

                if (view != null)
                    return view;
            }

            return null;
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<IRelationalDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
            {
                var exists = await ViewExistsAsync(viewName, cancellationToken).ConfigureAwait(false);
                return exists
                    ? new SqliteRelationalDatabaseView(Connection, this, viewName)
                    : null;
            }

            var dbNamesResult = await Pragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var exists = await ViewExistsAsync(qualifiedViewName, cancellationToken).ConfigureAwait(false);
                var view = exists
                    ? new SqliteRelationalDatabaseView(Connection, this, qualifiedViewName)
                    : null;

                if (view != null)
                    return view;
            }

            return null;
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return false;
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return null;
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences { get; } = Array.Empty<IDatabaseSequence>();

        public Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Task.FromResult(false);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Task.FromResult<IDatabaseSequence>(null);
        }

        public Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySequences;

        private readonly static Task<IReadOnlyCollection<Task<IDatabaseSequence>>> _emptySequences =
            Task.FromResult<IReadOnlyCollection<Task<IDatabaseSequence>>>(Array.Empty<Task<IDatabaseSequence>>());

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return false;
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return null;
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; } = Array.Empty<IDatabaseSynonym>();

        public Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Task.FromResult(false);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Task.FromResult<IDatabaseSynonym>(null);
        }

        public Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySynonyms;

        private readonly static Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> _emptySynonyms =
            Task.FromResult<IReadOnlyCollection<Task<IDatabaseSynonym>>>(Array.Empty<Task<IDatabaseSynonym>>());

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
    }
}
