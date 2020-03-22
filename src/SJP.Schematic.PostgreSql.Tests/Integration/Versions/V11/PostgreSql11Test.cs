using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V11
{
    internal static class Config11
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
            .AddJsonFile("postgresql-test-11.config.json")
            .AddJsonFile("postgresql-test-11.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config11), nameof(Config11.Connection), "No PostgreSQL v11 DB available")]
    internal abstract class PostgreSql11Test
    {
        protected ISchematicConnection Connection { get; } = Config11.SchematicConnection;

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config11.SchematicConnection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
