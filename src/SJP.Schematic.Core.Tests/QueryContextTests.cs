using Microsoft.Extensions.Logging;
using Moq;
using Nito.AsyncEx;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class QueryContextTests
    {
        [Test]
        public static void CreateAsync_GivenNullConnectionFactory_ThrowsArgNullException()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>();
            const string sql = "select 1";
            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(() => QueryContext.CreateAsync(null, loggingContext), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateAsync_GivenNullLoggingContext_ThrowsArgNullException()
        {
            Assert.That(() => QueryContext.CreateAsync(Mock.Of<IDbConnectionFactory>(), null), Throws.ArgumentNullException);
        }

        [Test]
        public static void SetMaxConcurrentQueries_GivenNullConnectionFactory_ThrowsArgNullException()
        {
            Assert.That(() => QueryContext.SetMaxConcurrentQueries(null, new AsyncSemaphore(1)), Throws.ArgumentNullException);
        }

        [Test]
        public static void SetMaxConcurrentQueries_GivenNullSemaphore_ThrowsArgNullException()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>();

            Assert.That(() => QueryContext.SetMaxConcurrentQueries(connectionFactory, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateAsync_WithoutQueryLimitConfigured_ReturnsNewInstance()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>();
            const string sql = "select 1";
            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(async () => await QueryContext.CreateAsync(connectionFactory, loggingContext), Throws.Nothing);
        }

        [Test]
        public static void CreateAsync_WithQueryLimitConfigured_ReturnsNewInstance()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>();
            const string sql = "select 1";
            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            QueryContext.SetMaxConcurrentQueries(connectionFactory, new AsyncSemaphore(1));

            Assert.That(async () => await QueryContext.CreateAsync(connectionFactory, loggingContext), Throws.Nothing);
        }
    }
}
