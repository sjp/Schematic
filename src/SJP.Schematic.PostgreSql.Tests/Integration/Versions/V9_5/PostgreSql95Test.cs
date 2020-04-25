using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_5
{
    internal static class Config95
    {
        public static IDbConnectionFactory ConnectionFactory => new PostgreSqlConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new PostgreSqlDialect());

        private static string ConnectionString => Configuration.GetConnectionString("PostgreSql_TestDb_95");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("postgresql-test.config.json")
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config95), nameof(Config95.ConnectionFactory), "No PostgreSQL v9.5 DB available")]
    internal abstract class PostgreSql95Test
    {
        protected ISchematicConnection Connection { get; } = Config95.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config95.SchematicConnection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
