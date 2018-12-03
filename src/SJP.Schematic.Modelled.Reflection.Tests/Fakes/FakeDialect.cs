using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes
{
    internal sealed class FakeDialect : DatabaseDialect<FakeDialect>
    {
        public override IDbConnection CreateConnection(string connectionString) => null;

        public override Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IDbConnection>(null);

        public override bool IsReservedKeyword(string text) => false;

        public override IDatabaseIdentifierDefaults GetIdentifierDefaults(IDbConnection connection) => null;

        public override Task<IDatabaseIdentifierDefaults> GetIdentifierDefaultsAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IDatabaseIdentifierDefaults>(null);

        public override string GetDatabaseVersion(IDbConnection connection) => null;

        public override Task<string> GetDatabaseVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<string>(null);

        public override IDbTypeProvider TypeProvider => _typeProvider;

        private readonly static IDbTypeProvider _typeProvider = new DbTypeProvider();

        private sealed class DbTypeProvider : IDbTypeProvider
        {
            public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata) => new ColumnDataType(
                "test_type",
                DataType.BigInteger,
                "test_type(10)",
                typeof(object),
                false,
                10,
                new NumericPrecision(10, 0),
                null
            );

            public IDbType GetComparableColumnType(IDbType otherType) => CreateColumnType(null);
        }
    }
}
