using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory => new PostgreSqlConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new PostgreSqlDialect());

        private static string ConnectionString => Configuration.GetConnectionString("PostgreSql_TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("postgresql-test.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No PostgreSQL DB available")]
    internal abstract class PostgreSqlTest
    {
        protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
