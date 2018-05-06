using System;
using System.Text;
using System.Security.Cryptography;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

namespace SJP.Schematic.SchemaSpy
{
    internal static class IdentifierExtensions
    {
        public static string ToSafeKey(this Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var safeName = ToSlug(identifier.LocalName);
            var hashKey = GenerateHashKey(identifier);
            var truncatedHash = Truncate(hashKey, HashKeyLength);

            return safeName + IdentifierSeparator + truncatedHash;
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
            result = Regex.Replace(result, @"[^a-z0-9\s-]", string.Empty);
            // convert multiple spaces into one space
            result = Regex.Replace(result, @"\s+", " ").Trim();
            // cut and trim
            result = Truncate(result, maxChars).Trim();
            // hyphens
            result = Regex.Replace(result, "\\s", "-").Trim();

            // ensure chars are safe on disk
            var invalidChars = Path.GetInvalidFileNameChars();
            var validChars = result.Where(c => !invalidChars.Contains(c)).ToArray();

            return new string(validChars);
        }

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

            var builder = new StringBuilder();

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

            var combinedHashSource = builder.ToString();
            return GenerateHashKey(combinedHashSource);
        }

        private static string GenerateHashKey(string input)
        {
            if (input.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(input));

            var bytes = Encoding.Unicode.GetBytes(input);
            using (var hasher = new SHA512Managed())
            {
                var hash = hasher.ComputeHash(bytes);
                var builder = new StringBuilder(hash.Length);
                foreach (var hashByte in hash)
                    builder.Append(BitConverter.ToString(hash));
                return builder.ToString();
            }
        }

        private const int HashKeyLength = 8;
        private const string IdentifierSeparator = "-";
    }
}
