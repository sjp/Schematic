using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Provides an access/integration point for logging within Schematic.
    /// </summary>
    public static class Logging
    {
        public delegate void LogCommandExecuting(Guid commandId, string sql, IReadOnlyDictionary<string, object> parameters);

        public delegate void LogCommandExecuted(Guid commandId, string sql, IReadOnlyDictionary<string, object> parameters, TimeSpan duration);

        public static void OnCommandExecuting(this IDbConnection connection, LogCommandExecuting handler)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _ = CommandExecutingLookup.AddOrUpdate(connection, handler, (_, __) => handler);
        }

        public static void ClearCommandExecuting(this IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _ = CommandExecutingLookup.TryRemove(connection, out _);
        }

        public static void OnCommandExecuted(this IDbConnection connection, LogCommandExecuted handler)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _ = CommandExecutedLookup.AddOrUpdate(connection, handler, (_, __) => handler);
        }

        public static void ClearCommandExecuted(this IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _ = CommandExecutedLookup.TryRemove(connection, out _);
        }

        internal static bool IsCommandExecutingLogged(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return CommandExecutingLookup.ContainsKey(connection);
        }

        internal static bool IsCommandExecutedLogged(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return CommandExecutedLookup.ContainsKey(connection);
        }

        internal static void SchematicLogCommandExecuting(IDbConnection connection, Guid commandId, string sql, object? param)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (!CommandExecutingLookup.TryGetValue(connection, out var handler))
                return;

            var lookup = ToDictionary(param);
            handler.Invoke(commandId, sql, lookup);
        }

        internal static void SchematicLogCommandExecuted(IDbConnection connection, Guid commandId, string sql, object? param, TimeSpan duration)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (!CommandExecutedLookup.TryGetValue(connection, out var handler))
                return;

            var lookup = ToDictionary(param);
            handler.Invoke(commandId, sql, lookup, duration);
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

        private static readonly IReadOnlyDictionary<string, object> EmptyParams = new Dictionary<string, object>();
        private static readonly ConcurrentDictionary<IDbConnection, LogCommandExecuting> CommandExecutingLookup = new ConcurrentDictionary<IDbConnection, LogCommandExecuting>();
        private static readonly ConcurrentDictionary<IDbConnection, LogCommandExecuted> CommandExecutedLookup = new ConcurrentDictionary<IDbConnection, LogCommandExecuted>();
    }
}