using System;
using EnumsNET;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Configuration based extension methods for <see cref="ISchematicConnection"/> instances.
    /// </summary>
    public static class SchematicConnectionExtensions
    {
        /// <summary>
        /// Adds logging to a database connection. Logs when queries occur using the given database connection.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <param name="level">A logging level.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="loggerFactory"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="level"/> is not a valid enum value.</exception>
        public static void AddLogging(this ISchematicConnection connection, ILoggerFactory loggerFactory, LogLevel level)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(LogLevel) } provided must be a valid enum.", nameof(level));

            var logger = loggerFactory.CreateLogger("Schematic");
            Logging.AddLogging(connection, logger, level);
        }

        /// <summary>
        /// Removes the logging applied to a given connection.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public static void RemoveLogging(this ISchematicConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            Logging.RemoveLogging(connection);
        }

        /// <summary>
        /// Sets the maximum number of concurrent queries to perform with the given connection.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <param name="maxQueries">The maximum number of queries.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public static void SetMaxConcurrentQueries(this ISchematicConnection connection, uint maxQueries)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var maxQueryCount = maxQueries > 0 ? maxQueries : long.MaxValue;
            var semaphore = new AsyncSemaphore(maxQueryCount);
            QueryContext.SetMaxConcurrentQueries(connection.DbConnection, semaphore);
        }
    }
}
