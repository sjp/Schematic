using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Comments;

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

        Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
