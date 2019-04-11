using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;

namespace SJP.Schematic.Core.Extensions
{
    public static class ConnectionExtensions
    {
        public static void ConfigureSchematicCommand(this IDbConnection connection, Func<CommandDefinition> configure)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            CommandDefinition wrapper(CommandDefinition _) => configure.Invoke();

            _ = CommandConfigurationLookup.AddOrUpdate(connection, wrapper, (_, __) => wrapper);
        }

        public static void ConfigureSchematicCommand(this IDbConnection connection, Func<CommandDefinition, CommandDefinition> configure)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            _ = CommandConfigurationLookup.AddOrUpdate(connection, configure, (_, __) => configure);
        }

        public static void ClearSchematicCommandConfiguration(this IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _ = CommandConfigurationLookup.TryRemove(connection, out _);
        }

        private static CommandDefinition ConfigureCommand(IDbConnection connection, CommandDefinition command)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return CommandConfigurationLookup.TryGetValue(connection, out var configure) && configure != null
                ? configure.Invoke(command)
                : command;
        }

        private static readonly ConcurrentDictionary<IDbConnection, Func<CommandDefinition, CommandDefinition>> CommandConfigurationLookup = new ConcurrentDictionary<IDbConnection, Func<CommandDefinition, CommandDefinition>>();

        public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QueryAsyncCore<T>(connection, sql, cancellationToken);
        }

        private static async Task<IEnumerable<T>> QueryAsyncCore<T>(IDbConnection connection, string sql, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, null))
                return await connection.QueryAsync<T>(configuredCommand).ConfigureAwait(false);
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QueryAsyncCore<T>(connection, sql, parameters, cancellationToken);
        }

        private static async Task<IEnumerable<T>> QueryAsyncCore<T>(IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, parameters))
                return await connection.QueryAsync<T>(configuredCommand).ConfigureAwait(false);
        }

        public static T ExecuteFirstScalar<T>(this IDbConnection connection, string sql)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            var command = new CommandDefinition(sql);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, null))
                return connection.ExecuteScalar<T>(configuredCommand);
        }

        public static T ExecuteFirstScalar<T>(this IDbConnection connection, string sql, object parameters)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var command = new CommandDefinition(sql, parameters);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, parameters))
                return connection.ExecuteScalar<T>(configuredCommand);
        }

        public static Task<T> ExecuteScalarAsync<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return ExecuteScalarAsyncCore<T>(connection, sql, cancellationToken);
        }

        private static async Task<T> ExecuteScalarAsyncCore<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, null))
                return await connection.ExecuteScalarAsync<T>(configuredCommand).ConfigureAwait(false);
        }

        public static Task<T> ExecuteScalarAsync<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return ExecuteScalarAsyncCore<T>(connection, sql, parameters, cancellationToken);
        }

        private static async Task<T> ExecuteScalarAsyncCore<T>(IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, parameters))
                return await connection.ExecuteScalarAsync<T>(configuredCommand).ConfigureAwait(false);
        }

        public static Task<int> ExecuteAsync(this IDbConnection connection, string sql, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return ExecuteAsyncCore(connection, sql, cancellationToken);
        }

        private static async Task<int> ExecuteAsyncCore(IDbConnection connection, string sql, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, null))
                return await connection.ExecuteAsync(configuredCommand).ConfigureAwait(false);
        }

        public static Task<int> ExecuteAsync(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return ExecuteAsyncCore(connection, sql, parameters, cancellationToken);
        }

        private static async Task<int> ExecuteAsyncCore(IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, parameters))
                return await connection.ExecuteAsync(configuredCommand).ConfigureAwait(false);
        }

        public static OptionAsync<T> QueryFirstOrNone<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QueryFirstOrNoneAsyncCore<T>(connection, sql, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QueryFirstOrNoneAsyncCore<T>(IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, null))
            {
                var result = await connection.QueryFirstOrDefaultAsync<T>(configuredCommand).ConfigureAwait(false);
                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
        }

        public static OptionAsync<T> QueryFirstOrNone<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QueryFirstOrNoneAsyncCore<T>(connection, sql, parameters, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QueryFirstOrNoneAsyncCore<T>(IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : class
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, parameters))
            {
                var result = await connection.QueryFirstOrDefaultAsync<T>(configuredCommand).ConfigureAwait(false);
                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QuerySingleAsyncCore<T>(connection, sql, cancellationToken);
        }

        private static async Task<T> QuerySingleAsyncCore<T>(IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, null))
                return await connection.QuerySingleAsync<T>(configuredCommand).ConfigureAwait(false);
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QuerySingleAsyncCore<T>(connection, sql, parameters, cancellationToken);
        }

        private static async Task<T> QuerySingleAsyncCore<T>(IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : class
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            var configuredCommand = ConfigureCommand(connection, command);

            using (var logger = new LoggingAdapter(connection, sql, parameters))
                return await connection.QuerySingleAsync<T>(configuredCommand).ConfigureAwait(false);
        }

        public static OptionAsync<T> QuerySingleOrNone<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QuerySingleOrNoneAsyncCore<T>(connection, sql, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QuerySingleOrNoneAsyncCore<T>(IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                var configuredCommand = ConfigureCommand(connection, command);

                using (var logger = new LoggingAdapter(connection, sql, null))
                {
                    var result = await connection.QuerySingleOrDefaultAsync<T>(configuredCommand).ConfigureAwait(false);
                    return result != null
                        ? Option<T>.Some(result)
                        : Option<T>.None;
                }
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }

        public static OptionAsync<T> QuerySingleOrNone<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QuerySingleOrNoneAsyncCore<T>(connection, sql, parameters, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QuerySingleOrNoneAsyncCore<T>(IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
                var configuredCommand = ConfigureCommand(connection, command);

                using (var logger = new LoggingAdapter(connection, sql, parameters))
                {
                    var result = await connection.QuerySingleOrDefaultAsync<T>(configuredCommand).ConfigureAwait(false);
                    return result != null
                        ? Option<T>.Some(result)
                        : Option<T>.None;
                }
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }
    }
}