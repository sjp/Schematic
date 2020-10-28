using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SJP.Schematic.Modelled.Reflection
{
    internal static class XmlCommentsTextHelper
    {
        public static string Humanize(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return text
                .NormalizeIndentation()
                .HumanizeRefTags()
                .HumanizeCodeTags();
        }

        private static string NormalizeIndentation(this string text)
        {
            var lines = text.Split('\n');
            var padding = GetCommonLeadingWhitespace(lines);
            var padLen = padding.Length;

            // remove leading padding from each line
            for (int i = 0, l = lines.Length; i < l; ++i)
            {
                var line = lines[i].TrimEnd('\r'); // remove trailing '\r'

                if (padLen != 0 && line.Length >= padLen && string.Equals(line.Substring(0, padLen), padding, StringComparison.Ordinal))
                    line = line.Substring(padLen);

                lines[i] = line;
            }

            // remove leading empty lines, but not all leading padding
            // remove all trailing whitespace, regardless
            return string.Join(Environment.NewLine, lines.SkipWhile(x => string.IsNullOrWhiteSpace(x))).TrimEnd();
        }

        private static string GetCommonLeadingWhitespace(string[] lines)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            if (lines.Length == 0)
                return string.Empty;

            var nonEmptyLines = lines
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (nonEmptyLines.Length < 1)
                return string.Empty;

            var padLen = 0;

            // use the first line as a seed, and see what is shared over all nonEmptyLines
            var seed = nonEmptyLines[0];
            for (int i = 0, l = seed.Length; i < l; ++i)
            {
                if (!char.IsWhiteSpace(seed, i))
                    break;

                if (nonEmptyLines.Any(line => line[i] != seed[i]))
                    break;

                ++padLen;
            }

            if (padLen > 0)
                return seed.Substring(0, padLen);

            return string.Empty;
        }

        private static string HumanizeRefTags(this string text)
        {
            return RefTagPattern.Replace(text, (match) => match.Groups["display"].Value);
        }

        private static string HumanizeCodeTags(this string text)
        {
            return CodeTagPattern.Replace(text, (match) => "{" + match.Groups["display"].Value + "}");
        }

        private static readonly Regex RefTagPattern = new Regex(@"<(see|paramref) (name|cref)=""([TPF]{1}:)?(?<display>.+?)"" ?/>", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
        private static readonly Regex CodeTagPattern = new Regex("<c>(?<display>.+?)</c>", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
    }
}
