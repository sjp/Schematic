using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new SqlServerConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new SqlServerDialect());

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test.config.json")
            .AddJsonFile("sqlserver-test.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No SQL Server DB available")]
    internal abstract class SqlServerTest
    {
        protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();
    }
}
