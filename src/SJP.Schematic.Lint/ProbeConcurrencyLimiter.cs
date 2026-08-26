using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// Bounds the number of data-probing queries in flight against a database at any one time. Rules that
/// probe live data start one query per table (or per batch of columns), and a linter runs every rule
/// concurrently, so without a bound a large schema would launch thousands of simultaneous queries.
/// </summary>
internal sealed class ProbeConcurrencyLimiter
{
    /// <summary>
    /// The number of data-probing queries permitted to be in flight at once. Deliberately modest: probes
    /// are short, so a small number of them keeps a connection pool busy without exhausting it.
    /// </summary>
    public const int MaxConcurrentQueries = 8;

    /// <summary>
    /// Retrieves the limiter associated with a connection, creating one when needed. Limiters are shared
    /// so that every rule probing the same database draws from a single pool of permits, rather than each
    /// rule being free to add its own queries on top of the others'.
    /// </summary>
    /// <param name="connection">A database connection, qualified with a dialect.</param>
    /// <returns>A limiter bound to <paramref name="connection"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public static ProbeConcurrencyLimiter GetForConnection(ISchematicConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return Limiters.GetValue(connection, static _ => new ProbeConcurrencyLimiter());
    }

    private ProbeConcurrencyLimiter()
    {
    }

    /// <summary>
    /// Runs a query once a permit is available, releasing the permit when the query completes.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="query">A query to run.</param>
    /// <param name="cancellationToken">A cancellation token, observed while waiting for a permit as well as by the query.</param>
    /// <returns>The result of <paramref name="query"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is <see langword="null" />.</exception>
    public Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return RunAsyncCore(query, cancellationToken);
    }

    private async Task<T> RunAsyncCore<T>(Func<CancellationToken, Task<T>> query, CancellationToken cancellationToken)
    {
        await _queryLock.WaitAsync(cancellationToken);
        try
        {
            return await query(cancellationToken);
        }
        finally
        {
            _queryLock.Release();
        }
    }

    private readonly SemaphoreSlim _queryLock = new(MaxConcurrentQueries, MaxConcurrentQueries);

    private static readonly ConditionalWeakTable<ISchematicConnection, ProbeConcurrencyLimiter> Limiters = new();
}
