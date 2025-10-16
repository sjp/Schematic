using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal static class SqliteConnectionFactoryTests
{
    [Test]
    public static void OpenConnection_WhenInvoked_ReturnsConnectionInOpenState()
    {
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using var connection = factory.OpenConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
    }

    [Test]
    public static async Task OpenConnectionAsync_WhenInvoked_ReturnsConnectionInOpenState()
    {
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        await using var connection = await factory.OpenConnectionAsync();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
    }
}