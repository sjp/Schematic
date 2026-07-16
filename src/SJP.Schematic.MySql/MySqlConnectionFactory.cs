using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using Polly;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql;

/// <summary>
/// A connection factory that provides MySQL connections.
/// </summary>
/// <seealso cref="IDbConnectionFactory" />
public class MySqlConnectionFactory : IDbConnectionFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="connectionConfiguration">
    /// An optional callback used to configure each <see cref="MySqlConnection"/> before it is opened.
    /// Use this to authenticate via a mechanism other than the connection string, e.g. from an
    /// environment variable or an external credential provider.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <see langword="null" />, empty or whitespace.</exception>
    public MySqlConnectionFactory(string connectionString, Action<MySqlConnection>? connectionConfiguration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        DataSource = new MySqlDataSource(connectionString);
        ConnectionConfiguration = connectionConfiguration;
    }

    /// <summary>
    /// Gets the database provider's connection factory.
    /// </summary>
    /// <value>The database provider connection factory.</value>
    protected MySqlDataSource DataSource { get; }

    /// <summary>
    /// Gets the optional callback used to configure each connection before it is opened.
    /// </summary>
    /// <value>A connection configuration callback, or <see langword="null" /> if none was provided.</value>
    protected Action<MySqlConnection>? ConnectionConfiguration { get; }

    /// <summary>
    /// Creates a database connection instance, but does not open the connection.
    /// </summary>
    /// <returns>An object representing a database connection</returns>
    public DbConnection CreateConnection()
    {
        var connection = DataSource.CreateConnection();
        ConnectionConfiguration?.Invoke(connection);

        return connection;
    }

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
        var connection = CreateConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

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
    public PolicyBuilder RetryPolicy => Policy
        .Handle<MySqlException>(IsTransientError)
        .Or<TimeoutException>();

    private static bool IsTransientError(MySqlException mysqlEx)
    {
        switch (mysqlEx.Number)
        {
            case 1042: // ER_BAD_HOST_ERROR
            case 2002: // CR_CONNECTION_ERROR
            case 2003: // CR_CONN_HOST_ERROR
            case 2006: // CR_SERVER_GONE_ERROR
            case 2009: // CR_WRONG_HOST_INFO
            case 2013: // CR_SERVER_LOST
                return true;
            default:
                return false;
        }
    }
}