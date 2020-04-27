using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerConnectionFactoryTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string connectionString)
        {
            Assert.That(() => new SqlServerConnectionFactory(connectionString), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
        {
            var factory = new SqlServerConnectionFactory("Server=127.0.0.1; Integrated Security=True;");
            using var connection = factory.CreateConnection();

            Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
        }
    }
}
