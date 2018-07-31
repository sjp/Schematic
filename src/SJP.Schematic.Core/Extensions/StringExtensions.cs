using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!comparisonType.IsValid())
                throw new ArgumentException($"The { nameof(StringComparison) } provided must be a valid enum.", nameof(comparisonType));

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
