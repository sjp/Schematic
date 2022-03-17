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
public class AsyncCache<TKey, TValue, TCache>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncCache{TKey, TValue, TCache}"/> class.
    /// </summary>
    /// <param name="factory">A value factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
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
    /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="cache"/> is <c>null</c>.</exception>
    /// <remarks>This method guarantees that the factory method is only called once.</remarks>
    public Task<TValue> GetByKeyAsync(TKey key, TCache cache, CancellationToken cancellationToken = default)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (cache == null)
            throw new ArgumentNullException(nameof(cache));

        return _cache.GetOrAdd(
            key,
            key => new AsyncLazy<TValue>(() => _query.Invoke(key, cache, cancellationToken))
        ).Task;
    }

    private readonly ConcurrentDictionary<TKey, AsyncLazy<TValue>> _cache = new();
    private readonly Func<TKey, TCache, CancellationToken, Task<TValue>> _query;
}
