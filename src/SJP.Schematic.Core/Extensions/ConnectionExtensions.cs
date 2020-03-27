using System;
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
        public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : notnull
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

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, null)).ConfigureAwait(false);
            return await connection.QueryAsync<T>(command).ConfigureAwait(false);
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
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

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, parameters)).ConfigureAwait(false);
            return await connection.QueryAsync<T>(command).ConfigureAwait(false);
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

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, null)).ConfigureAwait(false);
            return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
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

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, parameters)).ConfigureAwait(false);
            return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
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

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, null)).ConfigureAwait(false);
            return await connection.ExecuteAsync(command).ConfigureAwait(false);
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

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, parameters)).ConfigureAwait(false);
            return await connection.ExecuteAsync(command).ConfigureAwait(false);
        }

        public static OptionAsync<T> QueryFirstOrNone<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QueryFirstOrNoneAsyncCore<T>(connection, sql, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QueryFirstOrNoneAsyncCore<T>(IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, null)).ConfigureAwait(false);
            var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        public static OptionAsync<T> QueryFirstOrNone<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
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
            where T : notnull
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, parameters)).ConfigureAwait(false);
            var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QuerySingleAsyncCore<T>(connection, sql, cancellationToken);
        }

        private static async Task<T> QuerySingleAsyncCore<T>(IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, null)).ConfigureAwait(false);
            return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
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
            where T : notnull
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, parameters)).ConfigureAwait(false);
            return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
        }

        public static OptionAsync<T> QuerySingleOrNone<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QuerySingleOrNoneAsyncCore<T>(connection, sql, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QuerySingleOrNoneAsyncCore<T>(IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            try
            {
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

                using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, null)).ConfigureAwait(false);
                var result = await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);
                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }

        public static OptionAsync<T> QuerySingleOrNone<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
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
            where T : notnull
        {
            try
            {
                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

                using var context = await QueryContext.CreateAsync(connection, new QueryLoggingContext(connection, sql, parameters)).ConfigureAwait(false);
                var result = await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);
                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }
    }
}