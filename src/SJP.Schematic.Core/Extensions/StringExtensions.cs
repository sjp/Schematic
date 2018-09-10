using System;
using System.Collections.Generic;
using EnumsNET;

namespace SJP.Schematic.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);

        public static bool IsNullOrWhiteSpace(this string input) => string.IsNullOrWhiteSpace(input);

        public static string Join(this IEnumerable<string> values, string separator)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));

            return string.Join(separator, values);
        }

        public static bool Contains(this string input, string value, StringComparison comparisonType)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!comparisonType.IsValid())
                throw new ArgumentException($"The { nameof(StringComparison) } provided must be a valid enum.", nameof(comparisonType));

            return input.IndexOf(value, comparisonType) >= 0;
        }
    }
}
