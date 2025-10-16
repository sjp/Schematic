using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed class SqlServerConnectionFactoryTests : SqlServerTest
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
        await using var connection = await factory.OpenConnectionAsync();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
    }
}