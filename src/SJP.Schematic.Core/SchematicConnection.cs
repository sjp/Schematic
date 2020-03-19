using System;
using System.Data;

namespace SJP.Schematic.Core
{
    public class SchematicConnection : ISchematicConnection
    {
        public SchematicConnection(Guid connectionId, IDbConnection connection, IDatabaseDialect dialect)
        {
            if (connectionId == Guid.Empty)
                throw new ArgumentException("An empty connection ID was provided. Consider using Guid.NewGuid() instead.", nameof(connectionId));

            ConnectionId = connectionId;
            DbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        public Guid ConnectionId { get; }

        public IDbConnection DbConnection { get; }

        public IDatabaseDialect Dialect { get; }
    }
}
