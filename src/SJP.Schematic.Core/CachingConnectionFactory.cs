using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// A <see cref="IDbConnectionFactory"/> instance that always returns the same <see cref="IDbConnection"/> instance.
/// </summary>
/// <remarks>
/// The factory owns the connection that it caches. Callers must not dispose a connection returned from this
/// factory; dispose the factory instead to release it.
/// <para>
/// Disposing the factory waits for a query that is running against it, including a streamed query that is still
/// being enumerated, before the connection is released. The wait is bounded, so the connection is released anyway
/// once the wait times out.
/// </para>
/// </remarks>
/// <seealso cref="IDbConnectionFactory" />
public sealed class CachingConnectionFactory : IDbConnectionFactory, IDisposable, IAsyncDisposable
{
    private static readonly TimeSpan DefaultDisposeTimeout = TimeSpan.FromSeconds(30);

    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly TimeSpan _disposeTimeout;
    private DbConnection? _connection;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionFactory">A connection factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <see langword="null" />.</exception>
    public CachingConnectionFactory(IDbConnectionFactory connectionFactory)
        : this(connectionFactory, DefaultDisposeTimeout)
    {
    }

    internal CachingConnectionFactory(IDbConnectionFactory connectionFactory, TimeSpan disposeTimeout)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _disposeTimeout = disposeTimeout;
    }

    private bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    /// <summary>
    /// Creates a database connection. If it has already been created, the existing one will be returned.
    /// </summary>
    /// <returns>A database connection. The connection is owned by this factory and must not be disposed by the caller.</returns>
    /// <exception cref="ObjectDisposedException">The factory has been disposed.</exception>
    /// <remarks>The connection will not be opened as part of this operation.</remarks>
    public DbConnection CreateConnection()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

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
        ObjectDisposedException.ThrowIf(IsDisposed, this);

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
        ObjectDisposedException.ThrowIf(IsDisposed, this);

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
    /// Gets the maximum number of queries that may run concurrently against this factory.
    /// </summary>
    /// <value>Always 1, since every query shares the single cached connection.</value>
    public int MaxConcurrentQueries => 1;

    /// <summary>
    /// Releases the cached connection, if one was created.
    /// </summary>
    /// <remarks>Waits for a running query to finish first, for at most 30 seconds.</remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        // a query holds its slot until it has finished with the connection, so take that before the connection itself
        var startTimestamp = Stopwatch.GetTimestamp();
        var querySemaphore = ConnectionExtensions.TryGetQuerySemaphore(this);
        var hasQuerySlot = querySemaphore?.Wait(_disposeTimeout) == true;
        var hasConnectionLock = _connectionLock.Wait(GetRemainingDisposeTimeout(startTimestamp));
        try
        {
            _connection?.Dispose();
            _connection = null;
        }
        finally
        {
            if (hasConnectionLock)
                _connectionLock.Release();
            if (hasQuerySlot)
                querySemaphore!.Release();
        }
    }

    /// <summary>
    /// Asynchronously releases the cached connection, if one was created.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    /// <remarks>Waits for a running query to finish first, for at most 30 seconds.</remarks>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        // a query holds its slot until it has finished with the connection, so take that before the connection itself
        var startTimestamp = Stopwatch.GetTimestamp();
        var querySemaphore = ConnectionExtensions.TryGetQuerySemaphore(this);
        var hasQuerySlot = querySemaphore != null && await querySemaphore.WaitAsync(_disposeTimeout);
        var hasConnectionLock = await _connectionLock.WaitAsync(GetRemainingDisposeTimeout(startTimestamp));
        try
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            if (hasConnectionLock)
                _connectionLock.Release();
            if (hasQuerySlot)
                querySemaphore!.Release();
        }
    }

    private TimeSpan GetRemainingDisposeTimeout(long startTimestamp)
    {
        var remaining = _disposeTimeout - Stopwatch.GetElapsedTime(startTimestamp);
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    // Callers must hold _connectionLock for the duration of this call.
    private DbConnection GetOrCreateConnection()
    {
        // dispose may have run while the caller was waiting for the lock
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (_connection?.State == ConnectionState.Broken)
        {
            _connection.Dispose();
            _connection = null;
        }

        return _connection ??= _connectionFactory.CreateConnection();
    }
}
