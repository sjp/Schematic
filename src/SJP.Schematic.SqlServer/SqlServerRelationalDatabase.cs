using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabase : RelationalDatabase, IRelationalDatabase, IDependentRelationalDatabase
    {
        public SqlServerRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IEqualityComparer<Identifier> comparer = null)
            : base(dialect, connection)
        {
            Comparer = comparer ?? IdentifierComparer.OrdinalIgnoreCase;
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);
            _parentDb = this;
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(Parent));
        }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected IRelationalDatabase Database => Parent;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata => _metadata.Value;

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            const string sql = "select count(*) from sys.tables where schema_id = schema_id(@SchemaName) and name = @TableName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ) != 0;
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            const string sql = "select count(*) from sys.tables where schema_id = schema_id(@SchemaName) and name = @TableName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableSync(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableAsync(tableName);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";
                var tableNames = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var tableName in tableNames)
                    yield return LoadTableSync(tableName);
            }
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync()
        {
            return Observable.Create<IRelationalDatabaseTable>(async observer =>
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";
                var queryResults = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
                var tableNames = queryResults.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var tableName in tableNames)
                {
                    var table = await LoadTableAsync(tableName).ConfigureAwait(false);
                    observer.OnNext(table);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return TableExists(tableName)
                ? new SqlServerRelationalDatabaseTable(Connection, Database, tableName, Comparer)
                : null;
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var exists = await TableExistsAsync(tableName).ConfigureAwait(false);
            return exists
                ? new SqlServerRelationalDatabaseTable(Connection, Database, tableName, Comparer)
                : null;
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return Connection.ExecuteScalar<int>(
                "select count(*) from sys.views where schema_id = schema_id(@SchemaName) and name = @ViewName",
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ) != 0;
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return await Connection.ExecuteScalarAsync<int>(
                "select count(*) from sys.views where schema_id = schema_id(@SchemaName) and name = @ViewName",
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewSync(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewAsync(viewName);
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";
                var viewNames = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var viewName in viewNames)
                    yield return LoadViewSync(viewName);
            }
        }

        public IObservable<IRelationalDatabaseView> ViewsAsync()
        {
            return Observable.Create<IRelationalDatabaseView>(async observer =>
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";
                var queryResult = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
                var viewNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var viewName in viewNames)
                {
                    var view = await LoadViewAsync(viewName).ConfigureAwait(false);
                    observer.OnNext(view);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return ViewExists(viewName)
                ? new SqlServerRelationalDatabaseView(Connection, Database, viewName, Comparer)
                : null;
        }

        protected virtual async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var exists = await ViewExistsAsync(viewName).ConfigureAwait(false);
            return exists
                ? new SqlServerRelationalDatabaseView(Connection, Database, viewName, Comparer)
                : null;
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            const string sql = "select count(*) from sys.sequences where schema_id = schema_id(@SchemaName) and name = @SequenceName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ) != 0;
        }

        public async Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            const string sql = "select count(*) from sys.sequences where schema_id = schema_id(@SchemaName) and name = @SequenceName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceSync(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceAsync(sequenceName);
        }

        public IEnumerable<IDatabaseSequence> Sequences
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name";
                var sequenceNames = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var sequenceName in sequenceNames)
                    yield return LoadSequenceSync(sequenceName);
            }
        }

        public IObservable<IDatabaseSequence> SequencesAsync()
        {
            return Observable.Create<IDatabaseSequence>(async observer =>
            {
                var queryResult = await Connection.QueryAsync<QualifiedName>(
                "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name").ConfigureAwait(false);
                var sequenceNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var sequenceName in sequenceNames)
                {
                    var sequence = await LoadSequenceAsync(sequenceName).ConfigureAwait(false);
                    observer.OnNext(sequence);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return SequenceExists(sequenceName)
                ? new SqlServerDatabaseSequence(Database, Connection, sequenceName)
                : null;
        }

        protected virtual async Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var exists = await SequenceExistsAsync(sequenceName).ConfigureAwait(false);
            return exists
                ? new SqlServerDatabaseSequence(Database, Connection, sequenceName)
                : null;
        }

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            const string sql = "select count(*) from sys.synonyms where schema_id = schema_id(@SchemaName) and name = @SynonymName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ) != 0;
        }

        public async Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            const string sql = "select count(*) from sys.synonyms where schema_id = schema_id(@SchemaName) and name = @SynonymName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymSync(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymAsync(synonymName);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";
                var synonyms = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var synonym in synonyms)
                    yield return LoadSynonymSync(synonym);
            }
        }

        public IObservable<IDatabaseSynonym> SynonymsAsync()
        {
            return Observable.Create<IDatabaseSynonym>(async observer =>
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";
                var queryResult = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
                var synonymNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var synonymName in synonymNames)
                {
                    var synonym = await LoadSynonymAsync(synonymName).ConfigureAwait(false);
                    observer.OnNext(synonym);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            if (!SynonymExists(synonymName))
                return null;

            var queryResult = Connection.QuerySingle<SynonymData>(@"
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
            else if (!queryResult.TargetDatabaseName.IsNullOrWhiteSpace())
                targetName = new Identifier(queryResult.TargetDatabaseName, queryResult.TargetSchemaName, queryResult.TargetObjectName);
            else if (!queryResult.TargetSchemaName.IsNullOrWhiteSpace())
                targetName = new Identifier(queryResult.TargetSchemaName, queryResult.TargetObjectName);
            else
                targetName = new Identifier(queryResult.TargetObjectName);

            return new SqlServerDatabaseSynonym(Database, synonymName, targetName);
        }

        protected virtual async Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var exists = await SynonymExistsAsync(synonymName).ConfigureAwait(false);
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
    ", new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }).ConfigureAwait(false);

            Identifier targetName;
            if (!queryResult.TargetServerName.IsNullOrWhiteSpace())
                targetName = new Identifier(queryResult.TargetServerName, queryResult.TargetDatabaseName, queryResult.TargetSchemaName, queryResult.TargetObjectName);
            else if (!queryResult.TargetDatabaseName.IsNullOrWhiteSpace())
                targetName = new Identifier(queryResult.TargetDatabaseName, queryResult.TargetSchemaName, queryResult.TargetObjectName);
            else if (!queryResult.TargetSchemaName.IsNullOrWhiteSpace())
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
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);

            const string sql = "select count(*) from sys.triggers where schema_id = schema_id(@SchemaName) and name = @TriggerName";
            return Connection.ExecuteScalar<int>(
                sql,
                new { SchemaName = triggerName.Schema, TriggerName = triggerName.LocalName }
            ) != 0;
        }

        public async Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);

            const string sql = "select count(*) from sys.triggers where schema_id = schema_id(@SchemaName) and name = @TriggerName";
            return await Connection.ExecuteScalarAsync<int>(
                sql,
                new { SchemaName = triggerName.Schema, TriggerName = triggerName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        public IDatabaseTrigger GetTrigger(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);
            return LoadTriggerSync(triggerName);
        }

        public Task<IDatabaseTrigger> GetTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);
            return LoadTriggerAsync(triggerName);
        }

        public IEnumerable<IDatabaseTrigger> Triggers
        {
            get
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.triggers order by schema_name(schema_id), name";
                var triggers = Connection.Query<QualifiedName>(sql)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var trigger in triggers)
                    yield return LoadTriggerSync(trigger);
            }
        }

        public IObservable<IDatabaseTrigger> TriggersAsync()
        {
            return Observable.Create<IDatabaseTrigger>(async observer =>
            {
                const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.triggers order by schema_name(schema_id), name";
                var queryResult = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
                var triggerNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var triggerName in triggerNames)
                {
                    var trigger = await LoadTriggerAsync(triggerName).ConfigureAwait(false);
                    observer.OnNext(trigger);
                }

                observer.OnCompleted();
            });
        }

        protected virtual IDatabaseTrigger LoadTriggerSync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);
            const string sql = @"
select st.name as TriggerName, sm.definition as Definition, st.is_instead_of_trigger as IsInsteadOfTrigger, te.type_desc as TriggerEvent, st.is_disabled as IsDisabled
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @SchemaName and st.name = @TriggerName";

            var queryResult = Connection.Query<TriggerData>(sql, new { SchemaName = triggerName.Schema, Trigger = triggerName.LocalName });
            if (queryResult.Empty())
                return null;

            var triggerResult = queryResult
                .GroupBy(row => new
                {
                    TriggerName = row.TriggerName,
                    Definition = row.Definition,
                    IsInsteadOfTrigger = row.IsInsteadOfTrigger,
                    IsDisabled = row.IsDisabled
                })
                .Single();

            var queryTiming = triggerResult.Key.IsInsteadOfTrigger ? TriggerQueryTiming.Before : TriggerQueryTiming.After;
            var definition = triggerResult.Key.Definition;
            var isEnabled = !triggerResult.Key.IsDisabled;

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
            return new SqlServerDatabaseTrigger(null, triggerName, definition, queryTiming, events, isEnabled);
        }

        protected virtual async Task<IDatabaseTrigger> LoadTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);
            const string sql = @"
select st.name as TriggerName, sm.definition as Definition, st.is_instead_of_trigger as IsInsteadOfTrigger, te.type_desc as TriggerEvent, st.is_disabled as IsDisabled
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @SchemaName and st.name = @TriggerName";

            var queryResult = await Connection.QueryAsync<TriggerData>(sql, new { SchemaName = triggerName.Schema, Trigger = triggerName.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return null;

            var triggerResult = queryResult
                .GroupBy(row => new
                {
                    TriggerName = row.TriggerName,
                    Definition = row.Definition,
                    IsInsteadOfTrigger = row.IsInsteadOfTrigger,
                    IsDisabled = row.IsDisabled
                })
                .Single();

            var queryTiming = triggerResult.Key.IsInsteadOfTrigger ? TriggerQueryTiming.Before : TriggerQueryTiming.After;
            var definition = triggerResult.Key.Definition;
            var isEnabled = !triggerResult.Key.IsDisabled;

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
            return new SqlServerDatabaseTrigger(null, triggerName, definition, queryTiming, events, isEnabled);
        }

        #endregion Triggers

        private DatabaseMetadata LoadDatabaseMetadata()
        {
            const string sql = "select db_name() as DatabaseName, schema_name() as DefaultSchema";
            return Connection.QuerySingle<DatabaseMetadata>(sql);
        }

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null || identifier.LocalName == null)
                throw new ArgumentNullException(nameof(identifier));

            return identifier.Schema == null && DefaultSchema != null
                ? new Identifier(DefaultSchema, identifier.LocalName)
                : identifier;
        }

        private IRelationalDatabase _parentDb;
        private readonly Lazy<DatabaseMetadata> _metadata;
    }
}
