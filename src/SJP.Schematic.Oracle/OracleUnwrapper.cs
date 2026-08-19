using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using SJP.Schematic.Oracle.Parsing.Antlr;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Contains routines used to unwrap obfuscated routines in Oracle.
/// </summary>
public static class OracleUnwrapper
{
    /// <summary>
    /// This will attempt to return an unwrapped definition of an obfuscated routine.
    /// </summary>
    /// <param name="input">A potentially wrapped routine definition.</param>
    /// <param name="unwrapped">If successful, the unwrapped routine definition.</param>
    /// <returns><see langword="true" /> if unwrapping was successful, <see langword="false" /> otherwise.</returns>
    public static bool TryUnwrap(string input, [NotNullWhen(true)] out string? unwrapped)
    {
        if (input != null
            && TryGetPayload(input, out var payload)
            && DecodePayload(payload, out var decoded) == DecodeStatus.Success
            && !string.IsNullOrWhiteSpace(decoded))
        {
            unwrapped = decoded;
            return true;
        }

        unwrapped = null;
        return false;
    }

    /// <summary>
    /// This will attempt to return an unwrapped definition of an obfuscated routine, or leave the input unchanged otherwise.
    /// </summary>
    /// <param name="input">A potentially wrapped routine definition.</param>
    /// <returns>An unwrapped routine definition if the input is valid.</returns>
    public static string Unwrap(string input)
    {
        return TryUnwrap(input, out var unwrapped)
            ? unwrapped
            : input;
    }

    /// <summary>
    /// <para>This will unwrap the definition of an obfuscated routine. The input must be wrapped.</para>
    /// <para>Not intended to be used directly as it will throw exceptions on invalid input. <see cref="TryUnwrap(string, out string)"/> or <see cref="Unwrap(string)"/> should be preferred instead.</para>
    /// </summary>
    /// <param name="input">A wrapped routine definition.</param>
    /// <returns>An unwrapped routine definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <see langword="null" />.</exception>
    /// <exception cref="InvalidDataException">Thrown when the data is not able to be unwrapped successfully. This is likely because the data is not wrapped or because it is not valid.</exception>
    public static string UnwrapUnsafe(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!TryGetPayload(input, out var payload))
            throw new InvalidDataException("The given input is not a wrapped definition");

        return DecodePayload(payload, out var decoded) switch
        {
            DecodeStatus.Success => decoded!,
            DecodeStatus.ChecksumFailed => throw new InvalidDataException("The given data is not a valid wrapped definition as it has failed a checksum."),
            _ => throw new InvalidDataException("The given input is not a wrapped definition"),
        };
    }

    // The expected input is:
    //
    // object_type name wrapped
    // a000000
    // HEX#
    // abcd
    // abcd
    // ... (continues for a total of 15 lines)
    // HEX#
    // HEX# HEX# (second one represents the length of the base64 string
    // A base64 string over multiple lines (wrapped at 72 chars)
    /// <summary>
    /// Determines whether the given input is a valid wrapped routine definition.
    /// </summary>
    /// <param name="input">A potentially wrapped routine definition.</param>
    /// <returns><see langword="true" /> if the input appears to be a valid wrapped routine definition.</returns>
    /// <remarks>This does not guarantee that unwrapping is successful, only that the input appears to be correct. For example, the obfuscated input may not pass a checksum.</remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <see langword="null" />.</exception>
    public static bool IsWrappedDefinition(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // The payload isn't cleaned up before being stored (see TryGetPayload), so mirror that here:
        // only the leading whitespace is trimmed, matching what the base64 decoder itself won't skip
        // (e.g. non-ASCII whitespace such as U+00A0). Anything else (embedded '\r', spaces, tabs) is
        // already tolerated by the base64 decoder used by IsValidBase64.
        return TryGetPayload(input, out var payload) && IsValidBase64(payload.TrimStart());
    }

    private static bool TryGetPayload(string input, out ReadOnlySpan<char> payload)
    {
        ArgumentNullException.ThrowIfNull(input);

        payload = default;

        const string magicPrefix = "a000000";

        // The scan below requires input.IndexOf(magicPrefix, currentIndex, Ordinal) >= 0 for some
        // currentIndex >= 0, which implies input.Contains(magicPrefix). Rejecting here therefore
        // cannot change the result, and it keeps the (expensive) ANTLR lexer below off the reject
        // path for the common case of a definition that was never wrapped in the first place.
        if (!input.Contains(magicPrefix, StringComparison.Ordinal))
            return false;

        const string wrappedKeyword = "wrapped";
        var lastIndex = input.LastIndexOf(wrappedKeyword, StringComparison.OrdinalIgnoreCase);
        if (lastIndex < 0)
            return false;

        var textToTokenize = input[..(lastIndex + wrappedKeyword.Length)];
        var tokens = OracleLexing.GetSignificantTokensSafe(textToTokenize);
        if (tokens.Count == 0)
            return false;

        // Note that currently we are not validating the object type.
        // Valid object types are: FUNCTION, PROCEDURE, PACKAGE, PACKAGE BODY, TYPE, TYPE BODY
        // Comments are emitted on the hidden channel and so are already excluded.
        var lastTokenValue = tokens[^1];
        var hasWrappedToken = lastTokenValue.Text.Equals(wrappedKeyword, StringComparison.OrdinalIgnoreCase);
        if (!hasWrappedToken)
            return false;

        var span = input.AsSpan();
        var currentIndex = lastTokenValue.StopIndex + 1;

        var magicPrefixIndex = span[currentIndex..].IndexOf(magicPrefix);
        if (magicPrefixIndex < 0)
            return false;

        currentIndex += magicPrefixIndex + magicPrefix.Length;

        if (!TryConsumeHexLine(span, ref currentIndex))
            return false;

        const string magicFiller = "abcd";
        const int fillerCount = 15;
        for (var i = 0; i < fillerCount; i++)
        {
            var fillerIndex = span[currentIndex..].IndexOf(magicFiller);
            if (fillerIndex < 0)
                return false;

            currentIndex += fillerIndex + magicFiller.Length;
        }

        var remaining = span[currentIndex..];
        var trimmedStart = remaining.IndexOfAnyExcept('\r', '\n');
        if (trimmedStart < 0)
            return false;

        remaining = remaining[trimmedStart..];

        if (!TryReadLine(ref remaining, out var numberLine)
            || !int.TryParse(numberLine, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            return false;

        if (!TryReadLine(ref remaining, out var lengthLine)
            || !TryGetSecondSpaceSeparatedField(lengthLine, out var lengthField)
            || !int.TryParse(lengthField, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            return false;

        // `remaining` is now everything left, equivalent to StringReader.ReadToEnd()
        if (remaining.IsWhiteSpace())
            return false;

        payload = remaining;
        return true;
    }

    // Consumes a single hex-number line (as parsed by StringReader.ReadLine) starting at span[index..],
    // skipping any leading newline characters first. Advances index past the line on success.
    private static bool TryConsumeHexLine(ReadOnlySpan<char> span, ref int index)
    {
        var remaining = span[index..];
        var start = remaining.IndexOfAnyExcept('\r', '\n');
        if (start < 0)
            return false;

        remaining = remaining[start..];
        if (!TryReadLine(ref remaining, out var line)
            || !int.TryParse(line, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            return false;

        index += start + line.Length;
        return true;
    }

    // Mirrors StringReader.ReadLine(): a line is terminated by '\r', '\n', or '\r\n' (and only those),
    // and the terminator is consumed but not included in the returned line. Returns false where
    // StringReader.ReadLine() would return null, i.e. there is nothing left to read.
    private static bool TryReadLine(ref ReadOnlySpan<char> remaining, out ReadOnlySpan<char> line)
    {
        if (remaining.IsEmpty)
        {
            line = default;
            return false;
        }

        var newlineIndex = remaining.IndexOfAny('\r', '\n');
        if (newlineIndex < 0)
        {
            line = remaining;
            remaining = default;
            return true;
        }

        line = remaining[..newlineIndex];

        var next = newlineIndex + 1;
        if (remaining[newlineIndex] == '\r' && next < remaining.Length && remaining[next] == '\n')
            next++;

        remaining = remaining[next..];
        return true;
    }

    // Mirrors line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries) having exactly two elements,
    // returning the second one. Any run of 1 or 3+ space-separated fields is rejected, matching the
    // original Length != 2 check.
    private static bool TryGetSecondSpaceSeparatedField(ReadOnlySpan<char> line, out ReadOnlySpan<char> field)
    {
        field = default;
        var fieldCount = 0;
        var i = 0;

        while (i < line.Length)
        {
            while (i < line.Length && line[i] == ' ')
                i++;
            if (i >= line.Length)
                break;

            var start = i;
            while (i < line.Length && line[i] != ' ')
                i++;

            fieldCount++;
            if (fieldCount > 2)
                return false;
            if (fieldCount == 2)
                field = line[start..i];
        }

        return fieldCount == 2;
    }

    private static bool IsValidBase64(ReadOnlySpan<char> payload)
    {
        if (payload.IsEmpty)
            return false;

        var maxByteCount = Math.Max(payload.Length / 4 * 3, 1);
        var rented = ArrayPool<byte>.Shared.Rent(maxByteCount);
        try
        {
            return Convert.TryFromBase64Chars(payload, rented, out _);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private enum DecodeStatus
    {
        InvalidBase64,
        ChecksumFailed,
        Success
    }

    private static DecodeStatus DecodePayload(ReadOnlySpan<char> payload, out string? decoded)
    {
        decoded = null;

        const int hashSize = 20; // bytes
        const int zlibHeaderSize = 2;
        const int zlibTrailerSize = 4;
        // ZlibToDeflate (see below) needs at least this many bytes to produce a non-negative slice.
        const int minDataSize = zlibHeaderSize + zlibTrailerSize + 1;

        var maxByteCount = Math.Max(payload.Length / 4 * 3, 1);
        var rented = ArrayPool<byte>.Shared.Rent(maxByteCount);
        try
        {
            if (!Convert.TryFromBase64Chars(payload, rented, out var bytesWritten))
                return DecodeStatus.InvalidBase64;

            if (bytesWritten < hashSize + minDataSize)
                return DecodeStatus.ChecksumFailed;

            var mapped = rented.AsSpan(0, bytesWritten);
            for (var i = 0; i < mapped.Length; i++)
            {
                mapped[i] = CharMap[mapped[i]];
            }

            var hashBuffer = mapped[..hashSize];
            var dataBuffer = mapped[hashSize..];

            Span<byte> computedHashBuffer = stackalloc byte[hashSize];
            var areEqual = SHA1.TryHashData(dataBuffer, computedHashBuffer, out _)
                && computedHashBuffer.SequenceEqual(hashBuffer);

            if (!areEqual)
                return DecodeStatus.ChecksumFailed;

            // need to skip zlib header bytes and trim trailing zlib checksum bytes to enable decompression.
            // Deliberately drops zlibHeaderSize + zlibTrailerSize + 1 = 7 bytes in total (not 6) -
            // preserved byte-for-byte from the original implementation.
            var deflateOffset = hashSize + zlibHeaderSize;
            var deflateLength = dataBuffer.Length - zlibHeaderSize - zlibTrailerSize + 1;

            using var reader = new MemoryStream(rented, deflateOffset, deflateLength, writable: false);
            using var unzipper = new DeflateStream(reader, CompressionMode.Decompress);
            using var writer = new MemoryStream(deflateLength);
            unzipper.CopyTo(writer);

            var decompressed = writer.GetBuffer().AsSpan(0, (int)writer.Length);
            var trimmed = decompressed.TrimEnd((byte)0); // remove trailing NUL bytes
            decoded = Encoding.UTF8.GetString(trimmed);
            return DecodeStatus.Success;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static ReadOnlySpan<byte> CharMap =>
    [
        0x3D, 0x65, 0x85, 0xB3, 0x18, 0xDB, 0xE2, 0x87, 0xF1, 0x52, 0xAB, 0x63, 0x4B, 0xB5, 0xA0, 0x5F,
        0x7D, 0x68, 0x7B, 0x9B, 0x24, 0xC2, 0x28, 0x67, 0x8A, 0xDE, 0xA4, 0x26, 0x1E, 0x03, 0xEB, 0x17,
        0x6F, 0x34, 0x3E, 0x7A, 0x3F, 0xD2, 0xA9, 0x6A, 0x0F, 0xE9, 0x35, 0x56, 0x1F, 0xB1, 0x4D, 0x10,
        0x78, 0xD9, 0x75, 0xF6, 0xBC, 0x41, 0x04, 0x81, 0x61, 0x06, 0xF9, 0xAD, 0xD6, 0xD5, 0x29, 0x7E,
        0x86, 0x9E, 0x79, 0xE5, 0x05, 0xBA, 0x84, 0xCC, 0x6E, 0x27, 0x8E, 0xB0, 0x5D, 0xA8, 0xF3, 0x9F,
        0xD0, 0xA2, 0x71, 0xB8, 0x58, 0xDD, 0x2C, 0x38, 0x99, 0x4C, 0x48, 0x07, 0x55, 0xE4, 0x53, 0x8C,
        0x46, 0xB6, 0x2D, 0xA5, 0xAF, 0x32, 0x22, 0x40, 0xDC, 0x50, 0xC3, 0xA1, 0x25, 0x8B, 0x9C, 0x16,
        0x60, 0x5C, 0xCF, 0xFD, 0x0C, 0x98, 0x1C, 0xD4, 0x37, 0x6D, 0x3C, 0x3A, 0x30, 0xE8, 0x6C, 0x31,
        0x47, 0xF5, 0x33, 0xDA, 0x43, 0xC8, 0xE3, 0x5E, 0x19, 0x94, 0xEC, 0xE6, 0xA3, 0x95, 0x14, 0xE0,
        0x9D, 0x64, 0xFA, 0x59, 0x15, 0xC5, 0x2F, 0xCA, 0xBB, 0x0B, 0xDF, 0xF2, 0x97, 0xBF, 0x0A, 0x76,
        0xB4, 0x49, 0x44, 0x5A, 0x1D, 0xF0, 0x00, 0x96, 0x21, 0x80, 0x7F, 0x1A, 0x82, 0x39, 0x4F, 0xC1,
        0xA7, 0xD7, 0x0D, 0xD1, 0xD8, 0xFF, 0x13, 0x93, 0x70, 0xEE, 0x5B, 0xEF, 0xBE, 0x09, 0xB9, 0x77,
        0x72, 0xE7, 0xB2, 0x54, 0xB7, 0x2A, 0xC7, 0x73, 0x90, 0x66, 0x20, 0x0E, 0x51, 0xED, 0xF8, 0x7C,
        0x8F, 0x2E, 0xF4, 0x12, 0xC6, 0x2B, 0x83, 0xCD, 0xAC, 0xCB, 0x3B, 0xC4, 0x4E, 0xC0, 0x69, 0x36,
        0x62, 0x02, 0xAE, 0x88, 0xFC, 0xAA, 0x42, 0x08, 0xA6, 0x45, 0x57, 0xD3, 0x9A, 0xBD, 0xE1, 0x23,
        0x8D, 0x92, 0x4A, 0x11, 0x89, 0x74, 0x6B, 0x91, 0xFB, 0xFE, 0xC9, 0x01, 0xEA, 0x1B, 0xF7, 0xCE,
    ];
}
