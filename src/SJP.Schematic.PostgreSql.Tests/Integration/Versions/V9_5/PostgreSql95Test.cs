using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_5
{
    internal static class Config95
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? PostgreSqlDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test-95.config.json")
            .AddJsonFile("postgresql-test-95.local.config.json", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [DatabaseTestFixture(typeof(Config95), nameof(Config95.Connection), "No PostgreSQL v9.5 DB available")]
    internal abstract class PostgreSql95Test
    {
        protected IDbConnection Connection { get; } = Config95.Connection;

        protected IDatabaseDialect Dialect { get; } = new PostgreSqlDialect();

        protected IIdentifierDefaults IdentifierDefaults { get; } = new PostgreSqlDialect().GetIdentifierDefaultsAsync(Config95.Connection).GetAwaiter().GetResult();

        protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();
    }
}
