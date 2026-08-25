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
/// <remarks>
/// The factory owns the connection that it caches. Callers must not dispose a connection returned from this
/// factory; dispose the factory instead to release it.
/// </remarks>
/// <seealso cref="IDbConnectionFactory" />
public sealed class CachingConnectionFactory : IDbConnectionFactory, IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly IDbConnectionFactory _connectionFactory;
    private DbConnection? _connection;
    private bool _disposed;

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
    /// <returns>A database connection. The connection is owned by this factory and must not be disposed by the caller.</returns>
    /// <exception cref="ObjectDisposedException">The factory has been disposed.</exception>
    /// <remarks>The connection will not be opened as part of this operation.</remarks>
    public DbConnection CreateConnection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _connectionLock.Wait();
        try
        {
            return GetOrCreateConnection();
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Creates a database connection, if required. The connection is then opened if it is not already open.
    /// </summary>
    /// <returns>A database connection in an open state. The connection is owned by this factory and must not be disposed by the caller.</returns>
    /// <exception cref="ObjectDisposedException">The factory has been disposed.</exception>
    public DbConnection OpenConnection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _connectionLock.Wait();
        try
        {
            var connection = GetOrCreateConnection();
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Creates a database connection, if required. The connection is then opened asynchronously if it is not already open.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database connection in an open state. The connection is owned by this factory and must not be disposed by the caller.</returns>
    /// <exception cref="ObjectDisposedException">The factory has been disposed.</exception>
    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            var connection = GetOrCreateConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            return connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Indicates whether the resulting connection should automatically be disposed.
    /// </summary>
    /// <value>Always <see langword="false" />, the cached connection is owned by this factory.</value>
    /// <remarks>Not intended to be used directly, used for internals.</remarks>
    public bool DisposeConnection => false;

    /// <summary>
    /// Gets a database command retry policy.
    /// </summary>
    /// <value>A retry policy.</value>
    public PolicyBuilder RetryPolicy => _connectionFactory.RetryPolicy;

    /// <summary>
    /// Releases the cached connection, if one was created.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _connection?.Dispose();
        _connection = null;
        _connectionLock.Dispose();
    }

    /// <summary>
    /// Asynchronously releases the cached connection, if one was created.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _connectionLock.Dispose();
    }

    // Callers must hold _connectionLock for the duration of this call.
    private DbConnection GetOrCreateConnection()
    {
        if (_connection?.State == ConnectionState.Broken)
        {
            _connection.Dispose();
            _connection = null;
        }

        return _connection ??= _connectionFactory.CreateConnection();
    }
}
