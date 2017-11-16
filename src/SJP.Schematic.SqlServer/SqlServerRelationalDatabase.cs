using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);
            Comparer = comparer ?? new IdentifierComparer(StringComparer.OrdinalIgnoreCase, ServerName, DatabaseName, DefaultSchema);
            _parentDb = this;
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(Parent));
        }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected IRelationalDatabase Database => Parent;

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata => _metadata.Value;

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

        public async Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync()
        {
            const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";
            var queryResults = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
            var tableNames = queryResults.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return tableNames
                .Select(LoadTableSync)
                .ToAsyncEnumerable();
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

        public async Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync()
        {
            const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";
            var queryResult = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
            var viewNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return viewNames
                .Select(LoadViewSync)
                .ToAsyncEnumerable();
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

        public async Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync()
        {
            const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name";
            var queryResult = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
            var sequenceNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return sequenceNames
                .Select(LoadSequenceSync)
                .ToAsyncEnumerable();
        }

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return SequenceExists(sequenceName)
                ? new SqlServerDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

        protected virtual async Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var exists = await SequenceExistsAsync(sequenceName).ConfigureAwait(false);
            return exists
                ? new SqlServerDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

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

        public async Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync()
        {
            const string sql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";
            var queryResult = await Connection.QueryAsync<QualifiedName>(sql).ConfigureAwait(false);
            var synonymNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return synonymNames
                .Select(LoadSynonymSync)
                .ToAsyncEnumerable();
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

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);

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

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);

            return new SqlServerDatabaseSynonym(Database, synonymName, targetName);
        }

        private DatabaseMetadata LoadDatabaseMetadata()
        {
            const string sql = "select @@SERVERNAME as ServerName, db_name() as DatabaseName, schema_name() as DefaultSchema";
            return Connection.QuerySingle<DatabaseMetadata>(sql);
        }

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null || identifier.LocalName == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? ServerName;
            var databaseName = identifier.Database ?? DatabaseName;
            var schema = identifier.Schema ?? DefaultSchema;

            return new Identifier(serverName, databaseName, schema, identifier.LocalName);
        }

        private IRelationalDatabase _parentDb;
        private readonly Lazy<DatabaseMetadata> _metadata;
    }
}
