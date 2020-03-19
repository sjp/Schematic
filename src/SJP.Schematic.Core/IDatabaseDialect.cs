using System;
using System.Data;
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

        IDependencyProvider GetDependencyProvider();

        Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(IDbConnection connection, CancellationToken cancellationToken = default);

        Task<Version> GetDatabaseVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default);

        Task<string> GetDatabaseDisplayVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default);

        Task<IRelationalDatabase> GetRelationalDatabaseAsync(IDbConnection connection, CancellationToken cancellationToken = default);

        Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(IDbConnection connection, CancellationToken cancellationToken = default);
    }
}
