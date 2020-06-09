using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace SJP.Schematic.Core.Extensions
{
    /// <summary>
    /// Extension methods used to query databases. When called, enables Schematic to enhance queries with logging, query limiting, etc.
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Queries the database and returns a collection of results.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of query results from the database.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c> or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.QueryAsync<T>(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Queries the database and returns a collection of results.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="parameters">Parameters for the associated SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of query results from the database.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, parameters);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.QueryAsync<T>(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Queries the database and returns a scalar result.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A single scalar value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
        /// <remarks>If the results contain more than one column or row, the value of the first column of the first row is taken.</remarks>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.ExecuteScalarAsync<T>(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Queries the database and returns a scalar result.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="parameters">Parameters for the associated SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A single scalar value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
        /// <remarks>If the results contain more than one column or row, the value of the first column of the first row is taken.</remarks>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, parameters);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.ExecuteScalarAsync<T>(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query against the database without observing its result.
        /// </summary>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing the number of rows affected by the given query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.ExecuteAsync(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query against the database without observing its result.
        /// </summary>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="parameters">Parameters for the associated SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing the number of rows affected by the given query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, parameters);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.ExecuteAsync(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Queries the database and returns a scalar result if available, otherwise none.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A single scalar value if one is available, otherwise none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
        /// <remarks>If the results contain more than one column or row, the value of the first column of the first row is taken. If there are no results, no results are provided.</remarks>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            var result = await retryPolicy.ExecuteAsync(_ => connection.QueryFirstOrDefaultAsync<T>(command), cancellationToken).ConfigureAwait(false);

            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        /// <summary>
        /// Queries the database and returns a scalar result if available, otherwise none.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="parameters">Parameters for the associated SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A single scalar value if one is available, otherwise none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
        /// <remarks>If the results contain more than one column or row, the value of the first column of the first row is taken. If there are no results, no results are provided.</remarks>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, parameters);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            var result = await retryPolicy.ExecuteAsync(_ => connection.QueryFirstOrDefaultAsync<T>(command), cancellationToken).ConfigureAwait(false);

            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        /// <summary>
        /// Queries the database and returns the only result.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing a single row of data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.QuerySingleAsync<T>(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Queries the database and returns the only result.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="parameters">Parameters for the associated SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A single row of data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

            var loggingContext = new QueryLoggingContext(connectionFactory, sql, parameters);
            using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
            return await retryPolicy.ExecuteAsync(_ => connection.QuerySingleAsync<T>(command), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Queries the database and returns the only result. If no results or too many results are present, a 'none' value is returned.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing single row of data if only one row is returned, otherwise none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

                var loggingContext = new QueryLoggingContext(connectionFactory, sql, null);
                using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
                var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
                await using var _ = connection.WithDispose(connectionFactory);

                var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
                var result = await retryPolicy.ExecuteAsync(_ => connection.QuerySingleOrDefaultAsync<T>(command), cancellationToken).ConfigureAwait(false);

                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }

        /// <summary>
        /// Queries the database and returns the only result. If no results or too many results are present, a 'none' value is returned.
        /// </summary>
        /// <typeparam name="T">The type of results to map to.</typeparam>
        /// <param name="connectionFactory">A connection factory.</param>
        /// <param name="sql">The SQL to query with.</param>
        /// <param name="parameters">Parameters for the associated SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing single row of data if only one row is returned, otherwise none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
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

                var loggingContext = new QueryLoggingContext(connectionFactory, sql, parameters);
                using var context = await QueryContext.CreateAsync(connectionFactory, loggingContext, cancellationToken).ConfigureAwait(false);
                var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
                await using var _ = connection.WithDispose(connectionFactory);

                var retryPolicy = BuildRetryPolicy(connectionFactory, loggingContext);
                var result = await retryPolicy.ExecuteAsync(_ => connection.QuerySingleOrDefaultAsync<T>(command), cancellationToken).ConfigureAwait(false);

                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }

        private static IAsyncPolicy BuildRetryPolicy(IDbConnectionFactory connectionFactory, QueryLoggingContext loggingContext)
        {
            return connectionFactory.RetryPolicy.WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), MaxRetryAttempts),
                (ex, delay, retryAttempt, ctx) => loggingContext.Retry(retryAttempt, delay)
            );
        }

        private const int MaxRetryAttempts = 5;

        private static IAsyncDisposable WithDispose(this IDbConnection connection, IDbConnectionFactory factory) => new ConnectionDisposer(connection, factory);

        private sealed class ConnectionDisposer : IAsyncDisposable
        {
            public ConnectionDisposer(IDbConnection connection, IDbConnectionFactory factory)
            {
                if (factory == null)
                    throw new ArgumentNullException(nameof(factory));

                _connection = connection ?? throw new ArgumentNullException(nameof(connection));
                _shouldDispose = factory.DisposeConnection;
            }

            public async ValueTask DisposeAsync()
            {
                if (_disposed)
                    return;

                if (_shouldDispose)
                {
                    if (_connection is DbConnection dbConnection)
                        await dbConnection.DisposeAsync().ConfigureAwait(false);
                    else
                        _connection.Dispose();

                    _disposed = true;
                }
            }

            private bool _disposed;
            private readonly IDbConnection _connection;
            private readonly bool _shouldDispose;
        }
    }
}