using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabase : RelationalDatabase, IDependentRelationalDatabase
    {
        public PostgreSqlRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IEqualityComparer<Identifier> comparer = null)
            : base(dialect, connection)
        {
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);
            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, ServerName, DatabaseName, DefaultSchema);
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

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Value;

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

        private const string TableExistsQuerySql = "select 1 from pg_catalog.pg_tables where schemaname = @SchemaName and tablename = @TableName and schemaname not in ('pg_catalog', 'information_schema') limit 1";

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

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables
        {
            get
            {
                var tableNames = Connection.Query<QualifiedName>(TablesQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var tables = tableNames.Select(LoadTableSync);
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(tableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var tables = tableNames.Select(name => LoadTableAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseTable>>(tableNames.Count, tables);
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = "select schemaname as SchemaName, tablename as ObjectName from pg_catalog.pg_tables where schemaname not in ('pg_catalog', 'information_schema')";

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return TableExists(tableName)
                ? new PostgreSqlRelationalDatabaseTable(Connection, Database, tableName, Comparer)
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
                ? new PostgreSqlRelationalDatabaseTable(Connection, Database, tableName, Comparer)
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

        private const string ViewExistsQuerySql = "select 1 from pg_catalog.pg_views where schemaname = @SchemaName and viewname = @ViewName and schemaname not in ('pg_catalog', 'information_schema') limit 1";

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

        public IReadOnlyCollection<IRelationalDatabaseView> Views
        {
            get
            {
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var views = viewNames.Select(LoadViewSync);
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(viewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var views = viewNames.Select(name => LoadViewAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseView>>(viewNames.Count, views);
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = "select schemaname as SchemaName, viewname as ObjectName from pg_catalog.pg_views where schemaname not in ('pg_catalog', 'information_schema')";

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return ViewExists(viewName)
                ? new PostgreSqlRelationalDatabaseView(Connection, Database, viewName, Comparer)
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
                ? new PostgreSqlRelationalDatabaseView(Connection, Database, viewName, Comparer)
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

        public async Task<bool> SequenceExistsAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return await Connection.ExecuteScalarAsync<int>(
                SequenceExistsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string SequenceExistsQuery => SequenceExistsQuerySql;

        private const string SequenceExistsQuerySql = "select 1 from information_schema.sequences where sequence_schema = @SchemaName and sequence_name = @SequenceName and sequence_schema not in ('pg_catalog', 'information_schema') limit 1";

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
            return LoadSequenceAsync(sequenceName, cancellationToken);
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences
        {
            get
            {
                var sequenceNames = Connection.Query<QualifiedName>(SequencesQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var sequences = sequenceNames.Select(LoadSequenceSync);
                return new ReadOnlyCollectionSlim<IDatabaseSequence>(sequenceNames.Count, sequences);
            }
        }

        public async Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery).ConfigureAwait(false);
            var sequenceNames = queryResult
                .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var sequences = sequenceNames.Select(name => LoadSequenceAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IDatabaseSequence>>(sequenceNames.Count, sequences);
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = "select sequence_schema as SchemaName, sequence_name as ObjectName from information_schema.sequences where sequence_schema not in ('pg_catalog', 'information_schema')";

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return SequenceExists(sequenceName)
                ? new PostgreSqlDatabaseSequence(Connection, Database, sequenceName)
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
                ? new PostgreSqlDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return false;
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Task.FromResult(false);
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return null;
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Task.FromResult<IDatabaseSynonym>(null);
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms => Array.Empty<IDatabaseSynonym>();

        public Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySynonyms;

        private readonly static Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> _emptySynonyms = Task.FromResult<IReadOnlyCollection<Task<IDatabaseSynonym>>>(
            Array.Empty<Task<IDatabaseSynonym>>()
        );

        private DatabaseMetadata LoadDatabaseMetadata()
        {
            const string sql = @"
select
    pg_catalog.host(pg_catalog.inet_server_addr()) as ServerName,
    pg_catalog.current_database() as DatabaseName,
    pg_catalog.current_schema() as DefaultSchema,
    pg_catalog.version() as DatabaseVersion";
            var result = Connection.QuerySingle<DatabaseMetadata>(sql);

            if (result.ServerName.IsNullOrWhiteSpace())
                result.ServerName = "127.0.0.1";

            return result;
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
        private readonly Lazy<DatabaseMetadata> _metadata;
    }
}
