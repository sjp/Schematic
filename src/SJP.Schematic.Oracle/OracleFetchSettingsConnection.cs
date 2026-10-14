using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SJP.Schematic.Oracle;

/// <summary>
/// An Oracle connection whose commands fetch many rows per round trip and retrieve <c>LONG</c> column data
/// together with the rest of the row.
/// </summary>
/// <remarks>
/// <para>
/// The number of rows fetched per round trip is <see cref="OracleCommand.FetchSize"/> divided by the declared
/// width of a row, not its actual width. Source text is read from the catalog one line per row, and a line is
/// declared as <c>VARCHAR2(4000)</c> even though most lines are far shorter, so the default fetch size of 256 KB
/// returns only about 64 lines per round trip. A 3,300-line package body then takes about 50 round trips. At
/// 2 MB it takes 7, and other large catalog reads need fewer round trips too. The fetch size is an upper bound,
/// not a buffer allocated up front: reading a single row allocates the same at every fetch size.
/// </para>
/// <para>
/// How much <c>LONG</c> and <c>LONG RAW</c> data arrives alongside a row is decided by
/// <see cref="OracleCommand.InitialLONGFetchSize"/>, which defaults to zero. That default leaves the data
/// behind on the server, to be requested separately while the reader is still positioned on the row.
/// </para>
/// <para>
/// The Oracle catalog exposes column default expressions, view and materialized view queries, trigger bodies
/// and check constraint definitions as <c>LONG</c> columns, and every one of them is read in full as soon as
/// its row is read. Deferring them therefore buys nothing and can only cost extra round trips, so commands
/// created here ask for the whole value up front instead.
/// </para>
/// <para>
/// Dapper already sets <see cref="OracleCommand.InitialLONGFetchSize"/> to the same value on every command it
/// runs, so that assignment only matters for commands not run through Dapper.
/// </para>
/// <para>
/// Both settings exist only on individual commands, so they have to be applied wherever commands are created.
/// Wrapping the connection puts that in one place, covering queries whose commands are built by a data access
/// library rather than by this library directly.
/// </para>
/// </remarks>
internal sealed class OracleFetchSettingsConnection : DbConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleFetchSettingsConnection"/> class.
    /// </summary>
    /// <param name="connection">The connection that performs the work.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public OracleFetchSettingsConnection(OracleConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        _connection = connection;
        _connection.StateChange += OnInnerStateChange;
    }

    [AllowNull]
    public override string ConnectionString
    {
        get => _connection.ConnectionString;
        set => _connection.ConnectionString = value;
    }

    public override int ConnectionTimeout => _connection.ConnectionTimeout;

    public override string Database => _connection.Database;

    public override string DataSource => _connection.DataSource;

    public override string ServerVersion => _connection.ServerVersion;

    public override ConnectionState State => _connection.State;

    public override void ChangeDatabase(string databaseName) => _connection.ChangeDatabase(databaseName);

    public override void Close() => _connection.Close();

    public override Task CloseAsync() => _connection.CloseAsync();

    public override void Open() => _connection.Open();

    public override Task OpenAsync(CancellationToken cancellationToken) => _connection.OpenAsync(cancellationToken);

    public override void EnlistTransaction(System.Transactions.Transaction? transaction) => _connection.EnlistTransaction(transaction);

    public override DataTable GetSchema() => _connection.GetSchema();

    public override DataTable GetSchema(string collectionName) => _connection.GetSchema(collectionName);

    public override DataTable GetSchema(string collectionName, string?[] restrictionValues) => _connection.GetSchema(collectionName, restrictionValues);

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => _connection.BeginTransaction(isolationLevel);

    /// <summary>
    /// Creates a command that fetches many rows per round trip and retrieves <c>LONG</c> data with the row it
    /// belongs to.
    /// </summary>
    /// <returns>A command bound to the underlying connection.</returns>
    /// <remarks>
    /// The command is returned as created by the underlying connection, still bound to it, so that nothing
    /// further has to be intercepted once the command is handed out.
    /// </remarks>
    protected override DbCommand CreateDbCommand()
    {
        var command = _connection.CreateCommand();
        command.FetchSize = FetchSizeBytes;
        command.InitialLONGFetchSize = EntireLongValue;

        return command;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.StateChange -= OnInnerStateChange;
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Asynchronously releases the resources used by this connection.
    /// </summary>
    /// <returns>A task that completes once the connection has been released.</returns>
    /// <remarks>
    /// The underlying connection is released asynchronously first; the base implementation then runs the
    /// synchronous disposal, whose second release of an already released connection does nothing.
    /// </remarks>
    public override async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Republishes the underlying connection's state changes as this connection's own.
    /// </summary>
    private void OnInnerStateChange(object sender, StateChangeEventArgs e) => OnStateChange(e);

    /// <summary>
    /// The <see cref="OracleCommand.InitialLONGFetchSize"/> that retrieves a <c>LONG</c> value in its entirety.
    /// </summary>
    private const int EntireLongValue = -1;

    /// <summary>
    /// The <see cref="OracleCommand.FetchSize"/>, in bytes, eight times the driver default of 256 KB.
    /// </summary>
    private const long FetchSizeBytes = 2 * 1024 * 1024;

    private readonly OracleConnection _connection;
}
