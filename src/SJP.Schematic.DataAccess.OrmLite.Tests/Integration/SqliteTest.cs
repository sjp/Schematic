using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.DataAccess.OrmLite.Tests.Integration;

internal static class Config
{
    public static IDbConnectionFactory ConnectionFactory { get; } = new SqliteConnectionFactory(ConnectionString);

    public static ISchematicConnection Connection { get; } = new SchematicConnection(
        ConnectionFactory,
        new SqliteDialect()
    );

    private static string ConnectionString => Configuration.GetConnectionString("OrmLite_TestDb");

    private static IConfigurationRoot Configuration => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("sqlite-test.config.json", optional: true)
        .Build();
}

[TestFixture]
internal abstract class SqliteTest
{
    protected ISchematicConnection Connection { get; } = Config.Connection;

    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(Config.Connection);

    /// <summary>
    /// Resolved once per process: every fixture shares the same cached value rather than issuing
    /// its own blocking round-trip.
    /// </summary>
    protected IIdentifierDefaults IdentifierDefaults => IdentifierDefaultsLazy.Value;

    private static readonly Lazy<IIdentifierDefaults> IdentifierDefaultsLazy = new(static () =>
        new SqliteDatabaseProvider(Config.Connection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult());
}