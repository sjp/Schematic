using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnumsNET;

namespace SJP.Schematic.Core.Extensions
{
    /// <summary>
    /// Convenience extension methods for working with <see cref="string"/> objects.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Indicates whether the specified string is <c>null</c> or an empty string (<c>""</c>).
        /// </summary>
        /// <param name="input">The string to test.</param>
        /// <returns><c>true</c> if <paramref name="input"/> is <c>null</c> or an empty string (<c>""</c>); otherwise, <c>false</c>.</returns>
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? input) => string.IsNullOrEmpty(input);

        /// <summary>
        /// Indicates whether the specified string is <c>null</c> or empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="input">The string to test.</param>
        /// <returns><c>true</c> if <paramref name="input"/> is <c>null</c> or an empty string (<c>""</c>); or if <paramref name="input"/> consists exclusively of white-space characters otherwise, <c>false</c>.</returns>
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? input) => string.IsNullOrWhiteSpace(input);

        /// <summary>
        /// Concatenates the members of a constructed <see cref="IEnumerable{String}"/>, using the specified separator between each member.
        /// </summary>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the returned string only if values has more than one element.</param>
        /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string. Alternatively <c>""</c> if <paramref name="values"/> has zero elements.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> or <paramref name="separator"/> is <c>null</c></exception>
        public static string Join(this IEnumerable<string> values, string separator)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));

            return string.Join(separator, values);
        }

        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string while also enabling configuration of substring comparison.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">The type of substring comparison that should occur.</param>
        /// <returns><c>true</c> if <paramref name="input"/> contains <paramref name="value"/> using the configured <paramref name="comparisonType"/> or if <paramref name="value"/> is empty; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="comparisonType"/> is not a valid value.</exception>
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
