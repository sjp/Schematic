using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory => new MySqlConnectionFactory(ConnectionString);

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

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();
    }
}
