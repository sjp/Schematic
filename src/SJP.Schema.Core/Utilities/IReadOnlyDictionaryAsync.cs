using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core.Utilities
{
    /// <summary>
    /// Represents a generic read-only collection of key/value pairs that can be queried asynchronously.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the read-only dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the read-only dictionary.</typeparam>
    public interface IReadOnlyDictionaryAsync<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns><b>true</b> if the object that implements the <see cref="IReadOnlyDictionaryAsync{TKey, TValue}"/> interface contains an element that has the specified key; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <b>null</b>.</exception>
        /// <remarks>
        /// Implementations can vary in how they determine the equality of objects; for example, the class that implements <see cref="IReadOnlyDictionaryAsync{TKey, TValue}"/> might use the <see cref="Comparer{T}.Default"/> property, or it might implement the <see cref="IComparer{T}"/> method.
        ///
        /// Implementations can vary in whether they allow <paramref name="key"/> to be <b>null</b>.
        /// </remarks>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <b>null</b>.</exception>
        /// <remarks>If the key is not found, the return value is assigned the appropriate default value for the type <paramref name="TValue"/>: for example, 0 (zero) for integer types, <b>false</b> for Boolean types, and <b>null</b> for reference types.</remarks>
        Task<TValue> GetValueAsync(TKey key);
    }
}
