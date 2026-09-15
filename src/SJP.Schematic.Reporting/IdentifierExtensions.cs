using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Reporting;

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

        // The same names are keyed over and over while a report is built (file names, routes,
        // graph nodes and edges), and every computation hashes several times, so keys are cached.
        if (SafeKeyCache.TryGetValue(identifier, out var cachedKey))
            return cachedKey;

        var safeKey = ComputeSafeKey(identifier);

        // Bounded so that a long-lived process does not keep a key for every name it has ever seen.
        // Starting over when full keeps the names currently being rendered hot; concurrent callers
        // can briefly take the count slightly past the limit, which is harmless.
        if (Volatile.Read(ref _safeKeyCacheCount) >= SafeKeyCacheCapacity)
        {
            SafeKeyCache.Clear();
            Volatile.Write(ref _safeKeyCacheCount, 0);
        }

        if (SafeKeyCache.TryAdd(identifier, safeKey))
            Interlocked.Increment(ref _safeKeyCacheCount);

        return safeKey;
    }

    private static string ComputeSafeKey(Identifier identifier)
    {
        var safeName = ToSlug(identifier.LocalName);
        var hashKey = GenerateHashKey(identifier);

        return safeName + IdentifierSeparator + hashKey;
    }

    private const int SafeKeyCacheCapacity = 16384;

    private static readonly ConcurrentDictionary<Identifier, string> SafeKeyCache = new();
    private static int _safeKeyCacheCount;

    // https://adamhathcock.blog/2017/05/04/generating-url-slugs-in-net-core/
    // with some modifications
    private static string ToSlug(string input, int maxChars = 45)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
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
        var slug = UnderscorePeriodRegex().Replace(result, "-").Trim();

        // The slug now holds only 'a'-'z', '0'-'9' and '-', none of which is invalid in a file name
        // on any platform, so it needs no further filtering to be safe on disk.

        // An all-symbol / non-ASCII name (e.g. "日本語", "+++") reduces to an empty slug. Fall back to a
        // placeholder so ToSafeKey never emits a key that starts with the '-' separator and has no
        // readable part; the appended hash still disambiguates such names from one another.
        return slug.Length == 0 ? EmptySlugPlaceholder : slug;
    }

    private const string EmptySlugPlaceholder = "unnamed";

    // Strips everything except letters, digits, whitespace, and '_' '.' '-'. The '_' and '.' are
    // kept intentionally — later passes collapse them (and whitespace) into '-'. The '-' is placed
    // last so it is unambiguously a literal rather than a range endpoint.
    [GeneratedRegex(@"[^a-z0-9\s_.-]", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex InvalidCharRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex SpaceCollapseRegex();

    [GeneratedRegex("\\s", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("[_.]", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex UnderscorePeriodRegex();

    private static string RemoveDiacritics(string input)
    {
        // ASCII text has no combining marks and is unchanged by normalization.
        if (Ascii.IsValid(input))
            return input;

        var temp = new string(input.Normalize(NormalizationForm.FormD)
            .Where(static c => c.GetUnicodeCategory() != UnicodeCategory.NonSpacingMark)
            .ToArray());

        return temp.Normalize(NormalizationForm.FormC);
    }

    private static string Truncate(string input, int maxChars)
    {
        // Deliberately permits empty (but not null) input: ToSlug calls this on a name that may
        // have already been reduced to nothing by invalid-character stripping (e.g. "+++",
        // "日本語"), before its own empty-slug fallback runs further down.
        ArgumentNullException.ThrowIfNull(input);
        if (maxChars <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxChars), "The number of characters to truncate to must be at least 1.");

        return input[..Math.Min(input.Length, maxChars)];
    }

    // The hash is the SHA-512 of the UTF-16 text formed by concatenating the upper-case hexadecimal
    // SHA-512 of each part of the name (server, database, schema, local name) that is present, where
    // each part is hashed from its UTF-16 encoding. The key keeps the leading hexadecimal digits of
    // that hash, in lower case. Changing any step changes every key, and keys are persisted as file
    // names and links in generated reports.
    private static string GenerateHashKey(Identifier identifier)
    {
        Span<char> partHashes = stackalloc char[MaxNameParts * Sha512HexLength];
        var length = 0;

        if (identifier.Server != null)
            length += WriteHexHash(identifier.Server, partHashes[length..]);
        if (identifier.Database != null)
            length += WriteHexHash(identifier.Database, partHashes[length..]);
        if (identifier.Schema != null)
            length += WriteHexHash(identifier.Schema, partHashes[length..]);
        length += WriteHexHash(identifier.LocalName, partHashes[length..]);

        Span<byte> hash = stackalloc byte[SHA512.HashSizeInBytes];
        HashUtf16(partHashes[..length], hash);

        return Convert.ToHexStringLower(hash[..(HashKeyLength / 2)]);
    }

    private static int WriteHexHash(string input, Span<char> destination)
    {
        Span<byte> hash = stackalloc byte[SHA512.HashSizeInBytes];
        HashUtf16(input, hash);

        Convert.TryToHexString(hash, destination, out var charsWritten);
        return charsWritten;
    }

    private static void HashUtf16(ReadOnlySpan<char> input, Span<byte> destination)
    {
        const int maxStackBytes = 2048;

        // Encoding.Unicode is used rather than reinterpreting the chars as bytes so that the result
        // stays little-endian and unpaired surrogates are still replaced, exactly as it encodes them.
        var byteCount = Encoding.Unicode.GetMaxByteCount(input.Length);
        byte[]? rented = null;
        var buffer = byteCount <= maxStackBytes
            ? stackalloc byte[byteCount]
            : (rented = ArrayPool<byte>.Shared.Rent(byteCount));

        try
        {
            var bytesWritten = Encoding.Unicode.GetBytes(input, buffer);
            SHA512.HashData(buffer[..bytesWritten], destination);
        }
        finally
        {
            if (rented != null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private const int MaxNameParts = 4;
    private const int Sha512HexLength = SHA512.HashSizeInBytes * 2;
    private const int HashKeyLength = 8;
    private const string IdentifierSeparator = "-";
}