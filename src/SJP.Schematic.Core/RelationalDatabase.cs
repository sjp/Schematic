using System;
using System.Data;

namespace SJP.Schematic.Core
{
    public abstract class RelationalDatabase
    {
        protected RelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        public IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public string ServerName => IdentifierDefaults.Server;

        public string DatabaseName => IdentifierDefaults.Database;

        public string DefaultSchema => IdentifierDefaults.Schema;

        public string DatabaseVersion => Dialect.GetDatabaseDisplayVersion(Connection);
    }
}
