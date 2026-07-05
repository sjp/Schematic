using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

internal static class SqliteRuleTestConfig
{
    public static IDbConnectionFactory ConnectionFactory { get; } = new SqliteConnectionFactory(ConnectionString);

    public static ISchematicConnection Connection { get; } = new SchematicConnection(
        ConnectionFactory,
        new SqliteDialect()
    );

    private static string ConnectionString => Configuration.GetConnectionString("Reporting_Lint_TestDb")!;

    private static IConfigurationRoot Configuration => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("sqlite-test.config.json", optional: true)
        .Build();
}

[TestFixture]
internal abstract class SqliteRuleTestBase
{
    protected ISchematicConnection Connection { get; } = SqliteRuleTestConfig.Connection;

    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    protected IIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect().GetIdentifierDefaultsAsync(SqliteRuleTestConfig.Connection).GetAwaiter().GetResult();

    protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(SqliteRuleTestConfig.Connection);

    protected IRelationalDatabase GetSqliteDatabase() => new SqliteRelationalDatabase(SqliteRuleTestConfig.Connection, IdentifierDefaults, Pragma);
}
