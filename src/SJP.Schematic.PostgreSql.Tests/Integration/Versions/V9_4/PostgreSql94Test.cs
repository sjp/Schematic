using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_4
{
    internal static class Config94
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? PostgreSqlDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test-94.config.json")
            .AddJsonFile("postgresql-test-94.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [DatabaseTestFixture(typeof(Config94), nameof(Config94.Connection), "No PostgreSQL v9.4 DB available")]
    internal abstract class PostgreSql94Test
    {
        protected IDbConnection Connection { get; } = Config94.Connection;

        protected IDatabaseDialect Dialect { get; } = new PostgreSqlDialect(Config94.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new PostgreSqlDialect(Config94.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
