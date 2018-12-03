using System.Data;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection => new SqliteDialect().CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlite-test.json.config")
            .AddJsonFile("sqlite-test.json.config.local", optional: true)
            .Build();
    }

    [Category("SqliteDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture]
    internal abstract class SqliteTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new SqliteDialect();

        protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(new SqliteDialect(), Config.Connection);

        protected IDatabaseIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect().GetIdentifierDefaults(Config.Connection);
    }
}
