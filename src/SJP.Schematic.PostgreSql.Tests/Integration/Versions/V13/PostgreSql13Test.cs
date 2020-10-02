using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V13
{
    internal static class Config13
    {
        public static IDbConnectionFactory ConnectionFactory => !ConnectionString.IsNullOrWhiteSpace()
            ? new PostgreSqlConnectionFactory(ConnectionString)
            : null;

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new PostgreSqlDialect());

        private static string ConnectionString => Configuration.GetConnectionString("PostgreSql_TestDb_13");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("postgresql-test.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config13), nameof(Config13.ConnectionFactory), "No PostgreSQL v13 DB available")]
    internal abstract class PostgreSql13Test
    {
        protected ISchematicConnection Connection { get; } = Config13.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config13.SchematicConnection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
