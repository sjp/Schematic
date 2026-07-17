using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Comments;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// Provides access to a PostgreSQL relational database and its metadata, for a given connection.
/// </summary>
/// <seealso cref="IRelationalDatabaseProvider" />
public class PostgreSqlDatabaseProvider : IRelationalDatabaseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public PostgreSqlDatabaseProvider(ISchematicConnection connection)
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
        var result = await connection.ConnectionFactory.QuerySingleAsync<GetIdentifierDefaults.Result>(GetIdentifierDefaults.Sql, cancellationToken);

        if (result.Server.IsNullOrWhiteSpace())
            return result with { Server = "127.0.0.1" };

        return result;
    }

    /// <summary>
    /// Gets the database display version. Usually a more user-friendly form of the database version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A descriptive version.</returns>
    public Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default)
    {
        return Connection.ConnectionFactory.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken)!;
    }

    private const string DatabaseDisplayVersionQuerySql = "select pg_catalog.version() as DatabaseVersion";

    /// <summary>
    /// Gets the database version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A version.</returns>
    public Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default)
    {
        return GetDatabaseVersionAsyncCore(Connection, cancellationToken);
    }

    private static async Task<Version> GetDatabaseVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var versionStr = await connection.ConnectionFactory.ExecuteScalarAsync<string>(DatabaseVersionQuerySql, cancellationToken);
        return ParsePostgresVersionString(versionStr!) ?? new Version(0, 0);
    }

    private const string DatabaseVersionQuerySql = "select current_setting('server_version_num') as DatabaseVersion";

    private static Version? ParsePostgresVersionString(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);

        return versionStr.Length >= 6
            ? ParseNewPostgresVersionString(versionStr)
            : ParseOldPostgresVersionString(versionStr);
    }

    // for v10 or newer
    private static Version? ParseNewPostgresVersionString(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);
        if (versionStr.Length != 6)
            throw new ArgumentException("The version string must be 6 characters long", nameof(versionStr));

        var majorVersionStr = versionStr[..2];
        var minorVersionStr = versionStr.Substring(4, 2);
        var parsedMajor = int.TryParse(majorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var majorVersion);
        var parsedMinor = int.TryParse(minorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minorVersion);

        return parsedMajor && parsedMinor
            ? new Version(majorVersion, minorVersion)
            : null;
    }

    // for v9 or older
    private static Version? ParseOldPostgresVersionString(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);
        if (versionStr.Length != 5)
            throw new ArgumentException("The version string must be 5 characters long", nameof(versionStr));

        var majorVersionStr = versionStr[..1];
        var minorVersionStr = versionStr.Substring(1, 2);
        var patchVersionStr = versionStr.Substring(3, 2);
        var parsedMajorVersion = int.TryParse(majorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var majorVersion);
        var parsedMinorVersion = int.TryParse(minorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minorVersion);
        var parsedPatchVersion = int.TryParse(patchVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var patchVersion);

        return parsedMajorVersion && parsedMinorVersion && parsedPatchVersion
            ? new Version(majorVersion, minorVersion, patchVersion)
            : null;
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
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();
        return new PostgreSqlRelationalDatabase(connection, identifierDefaults, identifierResolver);
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
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();
        return new PostgreSqlDatabaseCommentProvider(connection.ConnectionFactory, identifierDefaults, identifierResolver);
    }
}
