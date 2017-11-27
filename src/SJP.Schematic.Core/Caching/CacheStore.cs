using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// A generic in-memory cache store.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache store.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache store.</typeparam>
    public class CacheStore<TKey, TValue> : ICacheStore<TKey, TValue>
    {
        /// <summary>
        /// Creates a new cache store, with default key equality comparison.
        /// </summary>
        public CacheStore()
        {
            _cacheStore = new ConcurrentDictionary<TKey, TValue>();
        }

        /// <summary>
        /// Creates a new cache store, with a custom key equality comparer.
        /// </summary>
        /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
        public CacheStore(IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _cacheStore = new ConcurrentDictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        /// Creates a new cache store, with default key equality comparison, initially populated by provided data.
        /// </summary>
        /// <param name="collection">A collection of data to initially populate the cache with.</param>
        public CacheStore(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _cacheStore = new ConcurrentDictionary<TKey, TValue>(collection);
        }

        /// <summary>
        /// Creates a new cache store, with a custom key equality comparer, initially populated by provided data.
        /// </summary>
        /// <param name="collection">A collection of data to initially populate the cache with.</param>
        /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
        public CacheStore(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _cacheStore = new ConcurrentDictionary<TKey, TValue>(collection, comparer);
        }

        public TValue this[TKey key] => _cacheStore[key];

        public IEnumerable<TKey> Keys => _cacheStore.Keys;

        public IEnumerable<TValue> Values => _cacheStore.Values;

        public int Count => _cacheStore.Count;

        public void Clear() => _cacheStore.Clear();

        public bool ContainsKey(TKey key) => _cacheStore.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _cacheStore.GetEnumerator();

        public bool TryAdd(TKey key, TValue value) => _cacheStore.TryAdd(key, value);

        public bool TryGetValue(TKey key, out TValue value) => _cacheStore.TryGetValue(key, out value);

        public bool TryRemove(TKey key, out TValue value) => _cacheStore.TryRemove(key, out value);

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) => _cacheStore.TryUpdate(key, newValue, comparisonValue);

        IEnumerator IEnumerable.GetEnumerator() => _cacheStore.GetEnumerator();

        private readonly ConcurrentDictionary<TKey, TValue> _cacheStore;
    }
}