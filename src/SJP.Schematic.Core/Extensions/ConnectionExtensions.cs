using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace SJP.Schematic.Core.Extensions;

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
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));

        return QueryAsyncCore<T>(connectionFactory, sql, cancellationToken);
    }

    private static async Task<IEnumerable<T>> QueryAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
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
    public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnectionFactory connectionFactory, string sql, ISqlQuery<T> parameters, CancellationToken cancellationToken)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));
        ArgumentNullException.ThrowIfNull(parameters);

        return QueryAsyncCore(connectionFactory, sql, parameters, cancellationToken);
    }

    private static async Task<IEnumerable<T>> QueryAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, ISqlQuery<T> parameters, CancellationToken cancellationToken)
        where T : notnull
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        return await retryPolicy.ExecuteAsync(_ => connection.QueryAsync<T>(command), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Queries the database and returns a collection of results.
    /// </summary>
    /// <typeparam name="T">The type of results to map to.</typeparam>
    /// <param name="connectionFactory">A connection factory.</param>
    /// <param name="sql">The SQL to query with.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of query results from the database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c> or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
    public static IAsyncEnumerable<T> QueryEnumerableAsync<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));

        return QueryEnumerableAsyncCore<T>(connectionFactory, sql, cancellationToken);
    }

    private static async IAsyncEnumerable<T> QueryEnumerableAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);

        var source = connection.QueryUnbufferedAsync<T>(sql).WithRetryPolicy(retryPolicy, cancellationToken);
        await foreach (var item in source.WithCancellation(cancellationToken))
            yield return item;
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
    public static IAsyncEnumerable<T> QueryEnumerableAsync<T>(this IDbConnectionFactory connectionFactory, string sql, ISqlQuery<T> parameters, CancellationToken cancellationToken)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));
        ArgumentNullException.ThrowIfNull(parameters);

        return QueryEnumerableAsyncCore(connectionFactory, sql, parameters, cancellationToken);
    }

    private static async IAsyncEnumerable<T> QueryEnumerableAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, ISqlQuery<T> parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : notnull
    {
        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        var source = connection.QueryUnbufferedAsync<T>(sql, parameters).WithRetryPolicy(retryPolicy, cancellationToken);
        await foreach (var item in source.WithCancellation(cancellationToken))
            yield return item;
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
    public static Task<T?> ExecuteScalarAsync<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));

        return ExecuteScalarAsyncCore<T?>(connectionFactory, sql, cancellationToken);
    }

    private static async Task<T?> ExecuteScalarAsyncCore<T>(this IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        return await retryPolicy.ExecuteAsync(_ => connection.ExecuteScalarAsync<T>(command), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Queries the database and returns a scalar result.
    /// </summary>
    /// <typeparam name="TResult">The type of results to map to.</typeparam>
    /// <param name="connectionFactory">A connection factory.</param>
    /// <param name="sql">The SQL to query with.</param>
    /// <param name="parameters">Parameters for the associated SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A single scalar value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <remarks>If the results contain more than one column or row, the value of the first column of the first row is taken.</remarks>
    public static Task<TResult?> ExecuteScalarAsync<TResult>(this IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));
        ArgumentNullException.ThrowIfNull(parameters);

        return ExecuteScalarAsyncCore(connectionFactory, sql, parameters, cancellationToken);
    }

    private static async Task<TResult?> ExecuteScalarAsyncCore<TResult>(IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        return await retryPolicy.ExecuteAsync(_ => connection.ExecuteScalarAsync<TResult>(command), cancellationToken).ConfigureAwait(false);
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
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));

        return ExecuteAsyncCore(connectionFactory, sql, cancellationToken);
    }

    private static async Task<int> ExecuteAsyncCore(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
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
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));
        ArgumentNullException.ThrowIfNull(parameters);

        return ExecuteAsyncCore(connectionFactory, sql, parameters, cancellationToken);
    }

    private static async Task<int> ExecuteAsyncCore(IDbConnectionFactory connectionFactory, string sql, object parameters, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
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
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));

        return QueryFirstOrNoneAsyncCore<T>(connectionFactory, sql, cancellationToken).ToAsync();
    }

    private static async Task<Option<T>> QueryFirstOrNoneAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        where T : notnull
    {
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        var result = await retryPolicy.ExecuteAsync(_ => connection.QueryFirstOrDefaultAsync<T>(command), cancellationToken).ConfigureAwait(false);

        return result != null
            ? Option<T>.Some(result)
            : Option<T>.None;
    }

    /// <summary>
    /// Queries the database and returns a scalar result if available, otherwise none.
    /// </summary>
    /// <typeparam name="TResult">The type of results to map to.</typeparam>
    /// <param name="connectionFactory">A connection factory.</param>
    /// <param name="sql">The SQL to query with.</param>
    /// <param name="parameters">Parameters for the associated SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A single scalar value if one is available, otherwise none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <remarks>If the results contain more than one column or row, the value of the first column of the first row is taken. If there are no results, no results are provided.</remarks>
    public static OptionAsync<TResult> QueryFirstOrNone<TResult>(this IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));
        ArgumentNullException.ThrowIfNull(parameters);

        return QueryFirstOrNoneAsyncCore(connectionFactory, sql, parameters, cancellationToken).ToAsync();
    }

    private static async Task<Option<TResult>> QueryFirstOrNoneAsyncCore<TResult>(IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        var result = await retryPolicy.ExecuteAsync(_ => connection.QueryFirstOrDefaultAsync<TResult>(command), cancellationToken).ConfigureAwait(false);

        return result != null
            ? Option<TResult>.Some(result)
            : Option<TResult>.None;
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
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));

        return QuerySingleAsyncCore<T>(connectionFactory, sql, cancellationToken);
    }

    private static async Task<T> QuerySingleAsyncCore<T>(IDbConnectionFactory connectionFactory, string sql, CancellationToken cancellationToken)
        where T : notnull
    {
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        return await retryPolicy.ExecuteAsync(_ => connection.QuerySingleAsync<T>(command), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Queries the database and returns the only result.
    /// </summary>
    /// <typeparam name="TResult">The type of results to map to.</typeparam>
    /// <param name="connectionFactory">A connection factory.</param>
    /// <param name="sql">The SQL to query with.</param>
    /// <param name="parameters">Parameters for the associated SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A single row of data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
    public static Task<TResult> QuerySingleAsync<TResult>(this IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));
        ArgumentNullException.ThrowIfNull(parameters);

        return QuerySingleAsyncCore(connectionFactory, sql, parameters, cancellationToken);
    }

    private static async Task<TResult> QuerySingleAsyncCore<TResult>(IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var _ = connection.WithDispose(connectionFactory);

        var retryPolicy = BuildRetryPolicy(connectionFactory);
        return await retryPolicy.ExecuteAsync(_ => connection.QuerySingleAsync<TResult>(command), cancellationToken).ConfigureAwait(false);
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
        ArgumentNullException.ThrowIfNull(connectionFactory);
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

            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory);
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
    /// <typeparam name="TResult">The type of results to map to.</typeparam>
    /// <param name="connectionFactory">A connection factory.</param>
    /// <param name="sql">The SQL to query with.</param>
    /// <param name="parameters">Parameters for the associated SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing single row of data if only one row is returned, otherwise none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is <c>null</c>, <paramref name="parameters"/> is <c>null</c>, or <paramref name="sql"/> is <c>null</c>, empty, or whitespace.</exception>
    public static OptionAsync<TResult> QuerySingleOrNone<TResult>(this IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        if (sql.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(sql));
        ArgumentNullException.ThrowIfNull(parameters);

        return QuerySingleOrNoneAsyncCore(connectionFactory, sql, parameters, cancellationToken).ToAsync();
    }

    private static async Task<Option<TResult>> QuerySingleOrNoneAsyncCore<TResult>(IDbConnectionFactory connectionFactory, string sql, ISqlQuery<TResult> parameters, CancellationToken cancellationToken)
        where TResult : notnull
    {
        try
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var connection = await connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = connection.WithDispose(connectionFactory);

            var retryPolicy = BuildRetryPolicy(connectionFactory);
            var result = await retryPolicy.ExecuteAsync(_ => connection.QuerySingleOrDefaultAsync<TResult>(command), cancellationToken).ConfigureAwait(false);

            return result != null
                ? Option<TResult>.Some(result)
                : Option<TResult>.None;
        }
        catch (InvalidOperationException) // for > 1 case
        {
            return Option<TResult>.None;
        }
    }

    private static IAsyncPolicy BuildRetryPolicy(IDbConnectionFactory connectionFactory)
    {
        return connectionFactory.RetryPolicy.WaitAndRetryAsync(
            Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), MaxRetryAttempts)
        );
    }

    private static async IAsyncEnumerable<T> WithRetryPolicy<T>(this IAsyncEnumerable<T> source, IAsyncPolicy policy, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var enumerator = source.ConfigureAwait(false).WithCancellation(cancellationToken).GetAsyncEnumerator();

        try
        {
            bool hasNext;

            do
            {
                hasNext = await policy.ExecuteAsync(async () => await enumerator.MoveNextAsync());
                if (hasNext)
                    yield return enumerator.Current;
            }
            while (hasNext);
        }
        finally
        {
            await policy.ExecuteAsync(async () => await enumerator.DisposeAsync());
        }
    }

    private const int MaxRetryAttempts = 5;

    private static IAsyncDisposable WithDispose(this IDbConnection connection, IDbConnectionFactory factory) => new ConnectionDisposer(connection, factory);

    private sealed class ConnectionDisposer : IAsyncDisposable
    {
        public ConnectionDisposer(IDbConnection connection, IDbConnectionFactory factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _shouldDispose = factory.DisposeConnection;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            if (_shouldDispose)
            {
                if (_connection is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
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