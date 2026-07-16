using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Polly;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A connection factory that provides Oracle connections.
/// </summary>
/// <seealso cref="IDbConnectionFactory" />
public class OracleConnectionFactory : IDbConnectionFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="connectionConfiguration">
    /// An optional callback used to configure each <see cref="OracleConnection"/> before it is opened.
    /// Use this to authenticate via a mechanism other than the connection string, e.g. from an
    /// environment variable or an external credential provider.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <see langword="null" />, empty or whitespace.</exception>
    public OracleConnectionFactory(string connectionString, Action<OracleConnection>? connectionConfiguration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        ConnectionString = connectionString;
        ConnectionConfiguration = connectionConfiguration;
    }

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    /// <value>The connection string.</value>
    protected string ConnectionString { get; }

    /// <summary>
    /// Gets the optional callback used to configure each connection before it is opened.
    /// </summary>
    /// <value>A connection configuration callback, or <see langword="null" /> if none was provided.</value>
    protected Action<OracleConnection>? ConnectionConfiguration { get; }

    /// <summary>
    /// Creates a database connection instance, but does not open the connection.
    /// </summary>
    /// <returns>An object representing a database connection.</returns>
    public DbConnection CreateConnection()
    {
        var connection = new OracleConnection(ConnectionString);
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
        .Handle<OracleException>(IsTransientError)
        .Or<TimeoutException>();

    private static bool IsTransientError(OracleException oraEx)
    {
        // there may be more error codes that we need but there are so many to go through....
        // http://docs.oracle.com/cd/B19306_01/server.102/b14219.pdf
        switch (oraEx.Number)
        {
            case -6403:                 // network address connect timeout
            case 51:                    // timeout waiting for a resource
            case 1033:                  // ORACLE initialization or shutdown in progress
            case 1034:                  // ORACLE not available
            case 1089:                  // immediate shutdown in progress - no operations are permitted
            case 3113:                  // Closed connection
            case 3135:                  // connection lost contact
            case 12150:                 // TNS:unable to send data
            case 12153:                 // TNS:not connected
            case 12154:                 // TNS:could not resolve the connect identifier specified
            case 12157:                 // TNS:internal network communication error
            case 12161:                 // TNS:internal error: partial data received
            case 12170:                 // TNS:connect timeout occurred
            case 12171:                 // TNS:could not resolve connect identifier
            case 12203:                 // TNS:could not connect to destination
            case 12224:                 // TNS:no listener
            case 12225:                 // TNS:destination host unreachable
            case 12537:                 // TNS: connection closed
            case 12541:                 // TNS:no listener
            case 12543:                 // TNS:destination host unreachable
            case 12545:                 // Connection Failed (Generally a network failure - Cannot Reach Host)
            case 12552:                 // TNS: Unable to send break
            case 12571:                 // TNS: packet writer failure
                return true;
            default:
                return false;
        }
    }
}