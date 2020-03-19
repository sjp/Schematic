using System;
using System.Data;

namespace SJP.Schematic.Core
{
    public class SchematicConnectionFactory : ISchematicConnectionFactory
    {
        public SchematicConnectionFactory(string connectionString, IDatabaseDialect dialect)
        {
            ConnectionString = connectionString;
            Dialect = dialect;
        }

        protected string ConnectionString { get; }

        protected IDatabaseDialect Dialect { get; }

        public ISchematicConnection CreateConnection() => new SchematicConnection(Guid.NewGuid(), (IDbConnection)null!, Dialect);
    }
}
