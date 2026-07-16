using System;
using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string connectionString)
    {
        Assert.That(() => new MySqlConnectionFactory(connectionString), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        var factory = new MySqlConnectionFactory("Server=127.0.0.1;");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public static void CreateConnection_GivenNoConnectionConfiguration_DoesNotThrow()
    {
        var factory = new MySqlConnectionFactory("Server=127.0.0.1;", connectionConfiguration: null);

        Assert.That(() => factory.CreateConnection(), Throws.Nothing);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_InvokesCallbackBeforeReturning()
    {
        var wasInvoked = false;
        var factory = new MySqlConnectionFactory(
            "Server=127.0.0.1;",
            connection => wasInvoked = true);

        using var connection = factory.CreateConnection();

        Assert.That(wasInvoked, Is.True);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_AppliesConfigurationToReturnedConnection()
    {
        const string expectedConnectionString = "Server=127.0.0.1;Database=other;";
        var factory = new MySqlConnectionFactory(
            "Server=127.0.0.1;",
            connection => connection.ConnectionString = expectedConnectionString);

        using var connection = factory.CreateConnection();

        Assert.That(connection.ConnectionString, Is.EqualTo(expectedConnectionString));
    }
}