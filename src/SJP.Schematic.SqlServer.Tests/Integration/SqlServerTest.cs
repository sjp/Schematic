using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

// Caps how many tests run concurrently under [Parallelizable(ParallelScope.Children)], so the DB
// service container's max_connections isn't exhausted regardless of the CI runner's core count.
[assembly: LevelOfParallelism(4)]

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal static class Config
{
    public static IDbConnectionFactory ConnectionFactory => ConnectionFactoryLoader.Value;

    public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new SqlServerDialect());

    private static readonly Lazy<IDbConnectionFactory> ConnectionFactoryLoader = new(static () => !ConnectionString.IsNullOrWhiteSpace()
        ? new SqlServerConnectionFactory(ConnectionString)
        : null);

    private static string ConnectionString => ConnectionStringLoader.Value;

    private static readonly Lazy<string> ConnectionStringLoader = new(static () => Configuration.GetConnectionString("SqlServer_TestDb"));

    private static IConfigurationRoot Configuration => ConfigurationLoader.Value;

    private static readonly Lazy<IConfigurationRoot> ConfigurationLoader = new(static () => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("sqlserver-test.config.json", optional: true)
        .Build());
}

/// <summary>
/// Probes for a live SQL Server instance once per run, ignoring every fixture in this namespace
/// when one is not reachable, and resolves the values shared by every fixture beneath it.
/// Identifier defaults cannot change within a run, so they are awaited here once rather than
/// being resolved -- blocking -- once per fixture.
/// </summary>
[SetUpFixture]
internal sealed class SqlServerIntegrationSetUp
{
    public static ISchematicConnection Connection { get; private set; } = null!;

    public static ISqlServerDatabaseProvider DatabaseProvider { get; private set; } = null!;

    public static IIdentifierDefaults IdentifierDefaults { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task InitAsync()
    {
        DatabaseAvailability.EnsureAvailable(static () => Config.ConnectionFactory, "No SQL Server DB available");

        Connection = Config.SchematicConnection;
        DatabaseProvider = new SqlServerDatabaseProvider(Connection);
        IdentifierDefaults = await DatabaseProvider.GetIdentifierDefaultsAsync(TestContext.CurrentContext.CancellationToken);
    }
}

[Category("SqlServerDatabase")]
[Category("SkipWhenLiveUnitTesting")]
[Parallelizable(ParallelScope.Children)]
// A deadline, not a performance budget: generous enough that a slow CI image never trips it, but
// tight enough that a wedged connection fails the test rather than holding the job open until the
// CI timeout. Cooperative -- it only bites where the context's cancellation token is threaded
// through to the database call.
[CancelAfter(2 * 60 * 1000)]
internal abstract class SqlServerTest
{
    protected ISchematicConnection Connection => SqlServerIntegrationSetUp.Connection;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected ISqlServerDatabaseProvider DatabaseProvider => SqlServerIntegrationSetUp.DatabaseProvider;

    protected IIdentifierDefaults IdentifierDefaults => SqlServerIntegrationSetUp.IdentifierDefaults;

    /// <summary>
    /// Executes multiple DDL statements as a single T-SQL batch, in one round-trip. Every
    /// statement must be able to appear alongside others in a batch -- <c>create view</c>,
    /// <c>procedure</c>, <c>function</c> and <c>trigger</c> must be the only statement in their
    /// own batch, so keep those as individual <see cref="DbConnection"/> calls.
    /// </summary>
    protected Task ExecuteBatchAsync(params string[] statements) =>
        DbConnection.ExecuteAsync(string.Join(";\n", statements), TestContext.CurrentContext.CancellationToken);

    /// <summary>
    /// Drops multiple tables in a single round-trip. Table names are dropped in the order given,
    /// so pass them in dependency order (children before parents) exactly as with individual drops.
    /// </summary>
    protected Task DropTablesAsync(params string[] tableNames) =>
        ExecuteBatchAsync([.. tableNames.Select(static t => "drop table " + t)]);
}