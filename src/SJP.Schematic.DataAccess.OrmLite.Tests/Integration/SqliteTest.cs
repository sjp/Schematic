using System.Data;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.OrmLite.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new SqliteConnectionFactory();

        public static ISchematicConnection Connection { get; } = new SchematicConnection(
            ConnectionFactory.CreateConnection(ConnectionString),
            new SqliteDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlite-test.config.json")
            .AddJsonFile("sqlite-test.local.config.json", optional: true)
            .Build();
    }

    [TestFixture]
    internal abstract class SqliteTest
    {
        protected ISchematicConnection Connection { get; } = Config.Connection;

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect().GetIdentifierDefaultsAsync(Config.Connection).GetAwaiter().GetResult();
    }
}
