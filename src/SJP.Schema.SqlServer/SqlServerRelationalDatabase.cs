using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SJP.Schema.Core;
using SJP.Schema.Core.Utilities;
using SJP.Schema.SqlServer.Query;

namespace SJP.Schema.SqlServer
{
    public class SqlServerRelationalDatabase : RelationalDatabase, IRelationalDatabase, IDependentRelationalDatabase
    {
        public SqlServerRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
            _tableCache = new AsyncCache<Identifier, IRelationalDatabaseTable>(LoadTableAsync);
            _viewCache = new AsyncCache<Identifier, IRelationalDatabaseView>(LoadViewAsync);
            _sequenceCache = new AsyncCache<Identifier, IDatabaseSequence>(LoadSequenceAsync);
            _synonymCache = new AsyncCache<Identifier, IDatabaseSynonym>(LoadSynonymAsync);
            _triggerCache = new AsyncCache<Identifier, IDatabaseTrigger>(LoadTriggerAsync);

            Table = new LazyDictionaryCache<Identifier, IRelationalDatabaseTable>(tableName => TableAsync(tableName).Result);
            View = new LazyDictionaryCache<Identifier, IRelationalDatabaseView>(viewName => ViewAsync(viewName).Result);
            Sequence = new LazyDictionaryCache<Identifier, IDatabaseSequence>(sequenceName => SequenceAsync(sequenceName).Result);
            Synonym = new LazyDictionaryCache<Identifier, IDatabaseSynonym>(synonymName => SynonymAsync(synonymName).Result);
            Trigger = new LazyDictionaryCache<Identifier, IDatabaseTrigger>(triggerName => TriggerAsync(triggerName).Result);

            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);
            _parentDb = this;
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(Parent));
        }

        protected IRelationalDatabase Database => Parent;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata => _metadata.Value;

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Schema.IsNullOrWhiteSpace())
                tableName = new Identifier(DefaultSchema, tableName.LocalName);

            const string sql = "select count(*) from sys.tables where schema_id = schema_id(@SchemaName) and name = @TableName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table { get; }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";
                var tableNames = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var tableName in tableNames)
                    yield return _tableCache.GetValue(tableName).Result;
            }
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Schema.IsNullOrWhiteSpace())
                tableName = new Identifier(DefaultSchema, tableName.LocalName);

            const string sql = "select count(*) from sys.tables where schema_id = schema_id(@SchemaName) and name = @TableName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
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
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";
                var queryResults = await Connection.QueryAsync<QualifiedName>(sql);
                var tableNames = queryResults.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

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
            if (tableName.Schema.IsNullOrWhiteSpace())
                tableName = new Identifier(DefaultSchema, tableName.LocalName);

            var exists = await TableExistsAsync(tableName);
            return exists
                ? new SqlServerRelationalDatabaseTable(Connection, Database, tableName)
                : null;
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema.IsNullOrWhiteSpace())
                viewName = new Identifier(DefaultSchema, viewName.LocalName);

            return Connection.ExecuteScalar<int>(
                "select count(*) from sys.views where schema_id = schema_id(@SchemaName) and name = @ViewName",
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View { get; }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";
                var viewNames = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var viewName in viewNames)
                    yield return _viewCache.GetValue(viewName).Result;
            }
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema.IsNullOrWhiteSpace())
                viewName = new Identifier(DefaultSchema, viewName.LocalName);

            return await Connection.ExecuteScalarAsync<int>(
                "select count(*) from sys.views where schema_id = schema_id(@SchemaName) and name = @ViewName",
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
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
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";
                var queryResult = await Connection.QueryAsync<QualifiedName>(sql);
                var viewNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

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
            if (viewName.Schema.IsNullOrWhiteSpace())
                viewName = new Identifier(DefaultSchema, viewName.LocalName);

            var exists = await ViewExistsAsync(viewName);
            return exists
                ? new SqlServerRelationalDatabaseView(Connection, Database, viewName)
                : null;
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            if (sequenceName.Schema.IsNullOrWhiteSpace())
                sequenceName = new Identifier(DefaultSchema, sequenceName.LocalName);

            const string sql = "select count(*) from sys.sequences where schema_id = schema_id(@SchemaName) and name = @SequenceName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence { get; }

        public IEnumerable<IDatabaseSequence> Sequences
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name";
                var sequenceNames = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var sequenceName in sequenceNames)
                    yield return _sequenceCache.GetValue(sequenceName).Result;
            }
        }

        public async Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            if (sequenceName.Schema.IsNullOrWhiteSpace())
                sequenceName = new Identifier(DefaultSchema, sequenceName.LocalName);

            const string sql = "select count(*) from sys.sequences where schema_id = schema_id(@SchemaName) and name = @SequenceName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ) != 0;
        }

        public Task<IDatabaseSequence> SequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceCache.GetValue(sequenceName);
        }

        public IObservable<IDatabaseSequence> SequencesAsync()
        {
            return Observable.Create<IDatabaseSequence>(async observer =>
            {
                var queryResult = await Connection.QueryAsync<QualifiedName>(
                "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name");
                var sequenceNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var sequenceName in sequenceNames)
                {
                    var sequence = await _sequenceCache.GetValue(sequenceName);
                    observer.OnNext(sequence);
                }

                observer.OnCompleted();
            });
        }

        protected virtual async Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            if (sequenceName.Schema.IsNullOrWhiteSpace())
                sequenceName = new Identifier(DefaultSchema, sequenceName.LocalName);

            var exists = await SequenceExistsAsync(sequenceName);
            return exists
                ? new SqlServerDatabaseSequence(Database, Connection, sequenceName)
                : null;
        }

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            if (synonymName.Schema.IsNullOrWhiteSpace())
                synonymName = new Identifier(DefaultSchema, synonymName.LocalName);

            const string sql = "select count(*) from sys.synonyms where schema_id = schema_id(@SchemaName) and name = @SynonymName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym { get; }

        public IEnumerable<IDatabaseSynonym> Synonyms
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";
                var synonyms = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var synonym in synonyms)
                    yield return _synonymCache.GetValue(synonym).Result;
            }
        }

        public async Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            if (synonymName.Schema.IsNullOrWhiteSpace())
                synonymName = new Identifier(DefaultSchema, synonymName.LocalName);

            const string sql = "select count(*) from sys.synonyms where schema_id = schema_id(@SchemaName) and name = @SynonymName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ) != 0;
        }

        public Task<IDatabaseSynonym> SynonymAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymCache.GetValue(synonymName);
        }

        public IObservable<IDatabaseSynonym> SynonymsAsync()
        {
            return Observable.Create<IDatabaseSynonym>(async observer =>
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";
                var queryResult = await Connection.QueryAsync<QualifiedName>(sql);
                var synonymNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var synonymName in synonymNames)
                {
                    var synonym = await _synonymCache.GetValue(synonymName);
                    observer.OnNext(synonym);
                }

                observer.OnCompleted();
            });
        }

        protected virtual async Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName.Schema.IsNullOrWhiteSpace())
                synonymName = new Identifier(DefaultSchema, synonymName.LocalName);

            var exists = await SynonymExistsAsync(synonymName);
            if (!exists)
                return null;

            var queryResult = await Connection.QuerySingleAsync<SynonymData>(@"
select
    PARSENAME(base_object_name, 4) as TargetServerName,
    PARSENAME(base_object_name, 3) as TargetDatabaseName,
    PARSENAME(base_object_name, 2) as TargetSchemaName,
    PARSENAME(base_object_name, 1) as TargetObjectName
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName
    ", new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName });

            Identifier targetName;
            if (!queryResult.TargetServerName.IsNullOrWhiteSpace())
                targetName = new Identifier(queryResult.TargetServerName, queryResult.TargetDatabaseName, queryResult.TargetSchemaName, queryResult.TargetObjectName);
            else if (queryResult.TargetDatabaseName.IsNullOrWhiteSpace())
                targetName = new Identifier(queryResult.TargetDatabaseName, queryResult.TargetSchemaName, queryResult.TargetObjectName);
            else if (queryResult.TargetSchemaName.IsNullOrWhiteSpace())
                targetName = new Identifier(queryResult.TargetSchemaName, queryResult.TargetObjectName);
            else
                targetName = new Identifier(queryResult.TargetObjectName);

            return new SqlServerDatabaseSynonym(Database, synonymName, targetName);
        }

        #endregion Synonyms

        #region Triggers

        // TODO: check that triggers are scoped to database or to tables
        //       in oracle they are globally scoped...
        //       In SQL Server they are probably globally scoped given that the name is not relative to a table
        //  THESE QUERIES NEED TO BE SCOPED SO THAT THEY ARE JUST DB SCOPED
        public bool TriggerExists(Identifier triggerName)
        {
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));

            if (triggerName.Schema.IsNullOrWhiteSpace())
                triggerName = new Identifier(DefaultSchema, triggerName.LocalName);

            const string sql = "select count(*) from sys.triggers where schema_id = schema_id(@SchemaName) and name = @TriggerName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = triggerName.Schema, TriggerName = triggerName.LocalName }
            ) != 0;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        public IEnumerable<IDatabaseTrigger> Triggers
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.triggers order by schema_name(schema_id), name";
                var triggers = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var trigger in triggers)
                    yield return _triggerCache.GetValue(trigger).Result;
            }
        }

        public async Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));

            if (triggerName.Schema.IsNullOrWhiteSpace())
                triggerName = new Identifier(DefaultSchema, triggerName.LocalName);

            const string sql = "select count(*) from sys.triggers where schema_id = schema_id(@SchemaName) and name = @TriggerName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = triggerName.Schema, TriggerName = triggerName.LocalName }
            ) != 0;
        }

        public Task<IDatabaseTrigger> TriggerAsync(Identifier triggerName)
        {
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));

            return _triggerCache.GetValue(triggerName);
        }

        public IObservable<IDatabaseTrigger> TriggersAsync()
        {
            return Observable.Create<IDatabaseTrigger>(async observer =>
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.triggers order by schema_name(schema_id), name";
                var queryResult = await Connection.QueryAsync<QualifiedName>(sql);
                var triggerNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var triggerName in triggerNames)
                {
                    var trigger = await _triggerCache.GetValue(triggerName);
                    observer.OnNext(trigger);
                }

                observer.OnCompleted();
            });
        }

        protected virtual async Task<IDatabaseTrigger> LoadTriggerAsync(Identifier triggerName)
        {
            if (triggerName.Schema.IsNullOrWhiteSpace())
                triggerName = new Identifier(DefaultSchema, triggerName.LocalName);

            const string sql = @"
select st.name as TriggerName, sm.definition as Definition, st.is_instead_of_trigger as IsInsteadOfTrigger, te.type_desc as TriggerEvent
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @SchemaName and st.name = @TriggerName";

            var queryResult = await Connection.QueryAsync<TriggerData>(sql, new { SchemaName = triggerName.Schema, Trigger = triggerName.LocalName });
            if (queryResult.Empty())
                return null;

            var triggerResult = queryResult
                .GroupBy(row => new { TriggerName = row.TriggerName, Definition = row.Definition, IsInsteadOfTrigger = row.IsInsteadOfTrigger })
                .Single();

            var queryTiming = triggerResult.Key.IsInsteadOfTrigger ? TriggerQueryTiming.Before : TriggerQueryTiming.After;
            var definition = triggerResult.Key.Definition;
            var events = TriggerEvent.None;
            foreach (var trigEvent in triggerResult)
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

            // TODO: this will throw an exception because it needs a table
            //       maybe refactor if we want to support DDL triggers, i.e. schema change triggers
            var trigger = new SqlServerDatabaseTrigger(null, triggerName, definition, queryTiming, events);
            return trigger;
        }

        #endregion Triggers

        private DatabaseMetadata LoadDatabaseMetadata()
        {
            const string sql = "select db_name() as DatabaseName, schema_name() as DefaultSchema";
            return Connection.QuerySingle<DatabaseMetadata>(sql);
        }

        private IRelationalDatabase _parentDb;

        private readonly Lazy<DatabaseMetadata> _metadata;

        private readonly AsyncCache<Identifier, IRelationalDatabaseTable> _tableCache;
        private readonly AsyncCache<Identifier, IRelationalDatabaseView> _viewCache;
        private readonly AsyncCache<Identifier, IDatabaseSequence> _sequenceCache;
        private readonly AsyncCache<Identifier, IDatabaseSynonym> _synonymCache;
        private readonly AsyncCache<Identifier, IDatabaseTrigger> _triggerCache;
    }
}
