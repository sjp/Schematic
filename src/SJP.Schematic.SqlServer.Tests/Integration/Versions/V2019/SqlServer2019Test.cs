﻿using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2019;

internal static class Config2019
{
    public static IDbConnectionFactory ConnectionFactory => !ConnectionString.IsNullOrWhiteSpace()
        ? new SqlServerConnectionFactory(ConnectionString)
        : null;

    public static ISchematicConnection SchematicConnection => new SchematicConnection(
        ConnectionFactory,
        new SqlServerDialect()
    );

    private static string ConnectionString => Configuration.GetConnectionString("SqlServer_TestDb_2019");

    private static IConfigurationRoot Configuration => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("sqlserver-test.config.json", optional: true)
        .Build();
}

[Category("SqlServerDatabase")]
[DatabaseTestFixture(typeof(Config2019), nameof(Config2019.ConnectionFactory), "No SQL Server 2019 DB available")]
internal abstract class SqlServer2019Test
{
    protected ISchematicConnection Connection => _connection.Value;

    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    protected ISqlServerDialect Dialect => Connection.Dialect as ISqlServerDialect;

    protected IIdentifierDefaults IdentifierDefaults => _defaults.Value;

    private readonly Lazy<ISchematicConnection> _connection = new(() => Config2019.SchematicConnection);
    private readonly Lazy<IIdentifierDefaults> _defaults = new(() => Config2019.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config2019.SchematicConnection).GetAwaiter().GetResult());
}
