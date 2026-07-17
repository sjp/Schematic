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
internal abstract class MySqlTest
{
    protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected MySqlDatabaseProvider DatabaseProvider { get; } = new(Config.SchematicConnection);

    protected IIdentifierDefaults IdentifierDefaults { get; } = new MySqlDatabaseProvider(Config.SchematicConnection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
}