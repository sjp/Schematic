using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_5
{
    internal static class Config95
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new PostgreSqlConnectionFactory();

        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? ConnectionFactory.CreateConnection(ConnectionString)
            : null)
            .Match(c => c, _ => null);

        public static ISchematicConnection SchematicConnection
        {
            get
            {
                var connection = new SchematicConnection(Connection, new PostgreSqlDialect());
                connection.SetMaxConcurrentQueries(1);
                return connection;
            }
        }

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test-95.config.json")
            .AddJsonFile("postgresql-test-95.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config95), nameof(Config95.Connection), "No PostgreSQL v9.5 DB available")]
    internal abstract class PostgreSql95Test
    {
        protected ISchematicConnection Connection { get; } = Config95.SchematicConnection;

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config95.SchematicConnection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
