using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? PostgreSqlDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test.config.json")
            .AddJsonFile("postgresql-test.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.Connection), "No PostgreSQL DB available")]
    internal abstract class PostgreSqlTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new PostgreSqlDialect(Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new PostgreSqlDialect(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
