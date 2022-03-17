using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string connectionString)
    {
        Assert.That(() => new OracleConnectionFactory(connectionString), Throws.ArgumentNullException);
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        var factory = new OracleConnectionFactory("Data Source=127.0.0.1/orcl; User Id=SYSTEM; Password=oracle");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }
}
