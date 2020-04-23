using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V12
{
    internal static class Config12
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new PostgreSqlConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new PostgreSqlDialect());

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test-12.config.json")
            .AddJsonFile("postgresql-test-12.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config12), nameof(Config12.ConnectionFactory), "No PostgreSQL v12 DB available")]
    internal abstract class PostgreSql12Test
    {
        protected ISchematicConnection Connection { get; } = Config12.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config12.SchematicConnection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
