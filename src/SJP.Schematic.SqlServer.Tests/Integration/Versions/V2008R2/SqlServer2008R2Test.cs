using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2008R2
{
    internal static class Config2008R2
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new SqlServerConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(
            ConnectionFactory,
            new SqlServerDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2008R2.config.json")
            .AddJsonFile("sqlserver-test-2008R2.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config2008R2), nameof(Config2008R2.ConnectionFactory), "No SQL Server 2008R2 DB available")]
    internal abstract class SqlServer2008R2Test
    {
        protected ISchematicConnection Connection { get; } = Config2008R2.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected ISqlServerDialect Dialect => Connection.Dialect as ISqlServerDialect;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config2008R2.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();
    }
}
