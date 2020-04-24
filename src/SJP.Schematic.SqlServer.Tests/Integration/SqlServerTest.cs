using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory => new SqlServerConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new SqlServerDialect());

        private static string ConnectionString => Configuration.GetConnectionString("SqlServer_TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("sqlserver-test.config.json")
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No SQL Server DB available")]
    internal abstract class SqlServerTest
    {
        protected ISchematicConnection Connection => Config.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

        private readonly Lazy<IIdentifierDefaults> _defaults = new Lazy<IIdentifierDefaults>(() => Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult());
    }
}
