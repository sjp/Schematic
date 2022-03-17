using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string connectionString)
    {
        Assert.That(() => new MySqlConnectionFactory(connectionString), Throws.ArgumentNullException);
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        var factory = new MySqlConnectionFactory("Server=127.0.0.1;");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }
}
