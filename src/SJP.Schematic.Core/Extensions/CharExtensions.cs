using System.Globalization;

namespace SJP.Schematic.Core.Extensions
{
    /// <summary>
    /// Convenience extension methods for working with <see cref="char"/> objects.
    /// </summary>
    public static class CharExtensions
    {
        /// <summary>
        /// Categorizes a specified Unicode character into a group identified by one of the <see cref="UnicodeCategory"/> values.</summary>
        /// <param name="c">The Unicode character to categorize.</param>
        /// <returns>A <see cref="UnicodeCategory"/> value that identifies the group that contains <c>c</c>.</returns>
        public static UnicodeCategory GetUnicodeCategory(this char c) => char.GetUnicodeCategory(c);

        /// <summary>
        /// Indicates whether the specified Unicode character is categorized as a decimal digit.
        /// </summary>
        /// <param name="c">The Unicode character to evaluate.</param>
        /// <returns><c>true</c> if the specified <paramref name="c"/> is a decimal digit; otherwise, <c>false</c>.</returns>
        public static bool IsDigit(this char c) => char.IsDigit(c);

        /// <summary>
        /// Indicates whether a Unicode character is categorized as a Unicode letter.
        /// </summary>
        /// <param name="c">The Unicode character to evaluate.</param>
        /// <returns><c>true</c> if the specified <paramref name="c"/> is a letter; otherwise, <c>false</c>.</returns>
        public static bool IsLetter(this char c) => char.IsLetter(c);

        /// <summary>
        /// Indicates whether a Unicode character is categorized as a letter or a decimal digit.
        /// </summary>
        /// <param name="c">The Unicode character to evaluate.</param>
        /// <returns><c>true</c> if <paramref name="c"/> is a letter or a decimal digit; otherwise, <c>false</c>.</returns>
        public static bool IsLetterOrDigit(this char c) => char.IsLetterOrDigit(c);

        /// <summary>
        /// Indicates whether the specified Unicode character is categorized as a punctuation mark.
        /// </summary>
        /// <param name="c">The Unicode character to evaluate.</param>
        /// <returns><c>true</c> if <paramref name="c"/> is a punctuation mark; otherwise, <c>false</c>.</returns>
        public static bool IsPunctuation(this char c) => char.IsPunctuation(c);

        /// <summary>
        /// Indicates whether the specified Unicode character is categorized as white space.
        /// </summary>
        /// <param name="c">The Unicode character to evaluate.</param>
        /// <returns><c>true</c> if <paramref name="c"/> is white space; otherwise, <c>false</c>.</returns>
        public static bool IsWhiteSpace(this char c) => char.IsWhiteSpace(c);

        /// <summary>
        /// Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="c">The Unicode character to convert.</param>
        /// <returns>The lowercase equivalent of <paramref name="c"/>, or the unchanged value of <paramref name="c"/>, if <paramref name="c"/> is already lowercase or not alphabetic.</returns>
        public static char ToLowerInvariant(this char c) => char.ToLowerInvariant(c);

        /// <summary>
        /// Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="c">The Unicode character to convert.</param>
        /// <returns>The uppercase equivalent of <paramref name="c"/>, or the unchanged value of <paramref name="c"/>, if <paramref name="c"/> is already uppercase or not alphabetic.</returns>
        public static char ToUpperInvariant(this char c) => char.ToUpperInvariant(c);
    }
}
