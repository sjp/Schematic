using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection { get; } = new PostgreSqlDialect().CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("postgresql-test.json.config")
            .AddJsonFile("postgresql-test.json.config.local", optional: true)
            .Build();
    }

    [Category("PostgreSqlDatabase")]
    [Category("SkipWhenLiveUnitTesting")]
    [TestFixture]
    internal abstract class PostgreSqlTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new PostgreSqlDialect();
    }
}
