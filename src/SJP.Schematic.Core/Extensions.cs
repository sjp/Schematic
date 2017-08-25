using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SJP.Schematic.Core
{
    public static class CachingExtensions
    {
        public static IRelationalDatabase AsCachedDatabase(this IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var cachedDb = database as CachedRelationalDatabase;
            return cachedDb ?? new CachedRelationalDatabase(database);
        }
    }

    public static class CharExtensions
    {
        public static UnicodeCategory GetUnicodeCategory(this char c) => CharUnicodeInfo.GetUnicodeCategory(c);

        public static bool IsDigit(this char c) => char.IsDigit(c);

        public static bool IsLetter(this char c) => char.IsLetter(c);

        public static bool IsLetterOrDigit(this char c) => char.IsLetterOrDigit(c);

        public static char ToLowerInvariant(this char c) => char.ToLowerInvariant(c);

        public static char ToUpperInvariant(this char c) => char.ToUpperInvariant(c);
    }

    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public static class EnumerableExtensions
    {
        public static bool Empty<T>(this IEnumerable<T> source) => !source.Any();

        public static bool AnyNull<T>(this IEnumerable<T> source) where T : class => source.Any(x => x == null);

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => source.DistinctBy(keySelector, null);

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
           Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return _(); IEnumerable<TSource> _()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                        yield return element;
                }
            }
        }
    }

    public static class ReadOnlyCollectionExtensions
    {
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

    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);

        public static bool IsNullOrWhiteSpace(this string input) => string.IsNullOrWhiteSpace(input);

        public static string Join(this IEnumerable<string> values, string separator) => string.Join(separator, values);

        public static bool Contains(this string input, string value, StringComparison comparisonType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!Enum.IsDefined(typeof(StringComparison), comparisonType))
                throw new ArgumentException($"The { nameof(comparisonType) } argument given is not a member of { typeof(StringComparison).FullName }", nameof(comparisonType));

            return input.IndexOf(value, comparisonType) >= 0;
        }

        public static string TrimStart(this string input, string trimText)
        {
            if (input.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(input));
            if (trimText.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(trimText));

            return input.StartsWith(trimText)
                ? input.Substring(trimText.Length)
                : input;
        }

        public static string TrimStart(this string input, string trimText, StringComparison comparison)
        {
            if (input.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(input));
            if (trimText.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(trimText));

            return input.StartsWith(trimText, comparison)
                ? input.Substring(trimText.Length)
                : input;
        }

        public static string TrimStart(this string input, string trimText, bool ignoreCase, CultureInfo culture)
        {
            if (input.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(input));
            if (trimText.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(trimText));

            return input.StartsWith(trimText, ignoreCase, culture)
                ? input.Substring(trimText.Length)
                : input;
        }

        public static string TrimEnd(this string input, string trimText)
        {
            if (input.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(input));
            if (trimText.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(trimText));

            return input.EndsWith(trimText)
                ? input.Substring(0, input.Length - trimText.Length)
                : input;
        }

        public static string TrimEnd(this string input, string trimText, StringComparison comparison)
        {
            if (input.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(input));
            if (trimText.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(trimText));

            return input.EndsWith(trimText, comparison)
                ? input.Substring(0, input.Length - trimText.Length)
                : input;
        }

        public static string TrimEnd(this string input, string trimText, bool ignoreCase, CultureInfo culture)
        {
            if (input.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(input));
            if (trimText.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(trimText));

            return input.EndsWith(trimText, ignoreCase, culture)
                ? input.Substring(0, input.Length - trimText.Length)
                : input;
        }
    }
}
