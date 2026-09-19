using System;
using System.Data;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests;

internal static class SqlServerConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string connectionString)
    {
        Assert.That(
            () => new SqlServerConnectionFactory(connectionString),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("connectionString")
        );
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
        void configureConnection(SqlConnection _) => wasInvoked = true;

        var factory = new SqlServerConnectionFactory(
            "Server=127.0.0.1; Integrated Security=True;",
            configureConnection);

        using var connection = factory.CreateConnection();

        Assert.That(wasInvoked, Is.True);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_AppliesConfigurationToReturnedConnection()
    {
        static System.Threading.Tasks.Task<SqlAuthenticationToken> accessTokenCallback(SqlAuthenticationParameters _, System.Threading.CancellationToken __)
            => throw new NotSupportedException("Not used in this test.");

        var factory = new SqlServerConnectionFactory(
            "Server=127.0.0.1;",
            connection => connection.AccessTokenCallback = accessTokenCallback);

        using var connection = (SqlConnection)factory.CreateConnection();

        Assert.That(connection.AccessTokenCallback, Is.EqualTo(accessTokenCallback));
    }

    [Test]
    public static void MaxConcurrentQueries_GivenMaxPoolSize_ReturnsMaxPoolSize()
    {
        var factory = new SqlServerConnectionFactory("Server=127.0.0.1;Max Pool Size=7;");

        Assert.That(factory.MaxConcurrentQueries, Is.EqualTo(7));
    }

    [Test]
    public static void MaxConcurrentQueries_GivenNoMaxPoolSize_ReturnsDefaultPoolSize()
    {
        var factory = new SqlServerConnectionFactory("Server=127.0.0.1;");

        Assert.That(factory.MaxConcurrentQueries, Is.EqualTo(new SqlConnectionStringBuilder().MaxPoolSize));
    }
}