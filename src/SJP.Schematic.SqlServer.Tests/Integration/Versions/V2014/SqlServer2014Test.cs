using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2014
{
    internal static class Config2014
    {
        public static IDbConnectionFactory ConnectionFactory => new SqlServerConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(
            ConnectionFactory,
            new SqlServerDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("SqlServer_TestDb_2014");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("sqlserver-test-2014.config.json")
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config2014), nameof(Config2014.ConnectionFactory), "No SQL Server 2014 DB available")]
    internal abstract class SqlServer2014Test
    {
        protected ISchematicConnection Connection { get; } = Config2014.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected ISqlServerDialect Dialect => Connection.Dialect as ISqlServerDialect;

        protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

        private readonly Lazy<IIdentifierDefaults> _defaults = new Lazy<IIdentifierDefaults>(() => Config2014.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config2014.SchematicConnection).GetAwaiter().GetResult());
    }
}
