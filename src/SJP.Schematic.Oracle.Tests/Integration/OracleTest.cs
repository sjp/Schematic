using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = new OracleDialect().CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("oracle-test.json.config")
            .AddJsonFile("oracle-test.json.config.local", optional: true)
            .Build();
    }

    [Category("OracleDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture(Ignore = "No CI Oracle DB available")]
    internal abstract class OracleTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new OracleDialect();

        protected IIdentifierDefaults IdentifierDefaults { get; } = new OracleDialect().GetIdentifierDefaults(Config.Connection);

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultOracleIdentifierResolutionStrategy();
    }
}
