using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

// Caps how many tests run concurrently under [Parallelizable(ParallelScope.Children)], so the DB
// service container's max_connections isn't exhausted regardless of the CI runner's core count.
[assembly: LevelOfParallelism(4)]

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
    protected ISchematicConnection Connection => _connection.Value;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected PostgreSqlDatabaseProvider DatabaseProvider => _databaseProvider.Value;

    protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

    protected IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultPostgreSqlIdentifierResolutionStrategy();

    protected PostgreSqlTest()
    {
        _connection = new Lazy<ISchematicConnection>(() => Config.SchematicConnection);
        _databaseProvider = new Lazy<PostgreSqlDatabaseProvider>(() => new PostgreSqlDatabaseProvider(Connection));
        _defaults = new Lazy<IIdentifierDefaults>(() => new PostgreSqlDatabaseProvider(Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult());
    }

    private readonly Lazy<ISchematicConnection> _connection;
    private readonly Lazy<PostgreSqlDatabaseProvider> _databaseProvider;
    private readonly Lazy<IIdentifierDefaults> _defaults;

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