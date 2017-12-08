using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes
{
    public class FakeDialect : DatabaseDialect<FakeDialect>
    {
        public override IDbConnection CreateConnection(string connectionString) => null;

        public override Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IDbConnection>(null);

        public override IDbType CreateColumnType(ColumnTypeMetadata typeMetadata) => new ColumnDataType(
            "test_type",
            DataType.BigInteger,
            "test_type(10)",
            typeof(object),
            false,
            10,
            new NumericPrecision(10, 0),
            null
        );

        public override bool IsValidColumnName(Identifier name) => true;

        public override bool IsValidConstraintName(Identifier name) => true;

        public override bool IsValidObjectName(Identifier name) => true;

        public override IDbType GetComparableColumnType(IDbType otherType) => CreateColumnType(null);
    }
}
