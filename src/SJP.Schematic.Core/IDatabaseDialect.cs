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

        bool IsReservedKeyword(string text);

        IDbConnection CreateConnection(string connectionString);

        Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken));

        IDbTypeProvider TypeProvider { get; }

        IIdentifierDefaults GetIdentifierDefaults(IDbConnection connection);

        Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken));

        string GetDatabaseDisplayVersion(IDbConnection connection);

        Task<string> GetDatabaseDisplayVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken));

        Version GetDatabaseVersion(IDbConnection connection);

        Task<Version> GetDatabaseVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken));
    }
}
