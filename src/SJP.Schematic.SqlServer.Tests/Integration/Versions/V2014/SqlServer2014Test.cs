using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2014
{
    internal static class Config2014
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? SqlServerDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2014.config.json")
            .AddJsonFile("sqlserver-test-2014.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config2014), nameof(Config2014.Connection), "No SQL Server 2014 DB available")]
    internal abstract class SqlServer2014Test
    {
        protected IDbConnection Connection { get; } = Config2014.Connection;

        protected ISqlServerDialect Dialect { get; } = new SqlServerDialect();

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqlServerDialect().GetIdentifierDefaultsAsync(Config2014.Connection).GetAwaiter().GetResult();
    }
}
