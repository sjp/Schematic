using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Query;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public SqliteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
            Metadata = new DatabaseMetadata { DatabaseName = connection.Database };
        }

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata { get; }

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            const string sql = "select count(*) from sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
            return Connection.ExecuteScalar<int>(
                sql,
                new { TableName = tableName.LocalName }
            ) != 0;
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            const string sql = "select count(*) from sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { TableName = tableName.LocalName }
            ) != 0;
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return null;

            return LoadTableSync(tableName.LocalName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return null;

            return LoadTableAsync(tableName.LocalName);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'table' order by name";
                var tableNames = Connection.Query<string>(sql)
                    .Where(name => !BuiltInTables.Contains(name))
                    .Select(name => new LocalIdentifier(name));

                foreach (var tableName in tableNames)
                    yield return LoadTableSync(tableName);
            }
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync()
        {
            return Observable.Create<IRelationalDatabaseTable>(async observer =>
            {
                const string sql = "select name from sqlite_master where type = 'table' order by name";
                var queryResults = await Connection.QueryAsync<string>(sql);

                var tableNames = queryResults
                    .Where(name => !BuiltInTables.Contains(name))
                    .Select(name => new LocalIdentifier(name));

                foreach (var tableName in tableNames)
                {
                    var table = await LoadTableAsync(tableName);
                    observer.OnNext(table);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TableExists(tableName.LocalName)
                ? new SqliteRelationalDatabaseTable(Connection, this, tableName.LocalName)
                : null;
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            var exists = await TableExistsAsync(tableName.LocalName);
            return exists
                ? new SqliteRelationalDatabaseTable(Connection, this, tableName.LocalName)
                : null;
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            const string sql = "select count(*) from sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
            return Connection.ExecuteScalar<int>(
                sql,
                new { ViewName = viewName.LocalName }
            ) != 0;
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            const string sql = "select count(*) from sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { ViewName = viewName.LocalName }
            ) != 0;
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewSync(viewName.LocalName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsync(viewName.LocalName);
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'view' order by name";
                var viewNames = Connection.Query<string>(sql).Select(name => new LocalIdentifier(name));

                foreach (var viewName in viewNames)
                    yield return LoadViewSync(viewName);
            }
        }

        public IObservable<IRelationalDatabaseView> ViewsAsync()
        {
            return Observable.Create<IRelationalDatabaseView>(async observer =>
            {
                const string sql = "select name from sqlite_master where type = 'view' order by name";
                var queryResult = await Connection.QueryAsync<string>(sql);
                var viewNames = queryResult.Select(name => new LocalIdentifier(name));

                foreach (var viewName in viewNames)
                {
                    var view = await LoadViewAsync(viewName);
                    observer.OnNext(view);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            var exists = ViewExists(viewName.LocalName);
            return null;
            // TODO:
            //return exists
            //    ? new SQLiteRelationalDatabaseView(Connection, this, viewName)
            //    : null;
        }

        protected virtual async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            var exists = await ViewExistsAsync(viewName);
            return null;
            // TODO:
            //return exists
            //    ? new SQLiteRelationalDatabaseView(Connection, this, viewName)
            //    : null;
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName) => throw new NotSupportedException();

        public IDatabaseSequence GetSequence(Identifier sequenceName) => throw new NotSupportedException();

        public IEnumerable<IDatabaseSequence> Sequences => throw new NotSupportedException();

        public Task<bool> SequenceExistsAsync(Identifier sequenceName) => throw new NotSupportedException();

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName) => throw new NotSupportedException();

        public IObservable<IDatabaseSequence> SequencesAsync() => throw new NotSupportedException();

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName) => throw new NotSupportedException();

        public IDatabaseSynonym GetSynonym(Identifier synonymName) => throw new NotSupportedException();

        public IEnumerable<IDatabaseSynonym> Synonyms => throw new NotSupportedException();

        public Task<bool> SynonymExistsAsync(Identifier synonymName) => throw new NotSupportedException();

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName) => throw new NotSupportedException();

        public IObservable<IDatabaseSynonym> SynonymsAsync() => throw new NotSupportedException();

        #endregion Synonyms

        #region Triggers

        // TODO: check that triggers are scoped to database or to tables
        //       in oracle they are globally scoped...
        //       In SQL Server they are probably globally scoped given that the name is not relative to a table
        //  THESE QUERIES NEED TO BE SCOPED SO THAT THEY ARE JUST DB SCOPED
        public bool TriggerExists(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            return Connection.ExecuteScalar<int>(
                "select count(*) from sqlite_master where type = 'trigger' and lower(name) = lower(@TriggerName)",
                new { TriggerName = triggerName.LocalName }
            ) != 0;
        }

        public async Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            const string sql = "select count(*) from sqlite_master where type = 'trigger' and lower(name) = lower(@TriggerName)";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { TriggerName = triggerName.LocalName }
            ) != 0;
        }

        public IDatabaseTrigger GetTrigger(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            return LoadTriggerSync(triggerName.LocalName);
        }

        public Task<IDatabaseTrigger> GetTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            return LoadTriggerAsync(triggerName.LocalName);
        }

        public IEnumerable<IDatabaseTrigger> Triggers
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'trigger' order by name";
                var triggers = Connection.Query<string>(sql).Select(name => new LocalIdentifier(name));

                foreach (var trigger in triggers)
                    yield return LoadTriggerSync(trigger);
            }
        }

        public IObservable<IDatabaseTrigger> TriggersAsync()
        {
            return Observable.Create<IDatabaseTrigger>(async observer =>
            {
                const string sql = "select name from sqlite_master where type = 'trigger' order by name";
                var queryResult = await Connection.QueryAsync<string>(sql);
                var triggerNames = queryResult.Select(name => new LocalIdentifier(name));

                foreach (var triggerName in triggerNames)
                {
                    var trigger = await LoadTriggerAsync(triggerName);
                    observer.OnNext(trigger);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IDatabaseTrigger LoadTriggerSync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            const string sql = "select sql from sqlite_master where type = 'trigger' and lower(name) = lower(@TriggerName)";
            var queryResult = Connection.QuerySingle<string>(sql, new { TriggerName = triggerName.LocalName });
            if (queryResult == null)
                return null;

            var tokenizer = new SqliteTokenizer();
            var tokens = tokenizer.Tokenize(queryResult);
            var triggerParser = new SqliteTriggerParser(tokens);

            return new SqliteDatabaseTrigger(null, triggerName.LocalName, queryResult, triggerParser.Timing, triggerParser.Event);
        }

        protected virtual async Task<IDatabaseTrigger> LoadTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            const string sql = "select sql from sqlite_master where type = 'trigger' and lower(name) = lower(@TriggerName)";
            var queryResult = await Connection.QuerySingleAsync<string>(sql, new { TriggerName = triggerName.LocalName });
            if (queryResult == null)
                return null;

            var tokenizer = new SqliteTokenizer();
            var tokens = tokenizer.Tokenize(queryResult);
            var triggerParser = new SqliteTriggerParser(tokens);

            return new SqliteDatabaseTrigger(null, triggerName.LocalName, queryResult, triggerParser.Timing, triggerParser.Event);
        }

        #endregion Triggers

        protected static ISet<string> BuiltInTables { get; } = new HashSet<string>(new[] { "sqlite_master", "sqlite_sequence" }, StringComparer.OrdinalIgnoreCase);
    }
}
