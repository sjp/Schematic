using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Tests.Fakes;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class CachingConnectionFactoryTests
{
    private static Mock<IDbConnectionFactory> CreateFactoryReturning(params DbConnection[] connections)
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        var sequence = factory.SetupSequence(f => f.CreateConnection());
        foreach (var connection in connections)
            sequence = sequence.Returns(connection);

        return factory;
    }

    [Test]
    public static void Ctor_GivenNullFactory_ThrowsArgumentNullException()
    {
        Assert.That(() => new CachingConnectionFactory(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void CreateConnection_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<DbConnection>);

        using var cachingFactory = new CachingConnectionFactory(factory.Object);

        _ = cachingFactory.CreateConnection();
        _ = cachingFactory.CreateConnection();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static void OpenConnection_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<DbConnection>);

        using var cachingFactory = new CachingConnectionFactory(factory.Object);

        _ = cachingFactory.OpenConnection();
        _ = cachingFactory.OpenConnection();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static async Task OpenConnectionAsync_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<DbConnection>);

        await using var cachingFactory = new CachingConnectionFactory(factory.Object);

        _ = await cachingFactory.OpenConnectionAsync();
        _ = await cachingFactory.OpenConnectionAsync();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static void DisposeConnection_PropertyGet_IsFalse()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        using var cachingFactory = new CachingConnectionFactory(factory.Object);

        Assert.That(cachingFactory.DisposeConnection, Is.False);
    }

    [Test]
    public static async Task OpenConnectionAsync_WhenCalledConcurrently_OpensConnectionOnceWithoutOverlapping()
    {
        using var connection = new FakeDbConnection();
        var factory = CreateFactoryReturning(connection);

        await using var cachingFactory = new CachingConnectionFactory(factory.Object);

        var openTasks = Enumerable.Range(0, 16)
            .Select(_ => Task.Run(() => cachingFactory.OpenConnectionAsync()))
            .ToArray();

        Assert.That(async () => await Task.WhenAll(openTasks), Throws.Nothing);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(connection.OverlappingOpenDetected, Is.False);
            Assert.That(connection.OpenCount, Is.EqualTo(1));
        }
    }

    [Test]
    public static void OpenConnection_WhenCalledConcurrently_OpensConnectionOnceWithoutOverlapping()
    {
        using var connection = new FakeDbConnection();
        var factory = CreateFactoryReturning(connection);

        using var cachingFactory = new CachingConnectionFactory(factory.Object);

        var openTasks = Enumerable.Range(0, 16)
            .Select(_ => Task.Run(cachingFactory.OpenConnection))
            .ToArray();

        Assert.That(() => Task.WaitAll(openTasks), Throws.Nothing);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(connection.OverlappingOpenDetected, Is.False);
            Assert.That(connection.OpenCount, Is.EqualTo(1));
        }
    }

    [Test]
    public static void Dispose_WhenConnectionCreated_DisposesConnection()
    {
        var connection = new FakeDbConnection();
        var factory = CreateFactoryReturning(connection);

        var cachingFactory = new CachingConnectionFactory(factory.Object);
        _ = cachingFactory.CreateConnection();
        cachingFactory.Dispose();

        Assert.That(connection.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public static async Task DisposeAsync_WhenConnectionCreated_DisposesConnection()
    {
        var connection = new FakeDbConnection();
        var factory = CreateFactoryReturning(connection);

        var cachingFactory = new CachingConnectionFactory(factory.Object);
        _ = cachingFactory.CreateConnection();
        await cachingFactory.DisposeAsync();

        Assert.That(connection.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public static void Dispose_WhenNoConnectionCreated_DoesNotThrow()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        var cachingFactory = new CachingConnectionFactory(factory.Object);

        Assert.That(cachingFactory.Dispose, Throws.Nothing);
    }

    [Test]
    public static void Dispose_WhenCalledTwice_OnlyDisposesConnectionOnce()
    {
        var connection = new FakeDbConnection();
        var factory = CreateFactoryReturning(connection);

        var cachingFactory = new CachingConnectionFactory(factory.Object);
        _ = cachingFactory.CreateConnection();
        cachingFactory.Dispose();
        cachingFactory.Dispose();

        Assert.That(connection.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public static void CreateConnection_WhenFactoryDisposed_ThrowsObjectDisposedException()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        var cachingFactory = new CachingConnectionFactory(factory.Object);
        cachingFactory.Dispose();

        Assert.That(() => cachingFactory.CreateConnection(), Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public static void OpenConnection_WhenFactoryDisposed_ThrowsObjectDisposedException()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        var cachingFactory = new CachingConnectionFactory(factory.Object);
        cachingFactory.Dispose();

        Assert.That(() => cachingFactory.OpenConnection(), Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public static void OpenConnectionAsync_WhenFactoryDisposed_ThrowsObjectDisposedException()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        var cachingFactory = new CachingConnectionFactory(factory.Object);
        cachingFactory.Dispose();

        Assert.That(async () => await cachingFactory.OpenConnectionAsync(), Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public static async Task OpenConnectionAsync_WhenCachedConnectionBroken_ReplacesConnection()
    {
        var brokenConnection = new FakeDbConnection();
        using var replacementConnection = new FakeDbConnection();
        var factory = CreateFactoryReturning(brokenConnection, replacementConnection);

        await using var cachingFactory = new CachingConnectionFactory(factory.Object);

        var firstConnection = await cachingFactory.OpenConnectionAsync();
        brokenConnection.SetBroken();
        var secondConnection = await cachingFactory.OpenConnectionAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstConnection, Is.SameAs(brokenConnection));
            Assert.That(secondConnection, Is.SameAs(replacementConnection));
            Assert.That(brokenConnection.DisposeCount, Is.EqualTo(1));
            Assert.That(replacementConnection.OpenCount, Is.EqualTo(1));
        }
    }

    [Test]
    public static void CreateConnection_WhenCachedConnectionBroken_ReplacesConnection()
    {
        var brokenConnection = new FakeDbConnection();
        using var replacementConnection = new FakeDbConnection();
        var factory = CreateFactoryReturning(brokenConnection, replacementConnection);

        using var cachingFactory = new CachingConnectionFactory(factory.Object);

        _ = cachingFactory.CreateConnection();
        brokenConnection.SetBroken();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cachingFactory.CreateConnection(), Is.SameAs(replacementConnection));
            Assert.That(brokenConnection.DisposeCount, Is.EqualTo(1));
        }
    }
}
