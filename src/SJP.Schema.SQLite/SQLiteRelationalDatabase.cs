using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Dapper;
using SJP.Schema.Core.Utilities;
using SJP.Schema.SQLite.Query;
using SJP.Schema.Core;
using SJP.Schema.SQLite.Parsing;

namespace SJP.Schema.SQLite
{
    public class SQLiteRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public SQLiteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
            _tableCache = new AsyncCache<Identifier, IRelationalDatabaseTable>(LoadTableAsync);
            _viewCache = new AsyncCache<Identifier, IRelationalDatabaseView>(LoadViewAsync);
            _triggerCache = new AsyncCache<Identifier, IDatabaseTrigger>(LoadTriggerAsync);

            _tableLookup = new LazyDictionaryCache<Identifier, IRelationalDatabaseTable>(tableName => TableAsync(tableName).Result);
            _viewLookup = new LazyDictionaryCache<Identifier, IRelationalDatabaseView>(viewName => ViewAsync(viewName).Result);
            _triggerLookup = new LazyDictionaryCache<Identifier, IDatabaseTrigger>(triggerName => TriggerAsync(triggerName).Result);

            Metadata = new DatabaseMetadata { DatabaseName = connection.Database };
        }

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata { get; }

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            const string sql = "select count(*) from sqlite_master where type = 'table' and name = @TableName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { TableName = tableName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table => _tableLookup;

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'table' order by name";
                var tableNames = Connection.Query<string>(sql)
                    .Where(name => !BuiltInTables.Contains(name))
                    .Select(name => new LocalIdentifier(name));

                foreach (var tableName in tableNames)
                    yield return _tableCache.GetValue(tableName).Result;
            }
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            const string sql = "select count(*) from sqlite_master where type = 'table' and name = @TableName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { TableName = tableName.LocalName }
            ) != 0;
        }

        public Task<IRelationalDatabaseTable> TableAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableCache.GetValue(tableName);
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync()
        {
            return Observable.Create<IRelationalDatabaseTable>(async observer =>
            {
                const string sql = "select name from sqlite_master where type = 'table' order by name";
                var queryResults = await Connection.QueryAsync<string>(sql);

                var tableNames = queryResults
                    .Where(name => !BuiltInTables.Contains(name))
                    .Select(name => new Identifier(name));

                foreach (var tableName in tableNames)
                {
                    var table = await _tableCache.GetValue(tableName);
                    observer.OnNext(table);
                }

                observer.OnCompleted();
            });
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            var exists = await TableExistsAsync(tableName);
            return exists
                ? new SQLiteRelationalDatabaseTable(Connection, this, tableName)
                : null;
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            const string sql = "select count(*) from sqlite_master where type = 'view' and name = @ViewName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { ViewName = viewName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View => _viewLookup;

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'view' order by name";
                var viewNames = Connection.Query<string>(sql).Select(name => new LocalIdentifier(name));

                foreach (var viewName in viewNames)
                    yield return _viewCache.GetValue(viewName).Result;
            }
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            const string sql = "select count(*) from sqlite_master where type = 'view' and name = @ViewName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { ViewName = viewName.LocalName }
            ) != 0;
        }

        public Task<IRelationalDatabaseView> ViewAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewCache.GetValue(viewName);
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
                    var view = await _viewCache.GetValue(viewName);
                    observer.OnNext(view);
                }

                observer.OnCompleted();
            });
        }

        protected virtual async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
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

        public IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence => throw new NotSupportedException();

        public IEnumerable<IDatabaseSequence> Sequences => throw new NotSupportedException();

        public Task<bool> SequenceExistsAsync(Identifier sequenceName) => throw new NotSupportedException();

        public Task<IDatabaseSequence> SequenceAsync(Identifier sequenceName) => throw new NotSupportedException();

        public IObservable<IDatabaseSequence> SequencesAsync() => throw new NotSupportedException();

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName) => throw new NotSupportedException();

        public IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym => throw new NotSupportedException();

        public IEnumerable<IDatabaseSynonym> Synonyms => throw new NotSupportedException();

        public Task<bool> SynonymExistsAsync(Identifier synonymName) => throw new NotSupportedException();

        public Task<IDatabaseSynonym> SynonymAsync(Identifier synonymName) => throw new NotSupportedException();

        public IObservable<IDatabaseSynonym> SynonymsAsync() => throw new NotSupportedException();

        #endregion Synonyms

        #region Triggers

        // TODO: check that triggers are scoped to database or to tables
        //       in oracle they are globally scoped...
        //       In SQL Server they are probably globally scoped given that the name is not relative to a table
        //  THESE QUERIES NEED TO BE SCOPED SO THAT THEY ARE JUST DB SCOPED
        public bool TriggerExists(Identifier triggerName)
        {
            return Connection.ExecuteScalar<int>(
                "select count(*) from sqlite_master where type = 'trigger' and name = @TriggerName",
                new { TriggerName = triggerName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => _triggerLookup;

        public IEnumerable<IDatabaseTrigger> Triggers
        {
            get
            {
                const string sql = "select name from sqlite_master where type = 'trigger' order by name";
                var triggers = Connection.Query<string>(sql).Select(name => new LocalIdentifier(name));

                foreach (var trigger in triggers)
                    yield return _triggerCache.GetValue(trigger).Result;
            }
        }

        public async Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            const string sql = "select count(*) from sqlite_master where type = 'trigger' and name = @TriggerName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { TriggerName = triggerName.LocalName }
            ) != 0;
        }

        public Task<IDatabaseTrigger> TriggerAsync(Identifier triggerName) => _triggerCache.GetValue(triggerName);

        public IObservable<IDatabaseTrigger> TriggersAsync()
        {
            return Observable.Create<IDatabaseTrigger>(async observer =>
            {
                const string sql = "select name from sqlite_master where type = 'trigger' order by name";
                var queryResult = await Connection.QueryAsync<string>(sql);
                var triggerNames = queryResult.Select(name => new LocalIdentifier(name));

                foreach (var triggerName in triggerNames)
                {
                    var trigger = await _triggerCache.GetValue(triggerName);
                    observer.OnNext(trigger);
                }

                observer.OnCompleted();
            });
        }

        // TODO: need to parse this from create trigger statement in sqlite_master
        protected virtual async Task<IDatabaseTrigger> LoadTriggerAsync(Identifier triggerName)
        {
            const string sql = @"select sql from sqlite_master where type = 'trigger' and name = @TriggerName";
            var queryResult = await Connection.QuerySingleAsync<string>(sql, new { TriggerName = triggerName.LocalName });
            if (queryResult == null)
                return null;

            var tokenizer = new SQLiteTokenizer();
            var tokens = tokenizer.Tokenize(queryResult);
            var triggerParser = new SQLiteTriggerParser(tokens);

            return new SQLiteDatabaseTrigger(null, triggerName.LocalName, queryResult, triggerParser.Timing, triggerParser.Event);
        }

        #endregion Triggers

        protected static ISet<string> BuiltInTables { get; } = new HashSet<string>(new[] { "sqlite_master", "sqlite_sequence" }, StringComparer.OrdinalIgnoreCase);

        private readonly AsyncCache<Identifier, IRelationalDatabaseTable> _tableCache;
        private readonly AsyncCache<Identifier, IRelationalDatabaseView> _viewCache;
        private readonly AsyncCache<Identifier, IDatabaseTrigger> _triggerCache;

        private readonly IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> _tableLookup;
        private readonly IReadOnlyDictionary<Identifier, IRelationalDatabaseView> _viewLookup;
        private readonly IReadOnlyDictionary<Identifier, IDatabaseTrigger> _triggerLookup;
    }
}
