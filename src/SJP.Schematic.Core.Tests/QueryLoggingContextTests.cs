using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class QueryLoggingContextTests
    {
        [Test]
        public static void Ctor_GivenNullConnectionFactory_ThrowsArgNullException()
        {
            const string sql = "select 1";

            Assert.That(() => new QueryLoggingContext(null, sql, null), Throws.ArgumentNullException);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceSql_ThrowsArgNullException(string sql)
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>();

            Assert.That(() => new QueryLoggingContext(connectionFactory, sql, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Start_WhenNotConfigured_ShouldDoNothing()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>(MockBehavior.Strict);
            const string sql = "select 1";

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(() => loggingContext.Start(), Throws.Nothing);
        }

        [Test]
        public static void Stop_WhenNotConfigured_ShouldDoNothing()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>(MockBehavior.Strict);
            const string sql = "select 1";

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(() => loggingContext.Stop(), Throws.Nothing);
        }

        [Test]
        public static void Start_WhenAlreadyStarted_ShouldDoNothing()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>(MockBehavior.Strict);
            var logger = Mock.Of<ILogger>();
            const string sql = "select 1";

            var connection = new SchematicConnection(connectionFactory, Mock.Of<IDatabaseDialect>());

            Logging.AddLogging(connection, logger, LogLevel.Information);

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(() => loggingContext.Start(), Throws.Nothing);
            Assert.That(() => loggingContext.Start(), Throws.Nothing);
        }

        [Test]
        public static void Stop_WhenNotStarted_ShouldDoNothing()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>(MockBehavior.Strict);
            var logger = Mock.Of<ILogger>();
            const string sql = "select 1";

            var connection = new SchematicConnection(connectionFactory, Mock.Of<IDatabaseDialect>());

            Logging.AddLogging(connection, logger, LogLevel.Information);

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(() => loggingContext.Stop(), Throws.Nothing);
        }

        [Test]
        public static void Start_WhenLoggingConfigured_ShouldLog()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            const string sql = "select 1";

            var connection = new SchematicConnection(connectionFactory, Mock.Of<IDatabaseDialect>());

            Logging.AddLogging(connection, logger, LogLevel.Information);

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(() => loggingContext.Start(), Throws.Nothing);
        }

        [Test]
        public static void Stop_WhenLoggingConfigured_ShouldLog()
        {
            var connectionFactory = Mock.Of<IDbConnectionFactory>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            const string sql = "select 1";

            var connection = new SchematicConnection(connectionFactory, Mock.Of<IDatabaseDialect>());

            Logging.AddLogging(connection, logger, LogLevel.Information);

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);

            Assert.That(() => loggingContext.Start(), Throws.Nothing);
            Assert.That(() => loggingContext.Stop(), Throws.Nothing);
        }
    }
}
