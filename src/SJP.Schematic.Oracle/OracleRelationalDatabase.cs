using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public OracleRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierResolutionStrategy identifierResolver)
            : base(dialect, connection)
        {
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);

            var identifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithServer(ServerName)
                .WithDatabase(DatabaseName)
                .WithSchema(DefaultSchema)
                .Build();

            _tableProvider = new OracleRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver, dialect.TypeProvider);
            _viewProvider = new OracleRelationalDatabaseViewProvider(connection, identifierDefaults, identifierResolver, dialect.TypeProvider);
            _sequenceProvider = new OracleDatabaseSequenceProvider(connection, identifierDefaults, identifierResolver);
            _synonymProvider = new OracleDatabaseSynonymProvider(connection, identifierDefaults, identifierResolver);
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
            const string hostSql = @"
select
    SYS_CONTEXT('USERENV', 'SERVER_HOST') as ServerHost,
    SYS_CONTEXT('USERENV', 'INSTANCE_NAME') as ServerSid,
    SYS_CONTEXT('USERENV', 'DB_NAME') as DatabaseName,
    SYS_CONTEXT('USERENV', 'CURRENT_USER') as DefaultSchema
from DUAL";
            var hostInfoOption = Connection.QueryFirstOrNone<DatabaseHost>(hostSql);

            const string versionSql = @"
select
    PRODUCT as ProductName,
    VERSION as VersionNumber
from PRODUCT_COMPONENT_VERSION
where PRODUCT like 'Oracle Database%'";
            var versionInfoOption = Connection.QueryFirstOrNone<DatabaseVersion>(versionSql);

            var qualifiedServerName = hostInfoOption.MatchUnsafe(
                dbHost => dbHost.ServerHost + "/" + dbHost.ServerSid,
                () => null
            );
            var dbName = hostInfoOption.MatchUnsafe(h => h.DatabaseName, () => null);
            var defaultSchema = hostInfoOption.MatchUnsafe(h => h.DefaultSchema, () => null);
            var versionName = versionInfoOption.MatchUnsafe(
                vInfo => vInfo.ProductName + vInfo.VersionNumber,
                () => null
            );

            return new DatabaseMetadata
            {
                 ServerName = qualifiedServerName,
                 DatabaseName = dbName,
                 DefaultSchema = defaultSchema,
                 DatabaseVersion = versionName
            };
        }

        private readonly Lazy<DatabaseMetadata> _metadata;

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IRelationalDatabaseViewProvider _viewProvider;
        private readonly IDatabaseSequenceProvider _sequenceProvider;
        private readonly IDatabaseSynonymProvider _synonymProvider;
    }
}
