using System;
using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string connectionString)
    {
        Assert.That(() => new OracleConnectionFactory(connectionString), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        var factory = new OracleConnectionFactory("Data Source=127.0.0.1/orcl; User Id=SYSTEM; Password=oracle");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public static void CreateConnection_GivenNoConnectionConfiguration_DoesNotThrow()
    {
        var factory = new OracleConnectionFactory("Data Source=127.0.0.1/orcl; User Id=SYSTEM; Password=oracle", connectionConfiguration: null);

        Assert.That(() => factory.CreateConnection(), Throws.Nothing);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_InvokesCallbackBeforeReturning()
    {
        var wasInvoked = false;
        var factory = new OracleConnectionFactory(
            "Data Source=127.0.0.1/orcl; User Id=SYSTEM; Password=oracle",
            connection => wasInvoked = true);

        using var connection = factory.CreateConnection();

        Assert.That(wasInvoked, Is.True);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_AppliesConfigurationToReturnedConnection()
    {
        const string expectedConnectionString = "Data Source=127.0.0.1/orcl; User Id=SYSTEM; Password=hunter2";
        var factory = new OracleConnectionFactory(
            "Data Source=127.0.0.1/orcl; User Id=SYSTEM; Password=oracle",
            connection => connection.ConnectionString = expectedConnectionString);

        using var connection = factory.CreateConnection();

        Assert.That(connection.ConnectionString, Is.EqualTo(expectedConnectionString));
    }
}