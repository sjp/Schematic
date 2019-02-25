using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? MySqlDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("mysql-test.config.json")
            .AddJsonFile("mysql-test.local.config.json", optional: true)
            .Build();
    }

    [Category("MySqlDatabase")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.Connection), "No MySQL DB available")]
    internal abstract class MySqlTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new MySqlDialect(Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new MySqlDialect(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
