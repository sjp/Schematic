using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
