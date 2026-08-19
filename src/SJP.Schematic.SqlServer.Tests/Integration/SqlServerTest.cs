using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

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
[Parallelizable(ParallelScope.Children)]
internal abstract class SqlServerTest
{
    protected ISchematicConnection Connection => _connection.Value;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected ISqlServerDatabaseProvider DatabaseProvider => _databaseProvider.Value;

    protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

    private readonly Lazy<ISchematicConnection> _connection = new(() => Config.SchematicConnection);
    private readonly Lazy<ISqlServerDatabaseProvider> _databaseProvider = new(() => new SqlServerDatabaseProvider(Config.SchematicConnection));
    private readonly Lazy<IIdentifierDefaults> _defaults = new(() => new SqlServerDatabaseProvider(Config.SchematicConnection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult());

    /// <summary>
    /// Executes multiple DDL statements as a single T-SQL batch, in one round-trip. Every
    /// statement must be able to appear alongside others in a batch -- <c>create view</c>,
    /// <c>procedure</c>, <c>function</c> and <c>trigger</c> must be the only statement in their
    /// own batch, so keep those as individual <see cref="DbConnection"/> calls.
    /// </summary>
    protected Task ExecuteBatchAsync(params string[] statements) =>
        DbConnection.ExecuteAsync(string.Join(";\n", statements), CancellationToken.None);

    /// <summary>
    /// Drops multiple tables in a single round-trip. Table names are dropped in the order given,
    /// so pass them in dependency order (children before parents) exactly as with individual drops.
    /// </summary>
    protected Task DropTablesAsync(params string[] tableNames) =>
        ExecuteBatchAsync([.. tableNames.Select(static t => "drop table " + t)]);
}