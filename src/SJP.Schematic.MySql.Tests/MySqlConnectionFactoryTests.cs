using System;
using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.MySql.Tests;

internal static class MySqlConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string connectionString)
    {
        Assert.That(
            () => new MySqlConnectionFactory(connectionString),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("connectionString")
        );
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        using var factory = new MySqlConnectionFactory("Server=127.0.0.1;");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public static void CreateConnection_GivenNoConnectionConfiguration_DoesNotThrow()
    {
        using var factory = new MySqlConnectionFactory("Server=127.0.0.1;", connectionConfiguration: null);

        Assert.That(() => factory.CreateConnection(), Throws.Nothing);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_InvokesCallbackBeforeReturning()
    {
        var wasInvoked = false;
        using var factory = new MySqlConnectionFactory(
            "Server=127.0.0.1;",
            connection => wasInvoked = true);

        using var connection = factory.CreateConnection();

        Assert.That(wasInvoked, Is.True);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_AppliesConfigurationToReturnedConnection()
    {
        const string expectedConnectionString = "Server=127.0.0.1;Database=other;";
        using var factory = new MySqlConnectionFactory(
            "Server=127.0.0.1;",
            connection => connection.ConnectionString = expectedConnectionString);

        using var connection = factory.CreateConnection();

        Assert.That(connection.ConnectionString, Is.EqualTo(expectedConnectionString));
    }

    [Test]
    public static void MaxConcurrentQueries_GivenMaximumPoolSize_ReturnsMaximumPoolSize()
    {
        using var factory = new MySqlConnectionFactory("Server=127.0.0.1;Maximum Pool Size=7;");

        Assert.That(factory.MaxConcurrentQueries, Is.EqualTo(7));
    }

    [Test]
    public static void Dispose_WhenInvokedTwice_DoesNotThrow()
    {
        var factory = new MySqlConnectionFactory("Server=127.0.0.1;");
        factory.Dispose();

        Assert.That(factory.Dispose, Throws.Nothing);
    }

    [Test]
    public static void OpenConnection_AfterDispose_ThrowsObjectDisposedException()
    {
        var factory = new MySqlConnectionFactory("Server=127.0.0.1;");
        factory.Dispose();

        Assert.That(() => factory.OpenConnection(), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public static async Task OpenConnectionAsync_AfterDisposeAsync_ThrowsObjectDisposedException()
    {
        var factory = new MySqlConnectionFactory("Server=127.0.0.1;");
        await factory.DisposeAsync();

        Assert.That(async () => await factory.OpenConnectionAsync(), Throws.InstanceOf<ObjectDisposedException>());
    }
}