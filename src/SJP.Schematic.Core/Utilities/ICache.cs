using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// A read-only generic cache to store a key value pair of any type. While it is read-only, it is also possible to clear the cache.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache.</typeparam>
    public interface ICache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Gets a collection of keys stored by the cache.
        /// </summary>
        IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// Gets a collection of values stored by the cache.
        /// </summary>
        IEnumerable<TValue> Values { get; }

        /// <summary>
        /// Determines whether the cache contains a given key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns><c>true</c> if the cache contains the key, <c>false</c> otherwise.</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Asynchronously determines whether the cache contains a given key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns><c>true</c> if the cache contains the key, <c>false</c> otherwise.</returns>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Retrieves the value from the cache that is associated with a specific key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>A value stored in the cache.</returns>
        TValue GetValue(TKey key);

        /// <summary>
        /// Asynchronously retrieves the value from the cache that is associated with a specific key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>A value stored in the cache.</returns>
        Task<TValue> GetValueAsync(TKey key);

        /// <summary>
        /// Clears the cache of any values that it is currently storing.
        /// </summary>
        void Clear();
    }
}
