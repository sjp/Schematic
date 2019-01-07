using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2008R2
{
    internal static class Config2008R2
    {
        public static IDbConnection Connection { get; } = SqlServerDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2008R2.json.config")
            .AddJsonFile("sqlserver-test-2008R2.json.config.local", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture]
    internal abstract class SqlServer2008R2Test
    {
        protected IDbConnection Connection { get; } = Config2008R2.Connection;

        protected IDatabaseDialect Dialect { get; } = new SqlServerDialect(Config2008R2.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqlServerDialect(Config2008R2.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();
    }
}
