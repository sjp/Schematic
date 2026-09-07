using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

// Caps how many tests run concurrently under [Parallelizable(ParallelScope.Children)], so the DB
// service container's max_connections isn't exhausted regardless of the CI runner's core count.
[assembly: LevelOfParallelism(4)]

namespace SJP.Schematic.MySql.Tests.Integration;

internal static class Config
{
    public static IDbConnectionFactory ConnectionFactory => ConnectionFactoryLoader.Value;

    public static ISchematicConnection SchematicConnection => new SchematicConnection(
        ConnectionFactory,
        new MySqlDialect()
    );

    private static readonly Lazy<IDbConnectionFactory> ConnectionFactoryLoader = new(static () =>
    {
        if (ConnectionString.IsNullOrWhiteSpace())
            return null;

        var builder = new MySqlConnectionStringBuilder(ConnectionString)
        {
            MaximumPoolSize = 30
        };

        return new MySqlConnectionFactory(builder.ConnectionString);
    });

    private static string ConnectionString => ConnectionStringLoader.Value;

    private static readonly Lazy<string> ConnectionStringLoader = new(static () => Configuration.GetConnectionString("MySql_TestDb"));

    private static IConfigurationRoot Configuration => ConfigurationLoader.Value;

    private static readonly Lazy<IConfigurationRoot> ConfigurationLoader = new(static () => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("mysql-test.config.json", optional: true)
        .Build());
}

/// <summary>
/// Probes for a live MySQL instance once per run, ignoring every fixture in this namespace when
/// one is not reachable.
/// </summary>
[SetUpFixture]
internal sealed class MySqlIntegrationSetUp
{
    [OneTimeSetUp]
    public void ProbeDatabase() => DatabaseAvailability.EnsureAvailable(static () => Config.ConnectionFactory, "No MySQL DB available");
}

[Category("MySqlDatabase")]
[Category("SkipWhenLiveUnitTesting")]
[Parallelizable(ParallelScope.Children)]
internal abstract class MySqlTest
{
    protected ISchematicConnection Connection => _connection.Value;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected MySqlDatabaseProvider DatabaseProvider => _databaseProvider.Value;

    protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

    protected MySqlTest()
    {
        _connection = new Lazy<ISchematicConnection>(() => Config.SchematicConnection);
        _databaseProvider = new Lazy<MySqlDatabaseProvider>(() => new MySqlDatabaseProvider(Connection));
        _defaults = new Lazy<IIdentifierDefaults>(() => new MySqlDatabaseProvider(Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult());
    }

    private readonly Lazy<ISchematicConnection> _connection;
    private readonly Lazy<MySqlDatabaseProvider> _databaseProvider;
    private readonly Lazy<IIdentifierDefaults> _defaults;

    /// <summary>
    /// Executes multiple DDL statements as a single round-trip. MySqlConnector natively supports
    /// multi-statement command text, so any mix of statements can be batched together.
    /// </summary>
    protected Task ExecuteBatchAsync(params string[] statements) =>
        DbConnection.ExecuteAsync(string.Join(";\n", statements), CancellationToken.None);

    /// <summary>
    /// Drops multiple tables in a single round-trip. Table names are dropped in the order given,
    /// so pass them in dependency order (children before parents) exactly as with individual drops.
    /// </summary>
    protected Task DropTablesAsync(params string[] tableNames) =>
        ExecuteBatchAsync([.. tableNames.Select(static t => "drop table " + t)]);
}