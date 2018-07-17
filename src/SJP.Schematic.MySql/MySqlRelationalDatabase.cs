using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.MySql.Query;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabase : RelationalDatabase, IDependentRelationalDatabase
    {
        public MySqlRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IEqualityComparer<Identifier> comparer = null)
            : base(dialect, connection)
        {
            _metadata = new AsyncLazy<DatabaseMetadata>(LoadDatabaseMetadataAsync);
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

        private const string TableExistsQuerySql = "select 1 from information_schema.tables where table_schema = @SchemaName and table_name = @TableName limit 1";

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
                var tableNames = Connection.Query<QualifiedName>(TablesQuery, new { SchemaName = DefaultSchema })
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var tableName in tableNames)
                    yield return LoadTableSync(tableName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery, new { SchemaName = DefaultSchema }).ConfigureAwait(false);
            var tableNames = queryResults.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return tableNames
                .Select(LoadTableSync)
                .ToAsyncEnumerable();
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = "select TABLE_SCHEMA as SchemaName, TABLE_NAME as ObjectName from information_schema.tables where TABLE_SCHEMA = @SchemaName order by TABLE_NAME";

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return TableExists(tableName)
                ? new MySqlRelationalDatabaseTable(Connection, Database, tableName, Comparer)
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
                ? new MySqlRelationalDatabaseTable(Connection, Database, tableName, Comparer)
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

        private const string ViewExistsQuerySql = "select 1 from information_schema.views where table_schema = @SchemaName and table_name = @ViewName limit 1";

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
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery, new { SchemaName = DefaultSchema })
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var viewName in viewNames)
                    yield return LoadViewSync(viewName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery, new { SchemaName = DefaultSchema }).ConfigureAwait(false);
            var viewNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return viewNames
                .Select(LoadViewSync)
                .ToAsyncEnumerable();
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = "select TABLE_SCHEMA as SchemaName, TABLE_NAME as ObjectName from information_schema.views where TABLE_SCHEMA = @SchemaName order by TABLE_NAME";

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return ViewExists(viewName)
                ? new MySqlRelationalDatabaseView(Connection, Database, viewName, Comparer)
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
                ? new MySqlRelationalDatabaseView(Connection, Database, viewName, Comparer)
                : null;
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return false;
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Task.FromResult(false);
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return null;
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Task.FromResult<IDatabaseSequence>(null);
        }

        public IEnumerable<IDatabaseSequence> Sequences => Array.Empty<IDatabaseSequence>();

        public Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Array.Empty<IDatabaseSequence>().ToAsyncEnumerable());

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

        public IEnumerable<IDatabaseSynonym> Synonyms => Array.Empty<IDatabaseSynonym>();

        public Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Array.Empty<IDatabaseSynonym>().ToAsyncEnumerable());

        private Task<DatabaseMetadata> LoadDatabaseMetadataAsync()
        {
            const string sql = "select @@hostname as ServerName, database() as DatabaseName, schema() as DefaultSchema";
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
