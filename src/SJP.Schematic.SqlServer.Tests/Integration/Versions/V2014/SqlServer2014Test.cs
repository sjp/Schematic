using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2014
{
    internal static class Config2014
    {
        public static IDbConnection Connection { get; } = SqlServerDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2014.config.json")
            .AddJsonFile("sqlserver-test-2014.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture]
    internal abstract class SqlServer2014Test
    {
        protected IDbConnection Connection { get; } = Config2014.Connection;

        protected ISqlServerDialect Dialect { get; } = new SqlServerDialect(Config2014.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqlServerDialect(Config2014.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
