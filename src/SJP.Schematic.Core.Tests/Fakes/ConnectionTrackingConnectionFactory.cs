using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace SJP.Schematic.Core.Tests.Fakes;

/// <summary>
/// A connection factory decorator with a configurable query limit, recording the largest number of
/// connections that were open at the same time.
/// </summary>
internal sealed class ConnectionTrackingConnectionFactory : IDbConnectionFactory
{
    /// <param name="innerFactory">The factory that provides the connections.</param>
    /// <param name="maxConcurrentQueries">The query limit to report.</param>
    /// <param name="openDelay">
    /// How long each open is delayed by, so that queries allowed to run at once are still open when the
    /// last of them is counted.
    /// </param>
    public ConnectionTrackingConnectionFactory(IDbConnectionFactory innerFactory, int maxConcurrentQueries, TimeSpan openDelay = default)
    {
        _innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
        MaxConcurrentQueries = maxConcurrentQueries;
        _openDelay = openDelay;
    }

    public int MaxConcurrentQueries { get; }

    public int PeakOpenConnections
    {
        get
        {
            lock (_lock)
                return _peakOpenConnections;
        }
    }

    public DbConnection CreateConnection() => _innerFactory.CreateConnection();

    public DbConnection OpenConnection() => throw new NotSupportedException();

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _openConnections++;
            _peakOpenConnections = Math.Max(_peakOpenConnections, _openConnections);
        }

        try
        {
            if (_openDelay > TimeSpan.Zero)
                await Task.Delay(_openDelay, cancellationToken);

            var connection = await _innerFactory.OpenConnectionAsync(cancellationToken);
            connection.StateChange += OnStateChange;
            return connection;
        }
        catch
        {
            ConnectionClosed();
            throw;
        }
    }

    public bool DisposeConnection => _innerFactory.DisposeConnection;

    public PolicyBuilder RetryPolicy => _innerFactory.RetryPolicy;

    private void OnStateChange(object sender, StateChangeEventArgs e)
    {
        if (e.CurrentState != ConnectionState.Closed)
            return;

        ((DbConnection)sender).StateChange -= OnStateChange;
        ConnectionClosed();
    }

    private void ConnectionClosed()
    {
        lock (_lock)
            _openConnections--;
    }

    private readonly IDbConnectionFactory _innerFactory;
    private readonly TimeSpan _openDelay;
    private readonly Lock _lock = new();
    private int _openConnections;
    private int _peakOpenConnections;
}
