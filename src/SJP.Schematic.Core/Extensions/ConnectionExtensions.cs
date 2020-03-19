using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using Nito.AsyncEx;

namespace SJP.Schematic.Core.Extensions
{
    public static class ConnectionExtensions
    {
        private static readonly ConditionalWeakTable<IDbConnection, AsyncSemaphore?> SemaphoreLookup = new ConditionalWeakTable<IDbConnection, AsyncSemaphore?>();

        public static void SetMaxConcurrentQueries(IDbConnection connection, AsyncSemaphore? semaphore)
        {
            SemaphoreLookup.AddOrUpdate(connection, semaphore);
        }

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

            using (var logger = new LoggingAdapter(connection, sql, null))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.QueryAsync<T>(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.QueryAsync<T>(command).ConfigureAwait(false);
                }
            }
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

            using (var logger = new LoggingAdapter(connection, sql, parameters))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.QueryAsync<T>(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.QueryAsync<T>(command).ConfigureAwait(false);
                }
            }
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

            using (var logger = new LoggingAdapter(connection, sql, null))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
                }
            }
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

            using (var logger = new LoggingAdapter(connection, sql, parameters))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
                }
            }
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

            using (var logger = new LoggingAdapter(connection, sql, null))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.ExecuteAsync(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.ExecuteAsync(command).ConfigureAwait(false);
                }
            }
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

            using (var logger = new LoggingAdapter(connection, sql, parameters))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.ExecuteAsync(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.ExecuteAsync(command).ConfigureAwait(false);
                }
            }
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

            using (var logger = new LoggingAdapter(connection, sql, null))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
                    return result != null
                        ? Option<T>.Some(result)
                        : Option<T>.None;
                }
                else
                {
                    var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
                    return result != null
                        ? Option<T>.Some(result)
                        : Option<T>.None;
                }
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

            using (var logger = new LoggingAdapter(connection, sql, parameters))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
                    return result != null
                        ? Option<T>.Some(result)
                        : Option<T>.None;
                }
                else
                {
                    var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
                    return result != null
                        ? Option<T>.Some(result)
                        : Option<T>.None;
                }
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

            using (var logger = new LoggingAdapter(connection, sql, null))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
                }
            }
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

            using (var logger = new LoggingAdapter(connection, sql, parameters))
            {
                if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                {
                    using var _ = await semaphore.LockAsync();
                    return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
                }
                else
                {
                    return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
                }
            }
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

                using (var logger = new LoggingAdapter(connection, sql, null))
                {
                    if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                    {
                        using var _ = await semaphore.LockAsync();
                        var result = await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);
                        return result != null
                            ? Option<T>.Some(result)
                            : Option<T>.None;
                    }
                    else
                    {
                        var result = await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);
                        return result != null
                            ? Option<T>.Some(result)
                            : Option<T>.None;
                    }
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

                using (var logger = new LoggingAdapter(connection, sql, parameters))
                {
                    if (SemaphoreLookup.TryGetValue(connection, out var semaphore) && semaphore != null)
                    {
                        using var _ = await semaphore.LockAsync();
                        var result = await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);
                        return result != null
                            ? Option<T>.Some(result)
                            : Option<T>.None;
                    }
                    else
                    {
                        var result = await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);
                        return result != null
                            ? Option<T>.Some(result)
                            : Option<T>.None;
                    }
                }
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }
    }
}