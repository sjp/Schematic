using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    [TestFixture]
    internal class OracleConnectionFactoryTests : OracleTest
    {
        [Test]
        public void OpenConnection_WhenInvoked_ReturnsConnectionInOpenState()
        {
            var factory = Config.ConnectionFactory;
            using var connection = factory.OpenConnection();

            Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
        }

        [Test]
        public async Task OpenConnectionAsync_WhenInvoked_ReturnsConnectionInOpenState()
        {
            var factory = Config.ConnectionFactory;
            using var connection = await factory.OpenConnectionAsync().ConfigureAwait(false);

            Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
        }
    }
}
