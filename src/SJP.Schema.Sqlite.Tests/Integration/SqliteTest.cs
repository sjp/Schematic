using System.Data;
using NUnit.Framework;
using SJP.Schema.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schema.Sqlite.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection => SqliteDialect.Instance.CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlite-test.json.config")
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
    internal abstract class SqliteTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = SqliteDialect.Instance;
    }
}
