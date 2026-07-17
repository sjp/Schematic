using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Comments;
using SJP.Schematic.MySql.Queries;

namespace SJP.Schematic.MySql;

/// <summary>
/// Provides access to a MySQL relational database and its metadata, for a given connection.
/// </summary>
/// <seealso cref="IRelationalDatabaseProvider" />
public class MySqlDatabaseProvider : IRelationalDatabaseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public MySqlDatabaseProvider(ISchematicConnection connection)
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
        return await connection.ConnectionFactory.QuerySingleAsync<GetIdentifierDefaults.Result>(GetIdentifierDefaults.Sql, cancellationToken);
    }

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
        var versionStr = await connection.ConnectionFactory.ExecuteScalarAsync<string>(DatabaseVersionQuerySql, cancellationToken);
        return "MySQL " + versionStr;
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
        var versionStr = await connection.ConnectionFactory.ExecuteScalarAsync<string>(DatabaseVersionQuerySql, cancellationToken);
        return ParseMySqlVersion(versionStr!);
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
        return new MySqlRelationalDatabase(connection, identifierDefaults);
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
        return new MySqlDatabaseCommentProvider(connection.ConnectionFactory, identifierDefaults);
    }

    private static Version ParseMySqlVersion(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);

        var versionPieces = versionStr
            .Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries)
            .TakeWhile(piece => int.TryParse(piece, NumberStyles.Integer, CultureInfo.InvariantCulture, out _));

        var saferVersionStr = versionPieces.Join(".");
        return Version.Parse(saferVersionStr);
    }

    private const string DatabaseVersionQuerySql = "select version() as DatabaseVersion";
}
