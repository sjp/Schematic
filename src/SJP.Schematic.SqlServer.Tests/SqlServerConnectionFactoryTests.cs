using System;
using System.Data;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests;

[TestFixture]
internal static class SqlServerConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string connectionString)
    {
        Assert.That(() => new SqlServerConnectionFactory(connectionString), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        var factory = new SqlServerConnectionFactory("Server=127.0.0.1; Integrated Security=True;");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public static void CreateConnection_GivenNoConnectionConfiguration_DoesNotThrow()
    {
        var factory = new SqlServerConnectionFactory("Server=127.0.0.1; Integrated Security=True;", connectionConfiguration: null);

        Assert.That(() => factory.CreateConnection(), Throws.Nothing);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_InvokesCallbackBeforeReturning()
    {
        var wasInvoked = false;
        var factory = new SqlServerConnectionFactory(
            "Server=127.0.0.1; Integrated Security=True;",
            connection => wasInvoked = true);

        using var connection = factory.CreateConnection();

        Assert.That(wasInvoked, Is.True);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_AppliesConfigurationToReturnedConnection()
    {
        Func<SqlAuthenticationParameters, System.Threading.CancellationToken, System.Threading.Tasks.Task<SqlAuthenticationToken>> accessTokenCallback =
            (parameters, cancellationToken) => throw new NotSupportedException("Not used in this test.");

        var factory = new SqlServerConnectionFactory(
            "Server=127.0.0.1;",
            connection => connection.AccessTokenCallback = accessTokenCallback);

        using var connection = (SqlConnection)factory.CreateConnection();

        Assert.That(connection.AccessTokenCallback, Is.EqualTo(accessTokenCallback));
    }
}