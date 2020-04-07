using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V12
{
    internal static class Config12
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
            .AddJsonFile("postgresql-test-12.config.json")
            .AddJsonFile("postgresql-test-12.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config12), nameof(Config12.Connection), "No PostgreSQL v12 DB available")]
    internal abstract class PostgreSql12Test
    {
        protected ISchematicConnection Connection { get; } = Config12.SchematicConnection;

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config12.SchematicConnection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
