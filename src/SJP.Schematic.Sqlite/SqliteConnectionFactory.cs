using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Polly;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A connection factory that provides SQLite connections.
/// </summary>
/// <seealso cref="IDbConnectionFactory" />
public class SqliteConnectionFactory : IDbConnectionFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <see langword="null" />, empty or whitespace.</exception>
    public SqliteConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        ConnectionString = connectionString;
    }

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    /// <value>The connection string.</value>
    protected string ConnectionString { get; }

    /// <summary>
    /// Creates a database connection instance, but does not open the connection.
    /// </summary>
    /// <returns>An object representing a database connection.</returns>
    public DbConnection CreateConnection() => new SqliteConnection(ConnectionString);

    /// <summary>
    /// Creates and opens a database connection.
    /// </summary>
    /// <returns>An object representing a database connection.</returns>
    public DbConnection OpenConnection()
    {
        var connection = CreateConnection();

        if (connection.State != ConnectionState.Open)
            connection.Open();

        return connection;
    }

    /// <summary>
    /// Creates and opens a database connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing an object representing a database connection when completed.</returns>
    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(ConnectionString);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return connection;
    }

    /// <summary>
    /// Determines whether connections retrieved from this factory should be disposed.
    /// </summary>
    /// <value>Always <see langword="true" />.</value>
    public bool DisposeConnection { get; } = true;

    /// <summary>
    /// Gets a database command retry policy builder.
    /// </summary>
    /// <value>A retry policy builder.</value>
    public PolicyBuilder RetryPolicy => Policy.Handle<TimeoutException>();
}