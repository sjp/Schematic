using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

internal static class Config
{
    public static IDbConnectionFactory ConnectionFactory => !ConnectionString.IsNullOrWhiteSpace()
        ? new MySqlConnectionFactory(ConnectionString)
        : null;

    public static ISchematicConnection SchematicConnection => new SchematicConnection(
        ConnectionFactory,
        new MySqlDialect()
    );

    private static string ConnectionString => Configuration.GetConnectionString("MySql_TestDb");

    private static IConfigurationRoot Configuration => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("mysql-test.config.json", optional: true)
        .Build();
}

[Category("MySqlDatabase")]
[DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No MySQL DB available")]
[Parallelizable(ParallelScope.Children)]
internal abstract class MySqlTest
{
    protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected MySqlDatabaseProvider DatabaseProvider { get; } = new(Config.SchematicConnection);

    protected IIdentifierDefaults IdentifierDefaults { get; } = new MySqlDatabaseProvider(Config.SchematicConnection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

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