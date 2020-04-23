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
        public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QueryAsyncCore<T>(connectionFactory, sql, cancellationToken);
        }

        private static async Task<IEnumerable<T>> QueryAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, null)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.QueryAsync<T>(command).ConfigureAwait(false);
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QueryAsyncCore<T>(connectionFactory, sql, parameters, cancellationToken);
        }

        private static async Task<IEnumerable<T>> QueryAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, parameters)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.QueryAsync<T>(command).ConfigureAwait(false);
        }

        public static Task<T> ExecuteScalarAsync<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return ExecuteScalarAsyncCore<T>(connectionFactory, sql, cancellationToken);
        }

        private static async Task<T> ExecuteScalarAsyncCore<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, null)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
        }

        public static Task<T> ExecuteScalarAsync<T>(this IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return ExecuteScalarAsyncCore<T>(connectionFactory, sql, parameters, cancellationToken);
        }

        private static async Task<T> ExecuteScalarAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, parameters)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.ExecuteScalarAsync<T>(command).ConfigureAwait(false);
        }

        public static Task<int> ExecuteAsync(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return ExecuteAsyncCore(connectionFactory, sql, cancellationToken);
        }

        private static async Task<int> ExecuteAsyncCore(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, null)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.ExecuteAsync(command).ConfigureAwait(false);
        }

        public static Task<int> ExecuteAsync(this IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return ExecuteAsyncCore(connectionFactory, sql, parameters, cancellationToken);
        }

        private static async Task<int> ExecuteAsyncCore(IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, parameters)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.ExecuteAsync(command).ConfigureAwait(false);
        }

        public static OptionAsync<T> QueryFirstOrNone<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QueryFirstOrNoneAsyncCore<T>(connectionFactory, sql, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QueryFirstOrNoneAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, null)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        public static OptionAsync<T> QueryFirstOrNone<T>(this IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QueryFirstOrNoneAsyncCore<T>(connectionFactory, sql, parameters, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QueryFirstOrNoneAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, parameters)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            var result = await connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QuerySingleAsyncCore<T>(connectionFactory, sql, cancellationToken);
        }

        private static async Task<T> QuerySingleAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, null)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
        }

        public static Task<T> QuerySingleAsync<T>(this IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QuerySingleAsyncCore<T>(connectionFactory, sql, parameters, cancellationToken);
        }

        private static async Task<T> QuerySingleAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, parameters)).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var _ = connection.WithDispose(connectionFactory);

            return await connection.QuerySingleAsync<T>(command).ConfigureAwait(false);
        }

        public static OptionAsync<T> QuerySingleOrNone<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QuerySingleOrNoneAsyncCore<T>(connectionFactory, sql, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QuerySingleOrNoneAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
            where T : notnull
        {
            try
            {
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

                using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, null)).ConfigureAwait(false);
                var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
                using var _ = connection.WithDispose(connectionFactory);

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

        public static OptionAsync<T> QuerySingleOrNone<T>(this IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return QuerySingleOrNoneAsyncCore<T>(connectionFactory, sql, parameters, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> QuerySingleOrNoneAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
            where T : notnull
        {
            try
            {
                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

                using var context = await QueryContext.CreateAsync(connectionFactory, new QueryLoggingContext(connectionFactory, sql, parameters)).ConfigureAwait(false);
                var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
                using var _ = connection.WithDispose(connectionFactory);

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

        private static IDisposable WithDispose(this IDbConnection connection, IDbConnectionFactory factory) => new ConnectionDisposer(connection, factory);

        private sealed class ConnectionDisposer : IDisposable
        {
            public ConnectionDisposer(IDbConnection connection, IDbConnectionFactory factory)
            {
                if (factory == null)
                    throw new ArgumentNullException(nameof(factory));

                _connection = connection ?? throw new ArgumentNullException(nameof(connection));
                _shouldDispose = factory.DisposeConnection;
            }

            public void Dispose()
            {
                if (_shouldDispose)
                    _connection.Dispose();
            }

            private readonly IDbConnection _connection;
            private readonly bool _shouldDispose;
        }
    }
}