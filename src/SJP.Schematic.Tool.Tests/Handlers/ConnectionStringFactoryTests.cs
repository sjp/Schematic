#nullable enable
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

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void ForSqlite_GivenNullOrWhiteSpacePath_ThrowsArgumentException(string? path)
    {
        Assert.That(() => ConnectionStringFactory.ForSqlite(path!), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void BuildGuided_GivenNullOrWhiteSpaceDialect_ThrowsArgumentException(string? dialect)
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", null, null, null, null);

        Assert.That(() => ConnectionStringFactory.BuildGuided(dialect!, details), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void BuildGuided_GivenDialectIgnoringCase_IsSupported()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", 5432, null, null, null);

        var result = ConnectionStringFactory.BuildGuided("PostgreSQL", details);

        Assert.That(result, Does.Contain("Host=localhost"));
    }

    [Test]
    public static void BuildGuided_GivenSqliteDialect_UsesHostAsDataSource()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("app.db", null, null, null, null);

        var result = ConnectionStringFactory.BuildGuided("sqlite", details);

        Assert.That(result, Does.Contain("Data Source=app.db"));
    }

    [Test]
    public static void BuildGuided_GivenSqlServerWithoutPort_OmitsPortFromDataSource()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", null, "sa", "secret", "MyDb");

        var result = ConnectionStringFactory.BuildGuided("sqlserver", details);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("localhost"));
            Assert.That(result, Does.Not.Contain("localhost,"));
        }
    }

    [Test]
    public static void BuildGuided_GivenPostgreSqlWithoutCredentials_OmitsUserAndPassword()
    {
        var details = new ConnectionStringFactory.ConnectionDetails("localhost", 5432, null, null, "MyDb");

        var result = ConnectionStringFactory.BuildGuided("postgresql", details);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Not.Contain("Username="));
            Assert.That(result, Does.Not.Contain("Password="));
        }
    }
}
