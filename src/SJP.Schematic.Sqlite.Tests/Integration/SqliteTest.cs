using System.Data;
using NUnit.Framework;
using SJP.Schematic.Core;
using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection => new SqliteDialect().CreateConnection(ConnectionString);

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlite-test.json.config")
            .AddJsonFile("sqlite-test.json.config.local", optional: true)
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

        protected IDatabaseDialect Dialect { get; } = new SqliteDialect();
    }
}
