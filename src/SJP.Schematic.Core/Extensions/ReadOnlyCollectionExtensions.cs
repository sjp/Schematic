using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SJP.Schematic.Core.Extensions
{
    public static class ReadOnlyCollectionExtensions
    {
        public static bool Empty<T>(this IReadOnlyCollection<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Count == 0;
        }

        public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this ICollection<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ReadOnlyCollection<T>(source.ToList());
        }

        public static IReadOnlyList<T> AsReadOnlyList<T>(this IList<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source is List<T> list)
                return list.AsReadOnly();

            return source.ToList().AsReadOnly();
        }

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.ToList().AsReadOnly();
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            var dict = source.ToDictionary(keySelector, valueSelector, comparer);
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            var dict = source.ToDictionary(keySelector, valueSelector);
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            var dict = source.ToDictionary(kv => kv.Key, kv => kv.Value, comparer);
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var dict = source.ToDictionary(kv => kv.Key, kv => kv.Value);
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }

        public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ReadOnlyDictionary<TKey, TValue>(source);
        }
    }
}
