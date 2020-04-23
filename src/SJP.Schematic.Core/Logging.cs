using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using EnumsNET;
using Microsoft.Extensions.Logging;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Provides an access/integration point for logging within Schematic. Not intended be used directly.
    /// </summary>
    public static class Logging
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyParams = new Dictionary<string, object>();
        private static readonly ConditionalWeakTable<IDbConnectionFactory, LoggingConfiguration> ConnectionLoggerLookup = new ConditionalWeakTable<IDbConnectionFactory, LoggingConfiguration>();

        /// <summary>
        /// Determines whether logging is currently configured on the given connection factory.
        /// </summary>
        /// <param name="connectionFactory">A database connection factory.</param>
        /// <returns><c>true</c> if logging is configured on the connection; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>.</exception>
        public static bool IsLoggingConfigured(IDbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));

            return ConnectionLoggerLookup.TryGetValue(connectionFactory, out var config) && config != null;
        }

        /// <summary>
        /// Enables query logging on a database connection for Schematic queries.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="logLevel">The log level that should be applied to query logs.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="logLevel"/> is an invalid value.</exception>
        public static void AddLogging(ISchematicConnection connection, ILogger logger, LogLevel logLevel)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (!logLevel.IsValid())
                throw new ArgumentException($"The { nameof(LogLevel) } provided must be a valid enum.", nameof(logLevel));

            var loggingConfig = new LoggingConfiguration(logger, logLevel);
            ConnectionLoggerLookup.AddOrUpdate(connection.DbConnection, loggingConfig);
        }

        /// <summary>
        /// Removes logging from a database connection.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public static void RemoveLogging(ISchematicConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (ConnectionLoggerLookup.TryGetValue(connection.DbConnection, out _))
                ConnectionLoggerLookup.Remove(connection.DbConnection);
        }

        internal static void LogCommandExecuting(IDbConnectionFactory connectionFactory, Guid commandId, string sql, object? param)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (!ConnectionLoggerLookup.TryGetValue(connectionFactory, out var loggingConfig))
                return;
            if (!ConnectionRegistry.TryGetConnectionId(connectionFactory, out var connectionId))
                connectionId = Guid.Empty;

            var parameters = ToDictionary(param);

            loggingConfig.Logger.Log(
                loggingConfig.LogLevel,
                "Connection {connectionId} is executing query {commandId}. Attempting to execute {sql} with parameters {@parameters}.",
                connectionId,
                commandId,
                sql,
                parameters
            );
        }

        internal static void LogCommandExecuted(IDbConnectionFactory connectionFactory, Guid commandId, string sql, object? param, TimeSpan duration)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (!ConnectionLoggerLookup.TryGetValue(connectionFactory, out var loggingConfig))
                return;
            if (!ConnectionRegistry.TryGetConnectionId(connectionFactory, out var connectionId))
                connectionId = Guid.Empty;

            var parameters = ToDictionary(param);
            loggingConfig.Logger.Log(
                loggingConfig.LogLevel,
                "Connection {connectionId} completed executing query {commandId}. Query {sql} with parameters {@parameters} took {duration}ms to execute.",
                connectionId,
                commandId,
                sql,
                parameters,
                (ulong)duration.TotalMilliseconds
            );
        }

        private static IReadOnlyDictionary<string, object> ToDictionary(object? param)
        {
            if (param == null)
                return EmptyParams;

            var objType = param.GetType();
            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (properties.Empty())
                return EmptyParams;

            var result = new Dictionary<string, object>(properties.Length);

            foreach (var prop in properties)
            {
                var propName = prop.Name;
                result[propName] = prop.GetValue(param);
            }

            return result;
        }
    }
}