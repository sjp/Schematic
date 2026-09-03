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

    // server_version_num is a plain integer: major * 10000 + minor for v10 and newer
    // (e.g. 170004 -> 17.4), or major * 10000 + minor * 100 + patch for v9 and older
    // (e.g. 90604 -> 9.6.4).
    private static Version? ParsePostgresVersionString(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);

        if (!int.TryParse(versionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var versionNum))
            return null;

        return versionNum >= 100000
            ? new Version(versionNum / 10000, versionNum % 100)
            : new Version(versionNum / 10000, versionNum / 100 % 100, versionNum % 100);
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

    /// <summary>
    /// Retrieves a table statistics provider for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A table statistics provider.</returns>
    public Task<ITableStatisticsProvider> GetTableStatisticsProviderAsync(CancellationToken cancellationToken = default)
    {
        return GetTableStatisticsProviderAsyncCore(Connection, cancellationToken);
    }

    private static async Task<ITableStatisticsProvider> GetTableStatisticsProviderAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
        return new PostgreSqlTableStatisticsProvider(connection.ConnectionFactory, identifierDefaults);
    }
}
