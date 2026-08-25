using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace SJP.Schematic.Core.Tests.Fakes;

/// <summary>
/// Decides which connection opens and query executions fail, and how far into their results the failure occurs.
/// </summary>
internal sealed class FaultInjector
{
    /// <param name="rowsBeforeFailure">The number of rows to return successfully before failing.</param>
    /// <param name="failureCount">The number of executions that should fail, starting from the first.</param>
    /// <param name="openFailureCount">The number of connection opens that should fail, starting from the first.</param>
    public FaultInjector(int rowsBeforeFailure, int failureCount, int openFailureCount = 0)
    {
        RowsBeforeFailure = rowsBeforeFailure;
        _remainingFailures = failureCount;
        _remainingOpenFailures = openFailureCount;
    }

    public int RowsBeforeFailure { get; }

    /// <summary>
    /// The number of times a query has been executed, i.e. how many attempts have been made.
    /// </summary>
    public int ExecutionCount { get; private set; }

    /// <summary>
    /// The number of times a connection has been asked for, including opens that failed.
    /// </summary>
    public int OpenCount { get; private set; }

    /// <summary>
    /// Records a query execution, and determines whether this particular execution should fail.
    /// </summary>
    public bool BeginExecution()
    {
        ExecutionCount++;

        if (_remainingFailures == 0)
            return false;

        _remainingFailures--;
        return true;
    }

    /// <summary>
    /// Records a connection open, and determines whether this particular open should fail.
    /// </summary>
    public bool BeginOpen()
    {
        OpenCount++;

        if (_remainingOpenFailures == 0)
            return false;

        _remainingOpenFailures--;
        return true;
    }

    private int _remainingFailures;
    private int _remainingOpenFailures;
}

/// <summary>
/// A connection factory whose opens fail, and whose connections return readers that fail mid-stream,
/// in the manner of transient errors occurring while connecting and while results are being read.
/// </summary>
internal sealed class FaultInjectingConnectionFactory : IDbConnectionFactory
{
    public FaultInjectingConnectionFactory(IDbConnectionFactory innerFactory, FaultInjector injector)
    {
        _innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
        _injector = injector ?? throw new ArgumentNullException(nameof(injector));
    }

    public DbConnection CreateConnection() => new FaultInjectingDbConnection(_innerFactory.CreateConnection(), _injector);

    public DbConnection OpenConnection() => new FaultInjectingDbConnection(_innerFactory.OpenConnection(), _injector);

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_injector.BeginOpen())
            throw new TimeoutException("A transient failure occurred while opening a connection.");

        var connection = await _innerFactory.OpenConnectionAsync(cancellationToken);
        return new FaultInjectingDbConnection(connection, _injector);
    }

    public bool DisposeConnection => _innerFactory.DisposeConnection;

    public PolicyBuilder RetryPolicy => _innerFactory.RetryPolicy;

    private readonly IDbConnectionFactory _innerFactory;
    private readonly FaultInjector _injector;
}

internal sealed class FaultInjectingDbConnection : DbConnection
{
    public FaultInjectingDbConnection(DbConnection connection, FaultInjector injector)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _injector = injector ?? throw new ArgumentNullException(nameof(injector));
    }

    [AllowNull]
    public override string ConnectionString
    {
        get => _connection.ConnectionString;
        set => _connection.ConnectionString = value;
    }

    public override string Database => _connection.Database;

    public override string DataSource => _connection.DataSource;

    public override string ServerVersion => _connection.ServerVersion;

    public override ConnectionState State => _connection.State;

    public override void ChangeDatabase(string databaseName) => _connection.ChangeDatabase(databaseName);

    public override void Close() => _connection.Close();

    public override void Open() => _connection.Open();

    public override Task OpenAsync(CancellationToken cancellationToken) => _connection.OpenAsync(cancellationToken);

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => _connection.BeginTransaction(isolationLevel);

    protected override DbCommand CreateDbCommand() => new FaultInjectingDbCommand(_connection.CreateCommand(), this, _injector);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection.Dispose();

        base.Dispose(disposing);
    }

    private readonly DbConnection _connection;
    private readonly FaultInjector _injector;
}

internal sealed class FaultInjectingDbCommand : DbCommand
{
    public FaultInjectingDbCommand(DbCommand command, DbConnection connection, FaultInjector injector)
    {
        _command = command;
        _connection = connection;
        _injector = injector;
    }

    [AllowNull]
    public override string CommandText
    {
        get => _command.CommandText;
        set => _command.CommandText = value;
    }

    public override int CommandTimeout
    {
        get => _command.CommandTimeout;
        set => _command.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => _command.CommandType;
        set => _command.CommandType = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => _command.UpdatedRowSource;
        set => _command.UpdatedRowSource = value;
    }

    public override bool DesignTimeVisible
    {
        get => _command.DesignTimeVisible;
        set => _command.DesignTimeVisible = value;
    }

    // the wrapped command remains bound to the wrapped connection, so only the visible association changes
    protected override DbConnection DbConnection
    {
        get => _connection;
        set { }
    }

    protected override DbParameterCollection DbParameterCollection => _command.Parameters;

    protected override DbTransaction DbTransaction
    {
        get => _command.Transaction;
        set => _command.Transaction = value;
    }

    public override void Cancel() => _command.Cancel();

    public override void Prepare() => _command.Prepare();

    public override int ExecuteNonQuery() => _command.ExecuteNonQuery();

    public override object ExecuteScalar() => _command.ExecuteScalar();

    protected override DbParameter CreateDbParameter() => _command.CreateParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        var shouldFail = _injector.BeginExecution();
        return new FaultInjectingDbDataReader(_command.ExecuteReader(behavior), shouldFail ? _injector.RowsBeforeFailure : null);
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        var shouldFail = _injector.BeginExecution();
        var reader = await _command.ExecuteReaderAsync(behavior, cancellationToken);

        return new FaultInjectingDbDataReader(reader, shouldFail ? _injector.RowsBeforeFailure : null);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _command.Dispose();

        base.Dispose(disposing);
    }

    private readonly DbCommand _command;
    private readonly DbConnection _connection;
    private readonly FaultInjector _injector;
}

internal sealed class FaultInjectingDbDataReader : DbDataReader
{
    /// <param name="rowsBeforeFailure">The number of rows to return before failing, or <see langword="null" /> when this reader should not fail.</param>
    public FaultInjectingDbDataReader(DbDataReader reader, int? rowsBeforeFailure)
    {
        _reader = reader;
        _rowsBeforeFailure = rowsBeforeFailure;
    }

    public override bool Read()
    {
        ThrowWhenFailureReached();
        return ReadRow(_reader.Read());
    }

    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        ThrowWhenFailureReached();
        return ReadRow(await _reader.ReadAsync(cancellationToken));
    }

    private void ThrowWhenFailureReached()
    {
        if (_rowsRead == _rowsBeforeFailure)
            throw new TimeoutException("A transient failure occurred while reading results.");
    }

    private bool ReadRow(bool hasRow)
    {
        if (hasRow)
            _rowsRead++;

        return hasRow;
    }

    public override object this[int ordinal] => _reader[ordinal];

    public override object this[string name] => _reader[name];

    public override int Depth => _reader.Depth;

    public override int FieldCount => _reader.FieldCount;

    public override bool HasRows => _reader.HasRows;

    public override bool IsClosed => _reader.IsClosed;

    public override int RecordsAffected => _reader.RecordsAffected;

    public override bool GetBoolean(int ordinal) => _reader.GetBoolean(ordinal);

    public override byte GetByte(int ordinal) => _reader.GetByte(ordinal);

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => _reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => _reader.GetChar(ordinal);

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => _reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

    public override string GetDataTypeName(int ordinal) => _reader.GetDataTypeName(ordinal);

    public override DateTime GetDateTime(int ordinal) => _reader.GetDateTime(ordinal);

    public override decimal GetDecimal(int ordinal) => _reader.GetDecimal(ordinal);

    public override double GetDouble(int ordinal) => _reader.GetDouble(ordinal);

    public override Type GetFieldType(int ordinal) => _reader.GetFieldType(ordinal);

    public override T GetFieldValue<T>(int ordinal) => _reader.GetFieldValue<T>(ordinal);

    public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) => _reader.GetFieldValueAsync<T>(ordinal, cancellationToken);

    public override float GetFloat(int ordinal) => _reader.GetFloat(ordinal);

    public override Guid GetGuid(int ordinal) => _reader.GetGuid(ordinal);

    public override short GetInt16(int ordinal) => _reader.GetInt16(ordinal);

    public override int GetInt32(int ordinal) => _reader.GetInt32(ordinal);

    public override long GetInt64(int ordinal) => _reader.GetInt64(ordinal);

    public override string GetName(int ordinal) => _reader.GetName(ordinal);

    public override int GetOrdinal(string name) => _reader.GetOrdinal(name);

    public override string GetString(int ordinal) => _reader.GetString(ordinal);

    public override object GetValue(int ordinal) => _reader.GetValue(ordinal);

    public override int GetValues(object[] values) => _reader.GetValues(values);

    public override bool IsDBNull(int ordinal) => _reader.IsDBNull(ordinal);

    public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => _reader.IsDBNullAsync(ordinal, cancellationToken);

    public override bool NextResult() => _reader.NextResult();

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => _reader.NextResultAsync(cancellationToken);

    public override IEnumerator GetEnumerator() => _reader.GetEnumerator();

    public override void Close() => _reader.Close();

    public override Task CloseAsync() => _reader.CloseAsync();

    public override DataTable GetSchemaTable() => _reader.GetSchemaTable();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _reader.Dispose();

        base.Dispose(disposing);
    }

    public override ValueTask DisposeAsync() => _reader.DisposeAsync();

    private readonly DbDataReader _reader;
    private readonly int? _rowsBeforeFailure;
    private int _rowsRead;
}
