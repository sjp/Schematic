using System;
using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class LoggingTests
    {
        [Test]
        public static void IsLoggingConfigured_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.That(() => Logging.IsLoggingConfigured(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void IsLoggingConfigured_GivenConfiguredConnection_ReturnsTrue()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var connection = new SchematicConnection(dbConnection, Mock.Of<IDatabaseDialect>());

            Logging.AddLogging(connection, Mock.Of<ILogger>(), LogLevel.Information);
            Assert.That(Logging.IsLoggingConfigured(dbConnection), Is.True);
        }

        [Test]
        public static void IsLoggingConfigured_GivenNonConfiguredConnection_ReturnsFalse()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            Assert.That(Logging.IsLoggingConfigured(dbConnection), Is.False);
        }

        [Test]
        public static void AddLogging_GivenNullConnection_ThrowsArgNullException()
        {
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            Assert.That(() => Logging.AddLogging(null, logger, logLevel), Throws.ArgumentNullException);
        }

        [Test]
        public static void AddLogging_GivenNullLogger_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            const LogLevel logLevel = LogLevel.Information;

            Assert.That(() => Logging.AddLogging(connection, null, logLevel), Throws.ArgumentNullException);
        }

        [Test]
        public static void AddLogging_GivenInvalidLogLevel_ThrowsArgException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = (LogLevel)555;

            Assert.That(() => Logging.AddLogging(connection, logger, logLevel), Throws.ArgumentException);
        }

        [Test]
        public static void RemoveLogging_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.That(() => Logging.RemoveLogging(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void RemoveLogging_WhenNoLoggingConfigured_ThrowsNothing()
        {
            var connectionMock = new Mock<ISchematicConnection>();
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnection>());

            Assert.That(() => Logging.RemoveLogging(connectionMock.Object), Throws.Nothing);
        }

        [Test]
        public static void RemoveLogging_WhenLoggingConfigured_ThrowsNothing()
        {
            var connectionMock = new Mock<ISchematicConnection>();
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnection>());
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);
            Assert.That(() => Logging.RemoveLogging(connectionMock.Object), Throws.Nothing);
        }
    }
}
