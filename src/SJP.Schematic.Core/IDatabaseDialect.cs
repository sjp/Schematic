using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IDatabaseDialect
    {
        string QuoteIdentifier(string identifier);

        string QuoteName(Identifier name);

        bool IsValidColumnName(Identifier name);

        // e.g. PK_TEST_123
        bool IsValidConstraintName(Identifier name);

        // i.e. table, view, sequence, etc
        bool IsValidObjectName(Identifier name);

        IDbConnection CreateConnection(string connectionString);

        Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken));

        IDbType GetComparableColumnType(IDbType otherType);

        IDbType CreateColumnType(ColumnTypeMetadata typeMetadata);
    }
}
