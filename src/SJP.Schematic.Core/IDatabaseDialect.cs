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

        Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken = default);

        Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default);

        Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default);

        Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default);

        Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default);
    }
}
