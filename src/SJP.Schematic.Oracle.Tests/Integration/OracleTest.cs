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
    public static IDbConnectionFactory ConnectionFactory => !ConnectionString.IsNullOrWhiteSpace()
        ? new OracleConnectionFactory(ConnectionString)
        : null;

    public static ISchematicConnection SchematicConnection => new SchematicConnection(
        ConnectionFactory,
        new OracleDialect()
    );

    private static string ConnectionString => Configuration.GetConnectionString("Oracle_TestDb");

    private static IConfigurationRoot Configuration => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("oracle-test.config.json", optional: true)
        .Build();
}

/// <summary>
/// Probes for a live Oracle instance once per run, ignoring every fixture in this namespace when
/// one is not reachable.
/// </summary>
[SetUpFixture]
internal sealed class OracleIntegrationSetUp
{
    [OneTimeSetUp]
    public void ProbeDatabase() => DatabaseAvailability.EnsureAvailable(static () => Config.ConnectionFactory, "No Oracle DB available");
}

[Category("OracleDatabase")]
[Category("SkipWhenLiveUnitTesting")]
[Parallelizable(ParallelScope.Children)]
internal abstract class OracleTest
{
    protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected OracleDatabaseProvider DatabaseProvider { get; } = new(Config.SchematicConnection);

    protected IIdentifierDefaults IdentifierDefaults { get; } = new OracleDatabaseProvider(Config.SchematicConnection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

    protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultOracleIdentifierResolutionStrategy();
}