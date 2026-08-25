using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// An async-safe cache, intended for caching results of database queries within a given query context.
/// </summary>
/// <typeparam name="TKey">The type of the key to cache.</typeparam>
/// <typeparam name="TValue">The type of value that will be cached.</typeparam>
/// <typeparam name="TCache">A container type used to access other cached values if required.</typeparam>
/// <remarks>
/// Only successful results are cached. A factory invocation that fails is not retained, so the next
/// request for that key runs the factory again instead of replaying the failure.
/// </remarks>
public class AsyncCache<TKey, TValue, TCache>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncCache{TKey, TValue, TCache}"/> class.
    /// </summary>
    /// <param name="factory">A value factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null" />.</exception>
    public AsyncCache(Func<TKey, TCache, CancellationToken, Task<TValue>> factory)
    {
        _query = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Retrieves a value for a key asynchronously and caches the result.
    /// </summary>
    /// <param name="key">The key to use as a cache key.</param>
    /// <param name="cache">A cache container, used to enable access to other cached results.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that will contain the value of the key once completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="cache"/> is <see langword="null" />.</exception>
    /// <remarks>
    /// <para>
    /// The factory method runs at most once per key for as long as it keeps succeeding. Concurrent
    /// callers for the same key share a single factory invocation; if that invocation fails, the
    /// result is discarded and the next caller runs the factory again.
    /// </para>
    /// <para>
    /// Because the factory invocation is shared, it cannot be cancelled on behalf of any one caller.
    /// The factory therefore receives <see cref="CancellationToken.None"/>, and
    /// <paramref name="cancellationToken"/> instead cancels this caller's wait for the shared result.
    /// A cancelled wait leaves the shared invocation running for the remaining callers.
    /// </para>
    /// <para>
    /// Similarly, the <paramref name="cache"/> container given to the factory is the one supplied by
    /// the caller that started the invocation for a key.
    /// </para>
    /// </remarks>
    public Task<TValue> GetByKeyAsync(TKey key, TCache cache, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(cache);

        return _cache.GetOrAdd(
            key,
            key => new AsyncLazy<TValue>(
                () => _query.Invoke(key, cache, CancellationToken.None),
                AsyncLazyFlags.RetryOnFailure
            )
        ).Task.WaitAsync(cancellationToken);
    }

    private readonly ConcurrentDictionary<TKey, AsyncLazy<TValue>> _cache = new();
    private readonly Func<TKey, TCache, CancellationToken, Task<TValue>> _query;
}
