using System;
using NUnit.Framework;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Tests.Handlers;

[TestFixture]
internal static class ConnectionStringFactoryTests
{
    [Test]
    public static void ForSqlite_GivenPath_ContainsDataSource()
    {
        var result = ConnectionStringFactory.ForSqlite("app.db");

        Assert.That(result, Does.Contain("Data Source=app.db"));
    }

    [Test]
    public static void BuildGuided_GivenSqlServerDetails_ContainsHostPortAndCatalog()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", 1433, "sa", "secret", "MyDb");

        var result = ConnectionStringFactory.BuildGuided("sqlserver", details);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("localhost,1433"));
            Assert.That(result, Does.Contain("Initial Catalog=MyDb"));
        }
    }

    [Test]
    public static void BuildGuided_GivenPostgreSqlDetails_ContainsHostAndPort()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", 5432, "postgres", "secret", "MyDb");

        var result = ConnectionStringFactory.BuildGuided("postgresql", details);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("Host=localhost"));
            Assert.That(result, Does.Contain("Port=5432"));
            Assert.That(result, Does.Contain("Database=MyDb"));
        }
    }

    [Test]
    public static void BuildGuided_GivenMySqlDetails_ContainsServer()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", 3306, "root", "secret", "MyDb");

        var result = ConnectionStringFactory.BuildGuided("mysql", details);

        Assert.That(result, Does.Contain("Server=localhost"));
    }

    [Test]
    public static void BuildGuided_GivenOracleDetails_ContainsEasyConnectDataSource()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", 1521, "system", "secret", "XEPDB1");

        var result = ConnectionStringFactory.BuildGuided("oracle", details);

        Assert.That(result, Does.Contain("localhost:1521/XEPDB1"));
    }

    [Test]
    public static void BuildGuided_GivenUnknownDialect_ThrowsNotSupportedException()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", null, null, null, null);

        Assert.That(
            () => ConnectionStringFactory.BuildGuided("db2", details),
            Throws.InstanceOf<NotSupportedException>());
    }
}
