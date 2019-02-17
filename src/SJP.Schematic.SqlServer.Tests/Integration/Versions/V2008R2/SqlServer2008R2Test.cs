using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2008R2
{
    internal static class Config2008R2
    {
        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? SqlServerDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult()
            : null)
            .Match(c => c, _ => null);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2008R2.config.json")
            .AddJsonFile("sqlserver-test-2008R2.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [DatabaseTestFixture(typeof(Config2008R2), nameof(Config2008R2.Connection), "No SQL Server 2008R2 DB available")]
    internal abstract class SqlServer2008R2Test
    {
        protected IDbConnection Connection { get; } = Config2008R2.Connection;

        protected ISqlServerDialect Dialect { get; } = new SqlServerDialect(Config2008R2.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqlServerDialect(Config2008R2.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
