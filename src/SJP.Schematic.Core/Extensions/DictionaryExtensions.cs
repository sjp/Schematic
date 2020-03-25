using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Extensions
{
    /// <summary>
    /// Convenience extension methods for working with dictionary-like objects.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Creates a dictionary from its constituent key-value pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source collection of key-value objects.</param>
        /// <returns>A mutable dictionary that represents a lookup of the key-value pair objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
