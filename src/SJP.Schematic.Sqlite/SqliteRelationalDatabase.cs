using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabase : RelationalDatabase, ISqliteDatabase
    {
        public SqliteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IDatabaseIdentifierDefaults identifierDefaults)
            : base(dialect, connection, identifierDefaults)
        {
            var pragma = new ConnectionPragma(Dialect, Connection);
            _tableProvider = new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, identifierDefaults, dialect.TypeProvider);
            _viewProvider = new SqliteRelationalDatabaseViewProvider(connection, pragma, dialect, identifierDefaults, dialect.TypeProvider);
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

        public void AttachDatabase(string schemaName, string fileName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var sql = AttachDatabaseQuery(schemaName, fileName);
            Connection.Execute(sql);
        }

        public Task AttachDatabaseAsync(string schemaName, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var sql = AttachDatabaseQuery(schemaName, fileName);
            return Connection.ExecuteAsync(sql);
        }

        protected virtual string AttachDatabaseQuery(string schemaName, string fileName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var quotedSchemaName = Dialect.QuoteIdentifier(schemaName);
            var escapedFileName = fileName.Replace("'", "''");

            return $"ATTACH DATABASE '{ escapedFileName }' AS { quotedSchemaName }";
        }

        public void DetachDatabase(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = DetachDatabaseQuery(schemaName);
            Connection.Execute(sql);
        }

        public Task DetachDatabaseAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = DetachDatabaseQuery(schemaName);
            return Connection.ExecuteAsync(sql);
        }

        protected virtual string DetachDatabaseQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return "DETACH DATABASE " + Dialect.QuoteIdentifier(schemaName);
        }

        public void Vacuum()
        {
            const string sql = "vacuum";
            Connection.Execute(sql);
        }

        public Task VacuumAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "vacuum";
            return Connection.ExecuteAsync(sql);
        }

        public void Vacuum(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = VacuumQuery(schemaName);
            Connection.Execute(sql);
        }

        public Task VacuumAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = VacuumQuery(schemaName);
            return Connection.ExecuteAsync(sql);
        }

        protected virtual string VacuumQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return "vacuum " + Dialect.QuoteIdentifier(schemaName);
        }

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IRelationalDatabaseViewProvider _viewProvider;
        private readonly static IDatabaseSequenceProvider _sequenceProvider = new EmptyDatabaseSequenceProvider();
        private readonly static IDatabaseSynonymProvider _synonymProvider = new EmptyDatabaseSynonymProvider();
    }
}
