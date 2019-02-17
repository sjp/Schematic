using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_6
{
    internal static class Config96
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? PostgreSqlDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test-96.config.json")
            .AddJsonFile("postgresql-test-96.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [DatabaseTestFixture(typeof(Config96), nameof(Config96.Connection), "No PostgreSQL v9.6 DB available")]
    internal abstract class PostgreSql96Test
    {
        protected IDbConnection Connection { get; } = Config96.Connection;

        protected IDatabaseDialect Dialect { get; } = new PostgreSqlDialect(Config96.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new PostgreSqlDialect(Config96.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
