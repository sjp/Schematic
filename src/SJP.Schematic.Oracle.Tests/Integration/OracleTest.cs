using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = OracleDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("oracle-test.config.json")
            .AddJsonFile("oracle-test.local.config.json", optional: true)
            .Build();
    }

    [Category("OracleDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture(Ignore = "No CI Oracle DB available")]
    internal abstract class OracleTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new OracleDialect(Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new OracleDialect(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultOracleIdentifierResolutionStrategy();
    }
}
