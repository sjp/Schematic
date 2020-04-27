using System;
using EnumsNET;
using Microsoft.Extensions.Logging;

namespace SJP.Schematic.Core
{
    internal sealed class LoggingConfiguration
    {
        public LoggingConfiguration(ILogger logger, LogLevel logLevel)
        {
            if (!logLevel.IsValid())
                throw new ArgumentException($"The { nameof(LogLevel) } provided must be a valid enum.", nameof(logLevel));

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LogLevel = logLevel;
        }

        public ILogger Logger { get; }

        public LogLevel LogLevel { get; }
    }
}