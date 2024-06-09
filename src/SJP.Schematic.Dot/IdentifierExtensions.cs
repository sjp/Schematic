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

namespace SJP.Schematic.Dot;

internal static partial class IdentifierExtensions
{
    public static string ToVisibleName(this Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

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
        ArgumentNullException.ThrowIfNull(identifier);

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
        result = InvalidCharRegex().Replace(result, string.Empty);
        // convert multiple spaces into one space
        result = SpaceCollapseRegex().Replace(result, " ").Trim();
        // cut and trim
        result = Truncate(result, maxChars).Trim();
        // whitespace to hyphens
        result = WhitespaceRegex().Replace(result, "-").Trim();
        // underscore/period to hyphen
        result = UnderscorePeriodRegex().Replace(result, "-").Trim();

        // ensure chars are safe on disk
        var invalidChars = Path.GetInvalidFileNameChars();
        var validChars = result.Where(c => !invalidChars.Contains(c)).ToArray();

        return new string(validChars);
    }

    [GeneratedRegex(@"[^a-z0-9\s-_.]", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex InvalidCharRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex SpaceCollapseRegex();

    [GeneratedRegex("\\s", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("[_.]", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex UnderscorePeriodRegex();

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

        return input[..Math.Min(input.Length, maxChars)];
    }

    private static string GenerateHashKey(Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

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
        var hash = SHA512.HashData(bytes);

        var builder = StringBuilderCache.Acquire(hash.Length);
        foreach (var hashByte in hash)
            builder.Append(hashByte.ToString("X2", CultureInfo.InvariantCulture));
        return builder.GetStringAndRelease();
    }

    private const int HashKeyLength = 8;
    private const string IdentifierSeparator = "-";
}