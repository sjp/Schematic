using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2012
{
    internal static class Config2012
    {
        public static IDbConnectionFactory ConnectionFactory => new SqlServerConnectionFactory(ConnectionString);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(
            ConnectionFactory,
            new SqlServerDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("SqlServer_TestDb_2012");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("sqlserver-test.config.json")
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config2012), nameof(Config2012.ConnectionFactory), "No SQL Server 2012 DB available")]
    internal abstract class SqlServer2012Test
    {
        protected ISchematicConnection Connection { get; } = Config2012.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected ISqlServerDialect Dialect => Connection.Dialect as ISqlServerDialect;

        protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

        private readonly Lazy<IIdentifierDefaults> _defaults = new Lazy<IIdentifierDefaults>(() => Config2012.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config2012.SchematicConnection).GetAwaiter().GetResult());
    }
}
