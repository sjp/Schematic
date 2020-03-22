using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Provides an access/integration point for logging within Schematic.
    /// </summary>
    public static class Logging
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyParams = new Dictionary<string, object>();
        private static readonly ConditionalWeakTable<IDbConnection, LoggingConfiguration> ConnectionLoggerLookup = new ConditionalWeakTable<IDbConnection, LoggingConfiguration>();

        public static bool IsLoggingConfigured(IDbConnection connection) => ConnectionLoggerLookup.TryGetValue(connection, out var config) && config != null;

        public static void AddLogging(ISchematicConnection connection, ILogger logger, LogLevel level)
        {
            var loggingConfig = new LoggingConfiguration(logger, level);
            ConnectionLoggerLookup.AddOrUpdate(connection.DbConnection, loggingConfig);
        }

        public static void RemoveLogging(ISchematicConnection connection)
        {
            if (ConnectionLoggerLookup.TryGetValue(connection.DbConnection, out _))
                ConnectionLoggerLookup.Remove(connection.DbConnection);
        }

        internal static void LogCommandExecuting(IDbConnection connection, Guid commandId, string sql, object? param)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (!ConnectionLoggerLookup.TryGetValue(connection, out var loggingConfig))
                return;
            if (!ConnectionRegistry.TryGetConnectionId(connection, out var connectionId))
                connectionId = Guid.Empty;

            var parameters = ToDictionary(param);
            loggingConfig.Logger.Log(
                loggingConfig.Level,
                "Connection {connectionId} is executing query {commandId}. Attempting to execute {sql} with parameters {@parameters}.",
                connectionId,
                commandId,
                sql,
                parameters
            );
        }

        internal static void LogCommandExecuted(IDbConnection connection, Guid commandId, string sql, object? param, TimeSpan duration)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (!ConnectionLoggerLookup.TryGetValue(connection, out var loggingConfig))
                return;
            if (!ConnectionRegistry.TryGetConnectionId(connection, out var connectionId))
                connectionId = Guid.Empty;

            var parameters = ToDictionary(param);
            loggingConfig.Logger.Log(
                loggingConfig.Level,
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

            var result = new Dictionary<string, object>();

            foreach (var prop in properties)
            {
                var propName = prop.Name;
                result[propName] = prop.GetValue(param);
            }

            return result;
        }
    }
}