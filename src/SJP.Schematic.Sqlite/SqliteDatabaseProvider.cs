using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// Provides access to a SQLite relational database and its metadata, for a given connection.
/// </summary>
/// <seealso cref="IRelationalDatabaseProvider" />
public class SqliteDatabaseProvider : IRelationalDatabaseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public SqliteDatabaseProvider(ISchematicConnection connection)
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
        return GetIdentifierDefaultsAsyncCore();
    }

    private static Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore()
    {
        var identifierDefaults = new IdentifierDefaults(null, null, DefaultSchema);
        return Task.FromResult<IIdentifierDefaults>(identifierDefaults);
    }

    private const string DefaultSchema = "main";

    /// <summary>
    /// Gets the database display version. Usually a more user-friendly form of the database version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A descriptive version.</returns>
    public Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default)
    {
        return GetDatabaseDisplayVersionAsyncCore(Connection, cancellationToken);
    }

    private static async Task<string> GetDatabaseDisplayVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken);
        return "SQLite " + versionStr;
    }

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
        var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken);
        return Version.Parse(versionStr!);
    }

    private const string DatabaseDisplayVersionQuerySql = "select sqlite_version()";

    /// <summary>
    /// Retrieves a relational database for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database.</returns>
    public Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return GetRelationalDatabaseAsyncCore(Connection);
    }

    private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore();
        return new SqliteRelationalDatabase(connection, identifierDefaults, new ConnectionPragma(connection));
    }

    /// <summary>
    /// Retrieves a relational database comment provider for the underlying database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A comment provider.</returns>
    public async Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore();
        return new EmptyRelationalDatabaseCommentProvider(identifierDefaults);
    }
}
