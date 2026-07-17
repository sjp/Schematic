using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Comments;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Provides access to a SQL Server relational database and its metadata, for a given connection.
/// </summary>
/// <seealso cref="ISqlServerDatabaseProvider" />
public class SqlServerDatabaseProvider : ISqlServerDatabaseProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public SqlServerDatabaseProvider(ISchematicConnection connection)
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
        return Connection.ConnectionFactory.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken)!;
    }

    private const string DatabaseDisplayVersionQuerySql = "select @@version as DatabaseVersion";

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
        var versionStr = await connection.ConnectionFactory.ExecuteScalarAsync<string>(GetDatabaseVersion.Sql, cancellationToken);
        return Version.Parse(versionStr!);
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
        return new SqlServerRelationalDatabase(connection, identifierDefaults);
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
        return new SqlServerDatabaseCommentProvider(connection.ConnectionFactory, identifierDefaults);
    }

    /// <summary>
    /// Retrieve the assigned compatibility level for the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A representation of the compatibility level assigned to the database.</returns>
    public Task<CompatibilityLevel> GetCompatibilityLevel(CancellationToken cancellationToken = default)
    {
        return GetCompatibilityLevelCore(Connection, cancellationToken);
    }

    private static async Task<CompatibilityLevel> GetCompatibilityLevelCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var dbResult = await connection.ConnectionFactory.QuerySingleAsync<Queries.GetCompatibilityLevel.Result>(Queries.GetCompatibilityLevel.Sql, cancellationToken);
        return new CompatibilityLevel(dbResult.CompatibilityLevel);
    }
}
