using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? OracleDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("oracle-test.config.json")
            .AddJsonFile("oracle-test.local.config.json", optional: true)
            .Build();
    }

    [Category("OracleDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.Connection), "No Oracle DB available")]
    internal abstract class OracleTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new OracleDialect(Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new OracleDialect(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultOracleIdentifierResolutionStrategy();
    }
}
