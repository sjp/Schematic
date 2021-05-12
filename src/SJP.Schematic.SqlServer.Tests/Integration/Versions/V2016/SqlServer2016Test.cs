using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2016
{
    internal static class Config2016
    {
        public static IDbConnectionFactory ConnectionFactory => !ConnectionString.IsNullOrWhiteSpace()
            ? new SqlServerConnectionFactory(ConnectionString)
            : null;

        public static ISchematicConnection SchematicConnection => new SchematicConnection(
            ConnectionFactory,
            new SqlServerDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("SqlServer_TestDb_2016");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("sqlserver-test.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config2016), nameof(Config2016.ConnectionFactory), "No SQL Server 2016 DB available")]
    internal abstract class SqlServer2016Test
    {
        protected ISchematicConnection Connection => _connection.Value;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected ISqlServerDialect Dialect => Connection.Dialect as ISqlServerDialect;

        protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

        private readonly Lazy<ISchematicConnection> _connection = new(() => Config2016.SchematicConnection);
        private readonly Lazy<IIdentifierDefaults> _defaults = new(() => Config2016.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config2016.SchematicConnection).GetAwaiter().GetResult());
    }
}
