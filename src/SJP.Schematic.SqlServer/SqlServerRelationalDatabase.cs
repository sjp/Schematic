using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabase : RelationalDatabase, IDependentRelationalDatabase
    {
        public SqlServerRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IEqualityComparer<Identifier> comparer = null)
            : base(dialect, connection)
        {
            _metadata = new AsyncLazy<DatabaseMetadata>(LoadDatabaseMetadataAsync);
            Comparer = comparer ?? new IdentifierComparer(StringComparer.OrdinalIgnoreCase, ServerName, DatabaseName, DefaultSchema);
            _parentDb = this;
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(value));
        }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected IRelationalDatabase Database => Parent;

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata => _metadata.Task.GetAwaiter().GetResult();

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            return Connection.ExecuteScalar<int>(
                TableExistsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ) != 0;
        }

        public Task<bool> TableExistsAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TableExistsAsyncCore(tableName, cancellationToken);
        }

        private async Task<bool> TableExistsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = CreateQualifiedIdentifier(tableName);

            return await Connection.ExecuteScalarAsync<int>(
                TableExistsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string TableExistsQuery => TableExistsQuerySql;

        private const string TableExistsQuerySql = "select top 1 1 from sys.tables where schema_id = schema_id(@SchemaName) and name = @TableName";

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableSync(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableAsync(tableName, cancellationToken);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                var tableNames = Connection.Query<QualifiedName>(TablesQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var tableName in tableNames)
                    yield return LoadTableSync(tableName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery).ConfigureAwait(false);
            var tableNames = queryResults.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return tableNames
                .Select(LoadTableSync)
                .ToAsyncEnumerable();
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return TableExists(tableName)
                ? new SqlServerRelationalDatabaseTable(Connection, Database, tableName, Comparer)
                : null;
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = CreateQualifiedIdentifier(tableName);
            var exists = await TableExistsAsync(tableName, cancellationToken).ConfigureAwait(false);
            return exists
                ? new SqlServerRelationalDatabaseTable(Connection, Database, tableName, Comparer)
                : null;
        }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return Connection.ExecuteScalar<int>(
                ViewExistsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ) != 0;
        }

        public Task<bool> ViewExistsAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ViewExistsAsyncCore(viewName, cancellationToken);
        }

        private async Task<bool> ViewExistsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = CreateQualifiedIdentifier(viewName);

            return await Connection.ExecuteScalarAsync<int>(
                ViewExistsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string ViewExistsQuery => ViewExistsQuerySql;

        private const string ViewExistsQuerySql = "select top 1 1 from sys.views where schema_id = schema_id(@SchemaName) and name = @ViewName";

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewSync(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewAsync(viewName, cancellationToken);
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var viewName in viewNames)
                    yield return LoadViewSync(viewName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return viewNames
                .Select(LoadViewSync)
                .ToAsyncEnumerable();
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return ViewExists(viewName)
                ? new SqlServerRelationalDatabaseView(Connection, Database, viewName, Comparer)
                : null;
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<IRelationalDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = CreateQualifiedIdentifier(viewName);
            var exists = await ViewExistsAsync(viewName, cancellationToken).ConfigureAwait(false);
            return exists
                ? new SqlServerRelationalDatabaseView(Connection, Database, viewName, Comparer)
                : null;
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return Connection.ExecuteScalar<int>(
                SequenceExistsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ) != 0;
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return SequenceExistsAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<bool> SequenceExistsAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return await Connection.ExecuteScalarAsync<int>(
                SequenceExistsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string SequenceExistsQuery => SequenceExistsQuerySql;

        private const string SequenceExistsQuerySql = "select top 1 1 from sys.sequences where schema_id = schema_id(@SchemaName) and name = @SequenceName";

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceSync(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceAsync(sequenceName);
        }

        public IEnumerable<IDatabaseSequence> Sequences
        {
            get
            {
                var sequenceNames = Connection.Query<QualifiedName>(SequencesQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var sequenceName in sequenceNames)
                    yield return LoadSequenceSync(sequenceName);
            }
        }

        public async Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery).ConfigureAwait(false);
            var sequenceNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return sequenceNames
                .Select(LoadSequenceSync)
                .ToAsyncEnumerable();
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name";

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return SequenceExists(sequenceName)
                ? new SqlServerDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<IDatabaseSequence> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var exists = await SequenceExistsAsync(sequenceName, cancellationToken).ConfigureAwait(false);
            return exists
                ? new SqlServerDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            return Connection.ExecuteScalar<int>(
                SynonymExistsQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ) != 0;
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return SynonymExistsAsyncCore(synonymName, cancellationToken);
        }

        private async Task<bool> SynonymExistsAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);

            return await Connection.ExecuteScalarAsync<int>(
                SynonymExistsQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string SynonymExistsQuery => SynonymExistsQuerySql;

        private const string SynonymExistsQuerySql = "select top 1 1 from sys.synonyms where schema_id = schema_id(@SchemaName) and name = @SynonymName";

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymSync(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymAsync(synonymName, cancellationToken);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms
        {
            get
            {
                var synonyms = Connection.Query<QualifiedName>(SynonymsQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var synonym in synonyms)
                    yield return LoadSynonymSync(synonym);
            }
        }

        public async Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SynonymsQuery).ConfigureAwait(false);
            var synonymNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return synonymNames
                .Select(LoadSynonymSync)
                .ToAsyncEnumerable();
        }

        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            if (!SynonymExists(synonymName))
                return null;

            var queryResult = Connection.QuerySingle<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new SqlServerDatabaseSynonym(Database, synonymName, targetName);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken);
        }

        private async Task<IDatabaseSynonym> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);
            var exists = await SynonymExistsAsync(synonymName, cancellationToken).ConfigureAwait(false);
            if (!exists)
                return null;

            var queryResult = await Connection.QuerySingleAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false);

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new SqlServerDatabaseSynonym(Database, synonymName, targetName);
        }

        protected virtual string LoadSynonymQuery => LoadSynonymQuerySql;

        private const string LoadSynonymQuerySql = @"
select
    PARSENAME(base_object_name, 4) as TargetServerName,
    PARSENAME(base_object_name, 3) as TargetDatabaseName,
    PARSENAME(base_object_name, 2) as TargetSchemaName,
    PARSENAME(base_object_name, 1) as TargetObjectName
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName";

        private Task<DatabaseMetadata> LoadDatabaseMetadataAsync()
        {
            const string sql = "select @@SERVERNAME as ServerName, db_name() as DatabaseName, schema_name() as DefaultSchema";
            return Connection.QuerySingleAsync<DatabaseMetadata>(sql);
        }

        /// <summary>
        /// Qualifies an identifier with information from the database. For example, sets the schema if it is missing.
        /// </summary>
        /// <param name="identifier">An identifier which may or may not be fully qualified.</param>
        /// <returns>A new identifier, which will have components present where they were previously missing.</returns>
        /// <remarks>No components of an identifier when present will be modified. For example, when given a fully qualified identifier, a new identifier will be returned that is equal in value to the argument.</remarks>
        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? ServerName;
            var databaseName = identifier.Database ?? DatabaseName;
            var schema = identifier.Schema ?? DefaultSchema;

            return new Identifier(serverName, databaseName, schema, identifier.LocalName);
        }

        private IRelationalDatabase _parentDb;
        private readonly AsyncLazy<DatabaseMetadata> _metadata;
    }
}
