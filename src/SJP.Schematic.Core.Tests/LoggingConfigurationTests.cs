using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class LoggingConfigurationTests
    {
        [Test]
        public static void Ctor_GivenNullLogger_ThrowsArgNullException()
        {
            Assert.That(() => new LoggingConfiguration(null, LogLevel.Information), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidLoggingLevel_ThrowsArgException()
        {
            Assert.That(() => new LoggingConfiguration(Mock.Of<ILogger>(), (LogLevel)555), Throws.ArgumentException);
        }

        [Test]
        public static void Logger_PropertyGet_EqualsCtorArg()
        {
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            var loggingConfig = new LoggingConfiguration(logger, logLevel);

            Assert.That(loggingConfig.Logger, Is.EqualTo(logger));
        }

        [Test]
        public static void LogLevel_PropertyGet_EqualsCtorArg()
        {
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            var loggingConfig = new LoggingConfiguration(logger, logLevel);

            Assert.That(loggingConfig.LogLevel, Is.EqualTo(logLevel));
        }
    }
}
