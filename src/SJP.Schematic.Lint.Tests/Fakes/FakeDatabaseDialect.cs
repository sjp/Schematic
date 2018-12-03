using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests.Fakes
{
    internal sealed class FakeDatabaseDialect : DatabaseDialect<FakeDatabaseDialect>
    {
        public override IDbTypeProvider TypeProvider => null;

        public override IDbConnection CreateConnection(string connectionString) => null;

        public override Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IDbConnection>(null);

        public override bool IsReservedKeyword(string text) => ReservedKeywords.Contains(text);

        public IEnumerable<string> ReservedKeywords { get; set; } = new List<string>();

        public override IIdentifierDefaults GetIdentifierDefaults(IDbConnection connection) => null;

        public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IIdentifierDefaults>(null);

        public override string GetDatabaseVersion(IDbConnection connection) => null;

        public override Task<string> GetDatabaseVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<string>(null);
    }
}
