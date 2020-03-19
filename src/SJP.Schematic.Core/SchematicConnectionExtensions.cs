using System;
using EnumsNET;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace SJP.Schematic.Core
{
    public static class SchematicConnectionExtensions
    {
        public static void AddLogging(this ISchematicConnection connection, ILogger logger, LogLevel level)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(LogLevel) } provided must be a valid enum.", nameof(level));

            Logging.AddLogging(connection, logger, level);
        }

        public static void RemoveLogging(this ISchematicConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            Logging.RemoveLogging(connection);
        }

        /// <summary>
        /// Sets the maximum number of concurrent queries to perform with the given connection.
        /// </summary>
        /// <param name="maxQueries">The maximum number of queries.</param>
        public static void SetMaxConcurrentQueries(this ISchematicConnection connection, uint maxQueries)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var semaphore = maxQueries > 0 ? new AsyncSemaphore(maxQueries) : null;
            Extensions.ConnectionExtensions.SetMaxConcurrentQueries(connection.DbConnection, semaphore);
        }
    }
}
