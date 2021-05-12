using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory => !ConnectionString.IsNullOrWhiteSpace()
            ? new SqlServerConnectionFactory(ConnectionString)
            : null;

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new SqlServerDialect());

        private static string ConnectionString => Configuration.GetConnectionString("SqlServer_TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("sqlserver-test.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No SQL Server DB available")]
    internal abstract class SqlServerTest
    {
        protected ISchematicConnection Connection => _connection.Value;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

        private readonly Lazy<ISchematicConnection> _connection = new(() => Config.SchematicConnection);
        private readonly Lazy<IIdentifierDefaults> _defaults = new(() => Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult());
    }
}
