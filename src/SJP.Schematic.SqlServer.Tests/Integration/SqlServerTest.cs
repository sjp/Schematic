using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? SqlServerDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test.config.json")
            .AddJsonFile("sqlserver-test.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.Connection), "No SQL Server DB available")]
    internal abstract class SqlServerTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new SqlServerDialect(Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqlServerDialect(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
