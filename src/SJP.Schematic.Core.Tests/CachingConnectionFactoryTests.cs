using System.Data.Common;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class CachingConnectionFactoryTests
{
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

        var cachingFactory = new CachingConnectionFactory(factory.Object);

        _ = cachingFactory.CreateConnection();
        _ = cachingFactory.CreateConnection();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static void OpenConnection_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<DbConnection>);

        var cachingFactory = new CachingConnectionFactory(factory.Object);

        _ = cachingFactory.OpenConnection();
        _ = cachingFactory.OpenConnection();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static async Task OpenConnectionAsync_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<DbConnection>);

        var cachingFactory = new CachingConnectionFactory(factory.Object);

        _ = await cachingFactory.OpenConnectionAsync();
        _ = await cachingFactory.OpenConnectionAsync();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static void DisposeConnection_PropertyGet_IsFalse()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        var cachingFactory = new CachingConnectionFactory(factory.Object);

        Assert.That(cachingFactory.DisposeConnection, Is.False);
    }
}