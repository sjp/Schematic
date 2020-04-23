using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2016
{
    internal static class Config2016
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new SqlServerConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(
            ConnectionFactory,
            new SqlServerDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2016.config.json")
            .AddJsonFile("sqlserver-test-2016.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config2016), nameof(Config2016.ConnectionFactory), "No SQL Server 2016 DB available")]
    internal abstract class SqlServer2016Test
    {
        protected ISchematicConnection Connection { get; } = Config2016.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected ISqlServerDialect Dialect => Connection.Dialect as ISqlServerDialect;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config2016.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();
    }
}
