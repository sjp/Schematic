using System.Globalization;

namespace SJP.Schematic.Core.Extensions
{
    public static class CharExtensions
    {
        public static UnicodeCategory GetUnicodeCategory(this char c) => char.GetUnicodeCategory(c);

        public static bool IsDigit(this char c) => char.IsDigit(c);

        public static bool IsLetter(this char c) => char.IsLetter(c);

        public static bool IsLetterOrDigit(this char c) => char.IsLetterOrDigit(c);

        public static bool IsPunctuation(this char c) => char.IsPunctuation(c);

        public static bool IsWhiteSpace(this char c) => char.IsWhiteSpace(c);

        public static char ToLowerInvariant(this char c) => char.ToLowerInvariant(c);

        public static char ToUpperInvariant(this char c) => char.ToUpperInvariant(c);
    }
}
