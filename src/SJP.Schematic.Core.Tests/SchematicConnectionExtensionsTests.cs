using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class SchematicConnectionExtensionsTests
    {
        [Test]
        public static void AddLogging_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => SchematicConnectionExtensions.AddLogging(null, Mock.Of<ILoggerFactory>(), LogLevel.Information), Throws.ArgumentNullException);
        }

        [Test]
        public static void AddLogging_GivenNullLoggerFactory_ThrowsArgumentNullException()
        {
            Assert.That(() => Mock.Of<ISchematicConnection>().AddLogging(null, LogLevel.Information), Throws.ArgumentNullException);
        }

        [Test]
        public static void AddLogging_GivenInvalidLoggingLevel_ThrowsArgumentxception()
        {
            Assert.That(() => Mock.Of<ISchematicConnection>().AddLogging(Mock.Of<ILoggerFactory>(), (LogLevel)555), Throws.ArgumentException);
        }

        [Test]
        public static void RemoveLogging_GivenNullConnection_ThrowsArgumentxception()
        {
            Assert.That(() => SchematicConnectionExtensions.RemoveLogging(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void AddLogging_GivenValidConnection_ThrowsNothing()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            Assert.That(() => connection.AddLogging(loggerFactory.Object, LogLevel.Information), Throws.Nothing);
        }

        [Test]
        public static void RemoveLogging_GivenNonLoggedConnection_ThrowsNothing()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            Assert.That(() => connection.RemoveLogging(), Throws.Nothing);
        }

        [Test]
        public static void RemoveLogging_GivenLoggedConnection_ThrowsNothing()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            connection.AddLogging(loggerFactory.Object, LogLevel.Information);
            Assert.That(() => connection.RemoveLogging(), Throws.Nothing);
        }

        [Test]
        public static void SetMaxConcurrentQueries_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.That(() => SchematicConnectionExtensions.SetMaxConcurrentQueries(null, 100), Throws.ArgumentNullException);
        }

        [Test]
        public static void SetMaxConcurrentQueries_GivenValidConnection_ThrowsNothing()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            Assert.That(() => connection.SetMaxConcurrentQueries(123), Throws.Nothing);
        }
    }
}
