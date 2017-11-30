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
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="comparer"/> is <c>null</c>.</exception>
        public CacheStore(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _cacheStore = new ConcurrentDictionary<TKey, TValue>(collection, comparer);
        }

        /// <summary>
        /// Gets the element that has the specified key in the cache store.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public TValue this[TKey key] => _cacheStore[key];

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the cache store.
        /// </summary>
        public IEnumerable<TKey> Keys => _cacheStore.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the cache store.
        /// </summary>
        public IEnumerable<TValue> Values => _cacheStore.Values;

        /// <summary>
        /// Gets the number of elements contained in the cache.
        /// </summary>
        public int Count => _cacheStore.Count;

        /// <summary>
        /// Removes all entries from the cache.
        /// </summary>
        public void Clear() => _cacheStore.Clear();

        /// <summary>
        /// Determines whether the cache contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><c>true</c> if the cache contains an element that has the specified key; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public bool ContainsKey(TKey key) => _cacheStore.ContainsKey(key);

        /// <summary>
        /// Returns an enumerator that iterates through the cache.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> for the cache.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _cacheStore.GetEnumerator();

        /// <summary>
        /// Attempts to add the specified key and value to the cache.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be <c>null</c> for reference types.</param>
        /// <returns><c>true</c> if the key/value pair was added to the cache successfully; <c>false</c> if the key already exists.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public bool TryAdd(TKey key, TValue value) => _cacheStore.TryAdd(key, value);

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the cache contains an element that has the specified key; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public bool TryGetValue(TKey key, out TValue value) => _cacheStore.TryGetValue(key, out value);

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value"></param>
        /// <returns><c>true</c> if the object was removed successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public bool TryRemove(TKey key, out TValue value) => _cacheStore.TryRemove(key, out value);

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value.
        /// </summary>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue"/> and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element that has the specified <paramref name="key"/> if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element that has the specified <paramref name="key"/>.</param>
        /// <returns><c>true</c> if the value with <paramref name="key"/> was equal to <paramref name="comparisonValue"/> and was replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) => _cacheStore.TryUpdate(key, newValue, comparisonValue);

        /// <summary>
        /// Returns an enumerator that iterates through the cache.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> for the cache.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _cacheStore.GetEnumerator();

        private readonly ConcurrentDictionary<TKey, TValue> _cacheStore;
    }
}