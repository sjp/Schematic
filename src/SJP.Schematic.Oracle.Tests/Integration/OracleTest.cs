using System;
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

    [OneTimeSetUp]
    public async Task InitAsync()
    {
        DatabaseAvailability.EnsureAvailable(static () => Config.ConnectionFactory, "No Oracle DB available");

        Connection = Config.SchematicConnection;
        DatabaseProvider = new OracleDatabaseProvider(Connection);
        IdentifierDefaults = await DatabaseProvider.GetIdentifierDefaultsAsync(TestContext.CurrentContext.CancellationToken);
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

    protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultOracleIdentifierResolutionStrategy();
}