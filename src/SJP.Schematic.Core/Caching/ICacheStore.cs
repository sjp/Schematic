using System.Collections.Generic;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Represents a generic store that provides storage of cached values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache store.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache store.</typeparam>
    public interface ICacheStore<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Removes all keys and values from the cache store.
        /// </summary>
        void Clear();

        /// <summary>
        /// Attempts to add the specified key and value to the cache store.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be <c>null</c> for reference types.</param>
        /// <returns><c>true</c> if the key/value pair was added to the cache store successfully; <c>false</c> if the key already exists.</returns>
        bool TryAdd(TKey key, TValue value);

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the cache store.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, contains the object removed from the cache store, or the default value of the <see cref="TValue"/> type if <paramref name="key"/> does not exist.</param>
        /// <returns><c>true</c> if the object was removed successfully; otherwise, <c>false</c>.</returns>
        bool TryRemove(TKey key, out TValue value);

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value.
        /// </summary>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue"/> and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element that has the specified key if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element that has the specified key.</param>
        /// <returns><c>true</c> if the value with key was equal to comparisonValue and was replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.</returns>
        bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue);
    }
}