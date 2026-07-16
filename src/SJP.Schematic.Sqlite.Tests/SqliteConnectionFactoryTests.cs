using System;
using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string connectionString)
    {
        Assert.That(() => new SqliteConnectionFactory(connectionString), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public static void CreateConnection_GivenNoConnectionConfiguration_DoesNotThrow()
    {
        var factory = new SqliteConnectionFactory("Data Source=:memory:", connectionConfiguration: null);

        Assert.That(() => factory.CreateConnection(), Throws.Nothing);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_InvokesCallbackBeforeReturning()
    {
        var wasInvoked = false;
        var factory = new SqliteConnectionFactory(
            "Data Source=:memory:",
            connection => wasInvoked = true);

        using var connection = factory.CreateConnection();

        Assert.That(wasInvoked, Is.True);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_AppliesConfigurationToReturnedConnection()
    {
        const string expectedConnectionString = "Data Source=other.db";
        var factory = new SqliteConnectionFactory(
            "Data Source=:memory:",
            connection => connection.ConnectionString = expectedConnectionString);

        using var connection = factory.CreateConnection();

        Assert.That(connection.ConnectionString, Is.EqualTo(expectedConnectionString));
    }
}