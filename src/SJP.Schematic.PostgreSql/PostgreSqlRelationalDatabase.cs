using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public PostgreSqlRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierResolutionStrategy identifierResolver)
            : base(dialect, connection)
        {
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);

            var identifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithServer(ServerName)
                .WithDatabase(DatabaseName)
                .WithSchema(DefaultSchema)
                .Build();

            _tableProvider = new PostgreSqlRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver, dialect.TypeProvider);
            _viewProvider = new PostgreSqlRelationalDatabaseViewProvider(connection, identifierDefaults, identifierResolver, dialect.TypeProvider);
            _sequenceProvider = new PostgreSqlDatabaseSequenceProvider(connection, identifierDefaults, identifierResolver);
        }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Value;

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables => _tableProvider.Tables;

        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tableProvider.TablesAsync(cancellationToken);
        }

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableProvider.GetTable(tableName);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableProvider.GetTableAsync(tableName, cancellationToken);
        }

        public IReadOnlyCollection<IRelationalDatabaseView> Views => _viewProvider.Views;

        public Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _viewProvider.ViewsAsync(cancellationToken);
        }

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewProvider.GetView(viewName);
        }

        public OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewProvider.GetViewAsync(viewName, cancellationToken);
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences => _sequenceProvider.Sequences;

        public Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _sequenceProvider.SequencesAsync(cancellationToken);
        }

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceProvider.GetSequence(sequenceName);
        }

        public OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceProvider.GetSequenceAsync(sequenceName, cancellationToken);
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms => _synonymProvider.Synonyms;

        public Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _synonymProvider.SynonymsAsync(cancellationToken);
        }

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymProvider.GetSynonym(synonymName);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymProvider.GetSynonymAsync(synonymName, cancellationToken);
        }

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

        private readonly Lazy<DatabaseMetadata> _metadata;

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IRelationalDatabaseViewProvider _viewProvider;
        private readonly IDatabaseSequenceProvider _sequenceProvider;
        private readonly static IDatabaseSynonymProvider _synonymProvider = new EmptyDatabaseSynonymProvider();
    }
}