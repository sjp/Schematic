﻿using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal static class Config
{
    public static IDbConnectionFactory ConnectionFactory { get; } = new CachingConnectionFactory(new SqliteConnectionFactory(ConnectionString));

    public static ISchematicConnection Connection { get; } = new SchematicConnection(ConnectionFactory, new SqliteDialect());

    private static string ConnectionString => Configuration.GetConnectionString("Sqlite_TestDb");

    private static IConfigurationRoot Configuration => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("sqlite-test.config.json", optional: true)
        .Build();
}

[Category("SqliteDatabase")]
[TestFixture]
internal abstract class SqliteTest
{
    protected ISchematicConnection Connection { get; } = Config.Connection;

    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    protected IDatabaseDialect Dialect => Connection.Dialect;

    protected IIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect().GetIdentifierDefaultsAsync(Config.Connection).GetAwaiter().GetResult();

    protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(Config.Connection);

    protected ISqliteDatabase GetSqliteDatabase() => new SqliteRelationalDatabase(Config.Connection, IdentifierDefaults, Pragma);
}
