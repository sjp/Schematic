using System.Data;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection => SqliteDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();

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
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new SqliteDialect(Config.Connection);

        protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(new SqliteDialect(Config.Connection), Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
