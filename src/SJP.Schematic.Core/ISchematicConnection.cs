using System;
using System.Data;

namespace SJP.Schematic.Core
{
    public interface ISchematicConnection
    {
        Guid ConnectionId { get; }

        IDbConnection DbConnection { get; }

        IDatabaseDialect Dialect { get; }
    }
}
