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
        public SqliteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierDefaults identifierDefaults)
            : base(dialect, connection, identifierDefaults)
        {
            var pragma = new ConnectionPragma(Dialect, Connection);
            _tableProvider = new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, identifierDefaults, dialect.TypeProvider);
            _viewProvider = new SqliteRelationalDatabaseViewProvider(connection, pragma, dialect, identifierDefaults, dialect.TypeProvider);
        }

        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tableProvider.GetAllTables(cancellationToken);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableProvider.GetTable(tableName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IRelationalDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _viewProvider.GetAllViews(cancellationToken);
        }

        public OptionAsync<IRelationalDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewProvider.GetView(viewName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _sequenceProvider.GetAllSequences(cancellationToken);
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceProvider.GetSequence(sequenceName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _synonymProvider.GetAllSynonyms(cancellationToken);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymProvider.GetSynonym(synonymName, cancellationToken);
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
