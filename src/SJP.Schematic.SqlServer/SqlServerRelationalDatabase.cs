using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;
using LanguageExt;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public SqlServerRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IDatabaseIdentifierDefaults identifierDefaults)
            : base(dialect, connection, identifierDefaults)
        {
            _tableProvider = new SqlServerRelationalDatabaseTableProvider(connection, identifierDefaults, dialect.TypeProvider);
            _viewProvider = new SqlServerRelationalDatabaseViewProvider(connection, identifierDefaults, dialect.TypeProvider);
            _sequenceProvider = new SqlServerDatabaseSequenceProvider(connection, identifierDefaults);
            _synonymProvider = new SqlServerDatabaseSynonymProvider(connection, identifierDefaults);
        }

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

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IRelationalDatabaseViewProvider _viewProvider;
        private readonly IDatabaseSequenceProvider _sequenceProvider;
        private readonly IDatabaseSynonymProvider _synonymProvider;
    }
}
