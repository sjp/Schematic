using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace SJP.Schematic.Core;

/// <summary>
/// A <see cref="IDbConnectionFactory"/> instance that always returns the same <see cref="IDbConnection"/> instance.
/// </summary>
/// <seealso cref="IDbConnectionFactory" />
public sealed class CachingConnectionFactory : IDbConnectionFactory
{
    private readonly Lock _lock = new();
    private readonly IDbConnectionFactory _connectionFactory;
    private DbConnection? _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionFactory">A connection factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <see langword="null" />.</exception>
    public CachingConnectionFactory(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Creates a database connection. If it has already been created, the existing one will be returned.
    /// </summary>
    /// <returns>A database connection.</returns>
    /// <remarks>The connection will not be opened as part of this operation.</remarks>
    public DbConnection CreateConnection()
    {
        lock (_lock)
        {
            if (_connection != null)
                return _connection;

            _connection = _connectionFactory.CreateConnection();
            return _connection;
        }
    }

    /// <summary>
    /// Creates a database connection, if required. The connection is then opened if it is not already open.
    /// </summary>
    /// <returns>A database connection in an open state.</returns>
    public DbConnection OpenConnection()
    {
        var connection = CreateConnection();

        if (connection.State != ConnectionState.Open)
            connection.Open();

        return connection;
    }

    /// <summary>
    /// Creates a database connection, if required. The connection is then opened asynchronously if it is not already open.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database connection in an open state.</returns>
    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        return connection;
    }

    /// <summary>
    /// Indicates whether the resulting connection should automatically be disposed.
    /// </summary>
    /// <value>Always <see langword="false" />.</value>
    /// <remarks>Not intended to be used directly, used for internals.</remarks>
    public bool DisposeConnection { get; }

    /// <summary>
    /// Gets a database command retry policy.
    /// </summary>
    /// <value>A retry policy.</value>
    public PolicyBuilder RetryPolicy => _connectionFactory.RetryPolicy;
}