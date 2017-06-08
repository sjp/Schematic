using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SJP.Schema.Core
{
    public static class AutoSchemaCharExtensions
    {
        public static UnicodeCategory GetUnicodeCategory(this char c) => CharUnicodeInfo.GetUnicodeCategory(c);

        public static bool IsDigit(this char c) => char.IsDigit(c);

        public static bool IsLetter(this char c) => char.IsLetter(c);

        public static bool IsLetterOrDigit(this char c) => char.IsLetterOrDigit(c);

        public static char ToLowerInvariant(this char c) => char.ToLowerInvariant(c);

        public static char ToUpperInvariant(this char c) => char.ToUpperInvariant(c);
    }

    public static class AutoSchemaStringExtensions
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
    }

    public static class AutoSchemaEnumerableExtensions
    {
        public static bool Empty<T>(this IEnumerable<T> source) => !source.Any();

        public static bool AnyNull<T>(this IEnumerable<T> source) where T : class => source.Any(x => x == null);
    }
}
