using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Tests.Fakes;

/// <summary>
/// A connection that records how it was used, and rejects an open that overlaps another open,
/// in the same manner as an ADO.NET provider.
/// </summary>
internal sealed class FakeDbConnection : DbConnection
{
    private ConnectionState _state = ConnectionState.Closed;
    private int _openCount;
    private int _disposeCount;

    public int OpenCount => Volatile.Read(ref _openCount);

    public int DisposeCount => Volatile.Read(ref _disposeCount);

    public bool OverlappingOpenDetected { get; private set; }

    public TimeSpan OpenDelay { get; set; } = TimeSpan.FromMilliseconds(20);

    public void SetBroken() => _state = ConnectionState.Broken;

    public override ConnectionState State => _state;

    [AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    public override string Database => string.Empty;

    public override string DataSource => string.Empty;

    public override string ServerVersion => string.Empty;

    public override void ChangeDatabase(string databaseName) => throw new NotSupportedException();

    public override void Close() => _state = ConnectionState.Closed;

    public override void Open()
    {
        BeginOpen();
        Thread.Sleep(OpenDelay);
        _state = ConnectionState.Open;
    }

    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        BeginOpen();
        await Task.Delay(OpenDelay, cancellationToken);
        _state = ConnectionState.Open;
    }

    private void BeginOpen()
    {
        if (_state is ConnectionState.Open or ConnectionState.Connecting)
        {
            OverlappingOpenDetected = true;
            throw new InvalidOperationException("The connection was not closed. Current state is " + _state.ToString());
        }

        _state = ConnectionState.Connecting;
        Interlocked.Increment(ref _openCount);
    }

    protected override DbCommand CreateDbCommand() => throw new NotSupportedException();

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Interlocked.Increment(ref _disposeCount);

        base.Dispose(disposing);
    }
}
