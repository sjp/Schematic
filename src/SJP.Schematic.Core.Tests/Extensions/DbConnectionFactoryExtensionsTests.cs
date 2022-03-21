using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class DbConnectionFactoryExtensionsTests
{
    [Test]
    public static void Ctor_GivenNullFactory_ThrowsArgumentNullException()
    {
        Assert.That(() => DbConnectionFactoryExtensions.AsCachingFactory(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void CreateConnection_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<IDbConnection>);

        var cachingFactory = factory.Object.AsCachingFactory();

        _ = cachingFactory.CreateConnection();
        _ = cachingFactory.CreateConnection();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static void OpenConnection_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<IDbConnection>);

        var cachingFactory = factory.Object.AsCachingFactory();

        _ = cachingFactory.OpenConnection();
        _ = cachingFactory.OpenConnection();

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }

    [Test]
    public static async Task OpenConnectionAsync_WhenCalledTwice_OnlyCreatesConnectionOnce()
    {
        var factory = new Mock<IDbConnectionFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateConnection()).Returns(Mock.Of<DbConnection>);

        var cachingFactory = factory.Object.AsCachingFactory();

        _ = await cachingFactory.OpenConnectionAsync().ConfigureAwait(false);
        _ = await cachingFactory.OpenConnectionAsync().ConfigureAwait(false);

        factory.Verify(f => f.CreateConnection(), Times.Once);
    }
}