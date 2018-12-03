using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = new MySqlDialect().CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("mysql-test.json.config")
            .AddJsonFile("mysql-test.json.config.local", optional: true)
            .Build();
    }

    [Category("MySqlDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture]
    internal abstract class MySqlTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new MySqlDialect();

        protected IIdentifierDefaults IdentifierDefaults { get; } = new MySqlDialect().GetIdentifierDefaults(Config.Connection);
    }
}
