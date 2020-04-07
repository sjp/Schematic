using System.Data;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2019
{
    internal static class Config2019
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new SqlServerConnectionFactory();

        public static IDbConnection Connection { get; } = Prelude.Try(() => !ConnectionString.IsNullOrWhiteSpace()
            ? ConnectionFactory.CreateConnection(ConnectionString)
            : null)
            .Match(c => c, _ => null);

        public static ISchematicConnection SchematicConnection => new SchematicConnection(
            Connection,
            new SqlServerDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlserver-test-2019.config.json")
            .AddJsonFile("sqlserver-test-2019.local.config.json", optional: true)
            .Build();
    }

    [Category("SqlServerDatabase")]
    [DatabaseTestFixture(typeof(Config2019), nameof(Config2019.Connection), "No SQL Server 2019 DB available")]
    internal abstract class SqlServer2019Test
    {
        protected ISchematicConnection Connection { get; } = Config2019.SchematicConnection;

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected ISqlServerDialect Dialect => Connection.Dialect as ISqlServerDialect;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config2019.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();
    }
}
