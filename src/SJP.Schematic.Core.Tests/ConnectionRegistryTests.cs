using System;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class ConnectionRegistryTests
{
    [Test]
    public static void RegisterConnection_GivenNullConnection_ThrowsArgNullException()
    {
        Assert.That(() => ConnectionRegistry.RegisterConnection(Guid.NewGuid(), null), Throws.ArgumentNullException);
    }

    [Test]
    public static void RegisterConnection_GivenValidConnection_SetsConnection()
    {
        var connectionId = Guid.NewGuid();
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => ConnectionRegistry.RegisterConnection(connectionId, connection), Throws.Nothing);
    }

    [Test]
    public static void RegisterConnection_GivenTwoConnectionsWithSameId_SetsSecondConnection()
    {
        var connectionId = Guid.NewGuid();
        var firstConnection = Mock.Of<IDbConnectionFactory>();
        var secondConnection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() =>
        {
            ConnectionRegistry.RegisterConnection(connectionId, firstConnection);
            ConnectionRegistry.RegisterConnection(connectionId, secondConnection);
        }, Throws.Nothing);
    }

    [Test]
    public static void TryGetConnectionId_GivenNullConnection_ThrowsArgNullException()
    {
        Assert.That(() => ConnectionRegistry.TryGetConnectionId(null, out _), Throws.ArgumentNullException);
    }

    [Test]
    public static void TryGetConnectionId_WhenNoConnectionSet_ReturnsEmptyGuid()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var result = ConnectionRegistry.TryGetConnectionId(connection, out var connectionId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(connectionId, Is.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public static void TryGetConnectionId_WhenConnectionSet_ReturnsConnectionId()
    {
        var connectionId = Guid.NewGuid();
        var connection = Mock.Of<IDbConnectionFactory>();

        ConnectionRegistry.RegisterConnection(connectionId, connection);
        var result = ConnectionRegistry.TryGetConnectionId(connection, out var retrievedConnectionId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(retrievedConnectionId, Is.EqualTo(connectionId));
        });
    }
}