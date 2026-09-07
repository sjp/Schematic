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

namespace SJP.Schematic.Oracle.Tests.Integration;

internal static class Config
{
    public static IDbConnectionFactory ConnectionFactory => ConnectionFactoryLoader.Value;

    public static ISchematicConnection SchematicConnection => new SchematicConnection(
        ConnectionFactory,
        new OracleDialect()
    );

    private static readonly Lazy<IDbConnectionFactory> ConnectionFactoryLoader = new(static () => !ConnectionString.IsNullOrWhiteSpace()
        ? new OracleConnectionFactory(ConnectionString)
        : null);

    private static string ConnectionString => ConnectionStringLoader.Value;

    private static readonly Lazy<string> ConnectionStringLoader = new(static () => Configuration.GetConnectionString("Oracle_TestDb"));

    private static IConfigurationRoot Configuration => ConfigurationLoader.Value;

    private static readonly Lazy<IConfigurationRoot> ConfigurationLoader = new(static () => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("oracle-test.config.json", optional: true)
        .Build());
}

/// <summary>
/// Probes for a live Oracle instance once per run, ignoring every fixture in this namespace when
/// one is not reachable, and resolves the values shared by every fixture beneath it. Identifier
/// defaults cannot change within a run, so they are awaited here once rather than being resolved
/// -- blocking -- once per fixture.
/// </summary>
[SetUpFixture]
internal sealed class OracleIntegrationSetUp
{
    public static ISchematicConnection Connection { get; private set; } = null!;

    public static OracleDatabaseProvider DatabaseProvider { get; private set; } = null!;

    public static IIdentifierDefaults IdentifierDefaults { get; private set; } = null!;

    public static Version DatabaseVersion { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task InitAsync()
    {
        DatabaseAvailability.EnsureAvailable(static () => Config.ConnectionFactory, "No Oracle DB available");

        Connection = Config.SchematicConnection;
        DatabaseProvider = new OracleDatabaseProvider(Connection);
        IdentifierDefaults = await DatabaseProvider.GetIdentifierDefaultsAsync(TestContext.CurrentContext.CancellationToken);
        DatabaseVersion = await DatabaseProvider.GetDatabaseVersionAsync(TestContext.CurrentContext.CancellationToken);
    }
}

[Category("OracleDatabase")]
[Category("SkipWhenLiveUnitTesting")]
[Parallelizable(ParallelScope.Children)]
// A deadline, not a performance budget: generous enough that a slow CI image never trips it, but
// tight enough that a wedged connection fails the test rather than holding the job open until the
// CI timeout. Cooperative -- it only bites where the context's cancellation token is threaded
// through to the database call.
[CancelAfter(5 * 60 * 1000)]
internal abstract class OracleTest
{
    protected ISchematicConnection Connection => OracleIntegrationSetUp.Connection;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected OracleDatabaseProvider DatabaseProvider => OracleIntegrationSetUp.DatabaseProvider;

    protected IIdentifierDefaults IdentifierDefaults => OracleIntegrationSetUp.IdentifierDefaults;

    protected Version DatabaseVersion => OracleIntegrationSetUp.DatabaseVersion;

    protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultOracleIdentifierResolutionStrategy();

    /// <summary>
    /// Executes multiple DDL statements as a single round-trip. Oracle does not allow DDL directly
    /// inside a PL/SQL block, so each statement is wrapped in <c>execute immediate</c> within an
    /// anonymous block -- <c>create view</c>, <c>procedure</c>, <c>function</c> and <c>trigger</c>
    /// statements contain their own embedded quotes and statement terminators, so keep those as
    /// individual <see cref="DbConnection"/> calls.
    /// </summary>
    protected Task ExecuteBatchAsync(params string[] statements)
    {
        var body = string.Join('\n', statements.Select(static s => $"execute immediate '{s.Replace("'", "''")}';"));
        return DbConnection.ExecuteAsync($"begin\n{body}\nend;", TestContext.CurrentContext.CancellationToken);
    }

    /// <summary>
    /// Drops multiple tables in a single round-trip. Table names are dropped in the order given,
    /// so pass them in dependency order (children before parents) exactly as with individual drops.
    /// </summary>
    protected Task DropTablesAsync(params string[] tableNames) =>
        ExecuteBatchAsync([.. tableNames.Select(static t => "drop table " + t)]);
}