using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SJP.Schematic.Core.Utilities
{
    public class AsyncCache<TKey, TValue, TCache>
    {
        public AsyncCache(Func<TKey, TCache, CancellationToken, Task<TValue>> factory)
        {
            _query = factory ?? throw new ArgumentNullException(nameof(factory));
        }

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

        private readonly ConcurrentDictionary<TKey, AsyncLazy<TValue>> _cache = new ConcurrentDictionary<TKey, AsyncLazy<TValue>>();
        private readonly Func<TKey, TCache, CancellationToken, Task<TValue>> _query;
    }
}
