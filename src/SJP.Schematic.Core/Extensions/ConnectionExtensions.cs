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
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            return connection.QueryAsync<T>(command);
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

            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return connection.QueryAsync<T>(command);
        }

        public static Task<T> ExecuteScalarAsync<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            return connection.ExecuteScalarAsync<T>(command);
        }

        public static Task<T> ExecuteScalarAsync<T>(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return connection.ExecuteScalarAsync<T>(command);
        }

        public static Task<int> ExecuteAsync(this IDbConnection connection, string sql, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            return connection.ExecuteAsync(command);
        }

        public static Task<int> ExecuteAsync(this IDbConnection connection, string sql, object parameters, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return connection.ExecuteAsync(command);
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
            var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
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
            var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnection connection, string sql, CancellationToken cancellationToken)
            where T : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            return connection.QuerySingleAsync<T>(command);
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

            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return connection.QuerySingleAsync<T>(command);
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