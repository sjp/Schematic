using System;
using EnumsNET;
using Microsoft.Extensions.Logging;

namespace SJP.Schematic.Core
{
    internal class LoggingConfiguration
    {
        public LoggingConfiguration(ILogger logger, LogLevel level)
        {
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(LogLevel) } provided must be a valid enum.", nameof(level));

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Level = level;
        }

        public ILogger Logger { get; }

        public LogLevel Level { get; }
    }
}