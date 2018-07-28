using System;
using Microsoft.Extensions.Logging;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Provides an access/integration point for logging within Schematic.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Retrieves a logging factory to enable logging within Schematic.
        /// </summary>
        public static ILoggerFactory Factory
        {
            get => _factory;
            set => _factory = value ?? throw new ArgumentNullException(nameof(value));
        }

        private static ILoggerFactory _factory = new LoggerFactory();
    }
}