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

        IDbTypeProvider TypeProvider { get; }

        Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
