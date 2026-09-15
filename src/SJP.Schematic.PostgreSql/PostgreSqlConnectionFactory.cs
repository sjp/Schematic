using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Polly;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A connection factory that provides PostgreSQL connections.
/// </summary>
/// <seealso cref="IDbConnectionFactory" />
public class PostgreSqlConnectionFactory : IDbConnectionFactory, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">
    /// <para>
    /// The connection string. Pool size, connection timeout and command timeout fall back to Npgsql's
    /// defaults when the connection string does not set them.
    /// </para>
    /// <para>
    /// Automatic statement preparation is the one exception: unless the connection string sets them,
    /// <c>Max Auto Prepare</c> defaults to <c>32</c> and <c>Auto Prepare Min Usages</c> to <c>2</c>, because
    /// schema loading runs the same catalog queries for every object and preparing them saves the server
    /// from planning each execution again. Set <c>Max Auto Prepare=0</c> to turn preparation off, for example
    /// when connecting through PgBouncer in transaction or statement pooling mode without
    /// <c>max_prepared_statements</c> configured.
    /// </para>
    /// </param>
    /// <param name="connectionConfiguration">
    /// An optional callback used to configure each <see cref="NpgsqlConnection"/> before it is opened.
    /// Use this to authenticate via a mechanism other than the connection string, e.g. from an
    /// environment variable or an external credential provider.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="connectionString"/> is empty or whitespace.</exception>
    public PostgreSqlConnectionFactory(string connectionString, Action<NpgsqlConnection>? connectionConfiguration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        ApplyAutoPrepareDefaults(dataSourceBuilder.ConnectionStringBuilder);

        DataSource = dataSourceBuilder.Build();
        ConnectionConfiguration = connectionConfiguration;
        MaxConcurrentQueries = dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize;
    }

    private static void ApplyAutoPrepareDefaults(NpgsqlConnectionStringBuilder builder)
    {
        // Npgsql rewrites every accepted spelling of a keyword to its canonical name, but its own
        // ContainsKey reports whether a keyword is known rather than whether it was set.
        var explicitSettings = new DbConnectionStringBuilder { ConnectionString = builder.ConnectionString };

        if (!explicitSettings.ContainsKey(MaxAutoPrepareKeyword))
            builder.MaxAutoPrepare = DefaultMaxAutoPrepare;
        if (!explicitSettings.ContainsKey(AutoPrepareMinUsagesKeyword))
            builder.AutoPrepareMinUsages = DefaultAutoPrepareMinUsages;
    }

    private const string MaxAutoPrepareKeyword = "Max Auto Prepare";
    private const string AutoPrepareMinUsagesKeyword = "Auto Prepare Min Usages";

    // More than the distinct catalog statements a single object load issues, while bounding the
    // plans each server connection keeps.
    private const int DefaultMaxAutoPrepare = 32;
    private const int DefaultAutoPrepareMinUsages = 2;

    /// <summary>
    /// Gets the database provider's connection factory.
    /// </summary>
    /// <value>The database provider connection factory.</value>
    protected NpgsqlDataSource DataSource { get; }

    /// <summary>
    /// Gets the optional callback used to configure each connection before it is opened.
    /// </summary>
    /// <value>A connection configuration callback, or <see langword="null" /> if none was provided.</value>
    protected Action<NpgsqlConnection>? ConnectionConfiguration { get; }

    /// <summary>
    /// Creates a database connection instance, but does not open the connection.
    /// </summary>
    /// <returns>An object representing a database connection.</returns>
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
        .Handle<PostgresException>(IsTransientError)
        .Or<TimeoutException>();

    /// <summary>
    /// Gets the maximum number of queries that may run concurrently against this factory.
    /// </summary>
    /// <value>The connection string's effective <c>Max Pool Size</c>.</value>
    public int MaxConcurrentQueries { get; }

    /// <summary>
    /// Disposes the connection pool (<see cref="NpgsqlDataSource"/>) dedicated to this factory
    /// instance, releasing any connections it is holding open.
    /// </summary>
    public void Dispose() => DataSource.Dispose();

    /// <summary>
    /// Disposes the connection pool (<see cref="NpgsqlDataSource"/>) dedicated to this factory
    /// instance, releasing any connections it is holding open.
    /// </summary>
    public ValueTask DisposeAsync() => DataSource.DisposeAsync();

    private static bool IsTransientError(PostgresException pgex)
    {
        switch (pgex.SqlState)
        {
            case "40001": // serialzation_failure
            case "53000": // insufficient_resources
            case "53100": // disk_full
            case "53200": // out_of_memory
            case "53300": // too_many_connections
            case "53400": // configuration_limit_exceeded
            case "57P03": // cannot_connect_now
            case "58000": // system_error
            case "58030": // io_error
            case "55P03": // lock_not_available
            case "55006": // object_in_use
            case "55000": // object_not_in_prerequisite_state
            case "08000": // connection_exception
            case "08003": // connection_does_not_exist
            case "08006": // connection_failure
            case "08001": // sqlclient_unable_to_establish_sqlconnection
            case "08004": // sqlserver_rejected_establishment_of_sqlconnection
            case "08007": // transaction_resolution_unknown
                return true;
            default:
                return false;
        }
    }
}