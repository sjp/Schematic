using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal static class Config
{
    public static IDbConnectionFactory ConnectionFactory => !ConnectionString.IsNullOrWhiteSpace()
        ? new PostgreSqlConnectionFactory(ConnectionString)
        : null;

    public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new PostgreSqlDialect());

    private static string ConnectionString => Configuration.GetConnectionString("PostgreSql_TestDb");

    private static IConfigurationRoot Configuration => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("postgresql-test.config.json", optional: true)
        .Build();
}

[Category("PostgreSqlDatabase")]
[DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No PostgreSQL DB available")]
[Parallelizable(ParallelScope.Children)]
internal abstract class PostgreSqlTest
{
    protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected PostgreSqlDatabaseProvider DatabaseProvider { get; } = new(Config.SchematicConnection);

    protected IIdentifierDefaults IdentifierDefaults { get; } = new PostgreSqlDatabaseProvider(Config.SchematicConnection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult();

    protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();

    /// <summary>
    /// Executes multiple DDL statements as a single round-trip. Npgsql sends multi-statement
    /// command text as-is, so any mix of statements can be batched together.
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