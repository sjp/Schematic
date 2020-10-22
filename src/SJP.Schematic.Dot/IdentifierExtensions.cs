using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dot
{
    internal static class IdentifierExtensions
    {
        public static string ToVisibleName(this Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var builder = StringBuilderCache.Acquire();

            if (identifier.Schema != null)
            {
                builder.Append(identifier.Schema);
                builder.Append('.');
            }

            builder.Append(identifier.LocalName);

            return builder.GetStringAndRelease();
        }

        public static string ToSafeKey(this Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var safeName = ToSlug(identifier.LocalName);
            var hashKey = GenerateHashKey(identifier);
            var truncatedHash = Truncate(hashKey, HashKeyLength);

            return safeName + IdentifierSeparator + truncatedHash.ToLowerInvariant();
        }

        // https://adamhathcock.blog/2017/05/04/generating-url-slugs-in-net-core/
        // with some modifications
        private static string ToSlug(string input, int maxChars = 45)
        {
            if (input.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(input));
            if (maxChars <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChars), "The limit to the number of characters in a slug must be at least 1.");

            var result = RemoveDiacritics(input).ToLowerInvariant();
            // invalid chars
            result = _invalidCharRegex.Replace(result, string.Empty);
            // convert multiple spaces into one space
            result = _spaceCollapseRegex.Replace(result, " ").Trim();
            // cut and trim
            result = Truncate(result, maxChars).Trim();
            // whitespace to hyphens
            result = _whitespaceRegex.Replace(result, "-").Trim();
            // underscore/period to hyphen
            result = _underscorePeriodRegex.Replace(result, "-").Trim();

            // ensure chars are safe on disk
            var invalidChars = Path.GetInvalidFileNameChars();
            var validChars = result.Where(c => !invalidChars.Contains(c)).ToArray();

            return new string(validChars);
        }

        private static readonly Regex _invalidCharRegex = new Regex(@"[^a-z0-9\s-_.]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        private static readonly Regex _spaceCollapseRegex = new Regex(@"\s+", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        private static readonly Regex _whitespaceRegex = new Regex("\\s", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        private static readonly Regex _underscorePeriodRegex = new Regex("[_.]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        private static string RemoveDiacritics(string input)
        {
            var temp = new string(input.Normalize(NormalizationForm.FormD)
                .Where(c => c.GetUnicodeCategory() != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return temp.Normalize(NormalizationForm.FormC);
        }

        private static string Truncate(string input, int maxChars)
        {
            if (input.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(input));
            if (maxChars < 0)
                throw new ArgumentOutOfRangeException(nameof(maxChars), "The number of characters to truncate to must be at least 1.");

            return input.Substring(0, Math.Min(input.Length, maxChars));
        }

        private static string GenerateHashKey(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var builder = StringBuilderCache.Acquire();

            if (identifier.Server != null)
            {
                var serverHash = GenerateHashKey(identifier.Server);
                builder.Append(serverHash);
            }

            if (identifier.Database != null)
            {
                var databaseHash = GenerateHashKey(identifier.Database);
                builder.Append(databaseHash);
            }

            if (identifier.Schema != null)
            {
                var schemaHash = GenerateHashKey(identifier.Schema);
                builder.Append(schemaHash);
            }

            var localNameHash = GenerateHashKey(identifier.LocalName);
            builder.Append(localNameHash);

            var combinedHashSource = builder.GetStringAndRelease();
            return GenerateHashKey(combinedHashSource);
        }

        private static string GenerateHashKey(string input)
        {
            if (input.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(input));

            var bytes = Encoding.Unicode.GetBytes(input);

            using var hasher = new SHA512Managed();
            var hash = hasher.ComputeHash(bytes);

            var builder = StringBuilderCache.Acquire(hash.Length);
            foreach (var hashByte in hash)
                builder.Append(hashByte.ToString("X2", CultureInfo.InvariantCulture));
            return builder.GetStringAndRelease();
        }

        private const int HashKeyLength = 8;
        private const string IdentifierSeparator = "-";
    }
}
