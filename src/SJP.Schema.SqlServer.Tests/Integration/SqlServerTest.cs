using System.Data;
using NUnit.Framework;
using SJP.Schema.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schema.SqlServer.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection => SqlServerDialect.Instance.CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test.json.config")
            .AddJsonFile("sqlserver-test.json.config.local", optional: true)
            .Build();
    }

    internal sealed class DatabaseDependentAttribute : CategoryAttribute
    {
        public DatabaseDependentAttribute()
            : base("Database")
        {
        }
    }

    [DatabaseDependent]
    internal abstract class SqlServerTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = SqlServerDialect.Instance;
    }
}
