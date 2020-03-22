using System.Data;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new SqliteConnectionFactory();

        public static IDbConnection DbConnection { get; } = ConnectionFactory.CreateConnection(ConnectionString);

        public static ISchematicConnection Connection { get; } = new SchematicConnection(DbConnection, new SqliteDialect());

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlite-test.config.json")
            .AddJsonFile("sqlite-test.local.config.json", optional: true)
            .Build();
    }

    [Category("SqliteDatabase")]
    [TestFixture]
    internal abstract class SqliteTest
    {
        protected ISchematicConnection Connection { get; } = Config.Connection;

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect().GetIdentifierDefaultsAsync(Config.Connection).GetAwaiter().GetResult();

        protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(Config.Connection);

        protected ISqliteDatabase GetSqliteDatabase() => new SqliteRelationalDatabase(Config.Connection, IdentifierDefaults);
    }
}
