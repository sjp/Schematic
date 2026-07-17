using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Comments;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Provides access to an Oracle relational database and its metadata, for a given connection.
/// </summary>
/// <seealso cref="IRelationalDatabaseProvider" />
public class OracleDatabaseProvider : IRelationalDatabaseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public OracleDatabaseProvider(ISchematicConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// The connection that this provider is bound to.
    /// </summary>
    /// <value>A schematic connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Retrieves the set of identifier defaults for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A set of identifier defaults.</returns>
    public Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken = default)
    {
        return GetIdentifierDefaultsAsyncCore(Connection, cancellationToken);
    }

    private static async Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var hostInfoOption = connection.ConnectionFactory.QueryFirstOrNone<GetIdentifierDefaults.Result>(GetIdentifierDefaults.Sql, cancellationToken);
        var qualifiedServerName = await hostInfoOption
            .Bind(static dbHost => dbHost.ServerHost != null && dbHost.ServerSid != null
                ? OptionAsync<GetIdentifierDefaults.Result>.Some(dbHost)
                : OptionAsync<GetIdentifierDefaults.Result>.None
            )
            .MatchUnsafe(
                static dbHost => dbHost.ServerHost + "/" + dbHost.ServerSid,
                static () => (string?)null
            );
        var dbName = await hostInfoOption.MatchUnsafe(h => h.DatabaseName, () => null);
        var defaultSchema = await hostInfoOption.MatchUnsafe(h => h.DefaultSchema, () => null);

        return new IdentifierDefaults(qualifiedServerName, dbName, defaultSchema);
    }

    /// <summary>
    /// Gets the database display version. Usually a more user-friendly form of the database version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A descriptive version.</returns>
    public Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default)
    {
        var versionInfoOption = Connection.ConnectionFactory.QueryFirstOrNone<GetDatabaseVersion.Result>(GetDatabaseVersion.Sql, cancellationToken);
        return versionInfoOption.MatchUnsafe(
            static vInfo => vInfo.ProductName + vInfo.VersionNumber,
            static () => string.Empty
        );
    }

    /// <summary>
    /// Gets the database version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A version.</returns>
    public Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default)
    {
        var versionInfoOption = Connection.ConnectionFactory.QueryFirstOrNone<GetDatabaseVersion.Result>(GetDatabaseVersion.Sql, cancellationToken);
        return versionInfoOption
            .Bind(static dbv => TryParseLongVersionString(dbv.VersionNumber).ToAsync())
            .MatchUnsafeAsync(
                v => v,
                static () => Task.FromResult(new Version(0, 0))
            );
    }

    private static Option<Version> TryParseLongVersionString(string? version)
    {
        if (version.IsNullOrWhiteSpace())
            return Option<Version>.None;

        var dotCount = version.Count(static c => c == '.');
        if (dotCount < 4)
        {
            return Version.TryParse(version, out var validVersion)
                ? Option<Version>.Some(validVersion)
                : Option<Version>.None;
        }

        // only take the first 4 version numbers and try again
        var versionStr = version
            .Split(['.'], 5, StringSplitOptions.RemoveEmptyEntries)
            .Take(4)
            .Join(".");
        return Version.TryParse(versionStr, out var v)
                ? Option<Version>.Some(v)
                : Option<Version>.None;
    }

    /// <summary>
    /// Retrieves a relational database for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database.</returns>
    public Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return GetRelationalDatabaseAsyncCore(Connection, cancellationToken);
    }

    private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
        return new OracleRelationalDatabase(connection, identifierDefaults, identifierResolver);
    }

    /// <summary>
    /// Retrieves a relational database comment provider for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A comment provider.</returns>
    public Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default)
    {
        return GetRelationalDatabaseCommentProviderAsyncCore(Connection, cancellationToken);
    }

    private static async Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
        return new OracleDatabaseCommentProvider(connection.ConnectionFactory, identifierDefaults, identifierResolver);
    }
}
