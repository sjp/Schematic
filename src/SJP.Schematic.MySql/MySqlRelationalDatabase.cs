﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Query;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public MySqlRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);

            var identifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithServer(ServerName)
                .WithDatabase(DatabaseName)
                .WithSchema(DefaultSchema)
                .Build();

            _tableProvider = new MySqlRelationalDatabaseTableProvider(connection, identifierDefaults, dialect.TypeProvider);
            _viewProvider = new MySqlRelationalDatabaseViewProvider(connection, identifierDefaults, dialect.TypeProvider);
        }

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
    @@hostname as ServerName,
    database() as DatabaseName,
    schema() as DefaultSchema,
    version() as DatabaseVersion";
            var metadata = Connection.QuerySingle<DatabaseMetadata>(sql);
            metadata.DatabaseVersion = "MySQL " + metadata.DatabaseVersion;
            return metadata;
        }

        private readonly Lazy<DatabaseMetadata> _metadata;

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IRelationalDatabaseViewProvider _viewProvider;
        private readonly static IDatabaseSequenceProvider _sequenceProvider = new EmptyDatabaseSequenceProvider();
        private readonly static IDatabaseSynonymProvider _synonymProvider = new EmptyDatabaseSynonymProvider();
    }
}
