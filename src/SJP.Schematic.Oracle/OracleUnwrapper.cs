using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Parsing;

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
    /// <returns><c>true</c> if unwrapping was successful, <c>false</c> otherwise.</returns>
    public static bool TryUnwrap(string input, [NotNullWhen(true)] out string? unwrapped)
    {
        if (input == null || !TryGetPayload(input, out var payload))
        {
            unwrapped = null;
            return false;
        }

        if (TryDecodeBase64Package(payload, out var decodedPackage)
            && !string.IsNullOrWhiteSpace(decodedPackage))
        {
            unwrapped = decodedPackage;
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidDataException">Thrown when the data is not able to be unwrapped successfully. This is likely because the data is not wrapped or because it is not valid.</exception>
    public static string UnwrapUnsafe(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (!TryGetPayload(input, out var payload))
            throw new InvalidDataException("The given input is not a wrapped definition");

        return DecodeBase64PackageUnsafe(payload);
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
    /// <returns><c>true</c> if the input appears to be a valid wrapped routine definition.</returns>
    /// <remarks>This does not guarantee that unwrapping is successful, only that the input appears to be correct. For example, the obfuscated input may not pass a checksum.</remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <c>null</c>.</exception>
    public static bool IsWrappedDefinition(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return TryGetPayload(input, out _);
    }

    private static bool TryGetPayload(string input, [NotNullWhen(true)] out string? payload)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        payload = null;
        const string wrappedKeyword = "wrapped";
        var lastIndex = input.LastIndexOf(wrappedKeyword, StringComparison.OrdinalIgnoreCase);
        if (lastIndex < 0)
            return false;

        var textToTokenize = input[..(lastIndex + wrappedKeyword.Length)];
        var tokens = Tokenizer.TryTokenize(textToTokenize);

        // Note that currently we are not validating the object type.
        // Valid object types are: FUNCTION, PROCEDURE, PACKAGE, PACKAGE BODY, TYPE, TYPE BODY
        // Additionally, it can contain comments which need to be removed
        var hasWrappedToken = tokens.HasValue
            && tokens.Value.Last().Kind == OracleToken.Identifier
            && tokens.Value.Last().Span.ToStringValue().Equals(wrappedKeyword, StringComparison.OrdinalIgnoreCase);

        if (!hasWrappedToken)
            return false;

        var currentIndex = tokens.Value.Last().Span.Position.Absolute + tokens.Value.Last().Span.Length;
        const string magicPrefix = "a000000";

        var magicPrefixIndex = input.IndexOf(magicPrefix, currentIndex, StringComparison.Ordinal);
        if (magicPrefixIndex < 0)
            return false;

        currentIndex = magicPrefixIndex + magicPrefix.Length;
        using (var reader = new StringReader(input[currentIndex..].TrimStart(NewlineChars)))
        {
            var numberLine = reader.ReadLine();
            if (numberLine == null)
                return false;

            if (!int.TryParse(numberLine, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                return false;

            currentIndex = input.IndexOf(numberLine, currentIndex, StringComparison.Ordinal) + numberLine.Length;
        }

        const string magicFiller = "abcd";
        const int fillerCount = 15;
        for (var i = 0; i < fillerCount; i++)
        {
            var fillerIndex = input.IndexOf(magicFiller, currentIndex, StringComparison.Ordinal);
            if (fillerIndex < 0)
                return false;

            currentIndex = fillerIndex + magicFiller.Length;
        }

        var remainder = input[currentIndex..].TrimStart(NewlineChars);
        using (var reader = new StringReader(remainder))
        {
            var numberLine = reader.ReadLine();
            if (numberLine == null)
                return false;

            if (!int.TryParse(numberLine, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                return false;

            var lengthLine = reader.ReadLine();
            if (lengthLine == null)
                return false;

            var lengthPieces = lengthLine.Split(SpaceChar, 3, StringSplitOptions.RemoveEmptyEntries);
            if (lengthPieces.Length != 2)
                return false;

            if (!int.TryParse(lengthPieces[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var length))
                return false;

            var base64Remainder = reader.ReadToEnd();
            if (base64Remainder.IsNullOrWhiteSpace())
                return false;

            base64Remainder = base64Remainder
                .Split(CarriageReturn, StringSplitOptions.RemoveEmptyEntries)
                .Join(string.Empty)
                .TrimStart();

            var result = IsValidBase64String(base64Remainder);
            if (result)
            {
                payload = base64Remainder!;
                return true;
            }

            payload = null;
            return false;
        }
    }

    private static string DecodeBase64PackageUnsafe(string base64Input)
    {
        if (base64Input == null)
            throw new ArgumentNullException(nameof(base64Input));

        var bytes = Convert.FromBase64String(base64Input);

        var mappedCharBuffer = new Span<byte>(new byte[bytes.Length]);
        for (var i = 0; i < bytes.Length; i++)
        {
            mappedCharBuffer[i] = CharMap[bytes[i]];
        }

        const int hashSize = 20; // bytes
        var hashBuffer = mappedCharBuffer[..hashSize];
        var dataBuffer = mappedCharBuffer[hashSize..];

        using (var sha1 = SHA1.Create())
        {
            var computedHashBuffer = new Span<byte>(new byte[hashSize]);

            var areEqual = sha1.TryComputeHash(dataBuffer, computedHashBuffer, out _)
                && computedHashBuffer.SequenceEqual(hashBuffer);

            if (!areEqual)
                throw new InvalidDataException("The given data is not a valid wrapped definition as it has failed a checksum.");
        }

        // need to skip zlib header bytes and trim trailing zlib checksum bytes to enable decompression
        var trimmedBuffer = ZlibToDeflate(dataBuffer);

        using var reader = new MemoryStream(trimmedBuffer.ToArray());
        using var unzipper = new DeflateStream(reader, CompressionMode.Decompress);
        using var writer = new MemoryStream(trimmedBuffer.Length);
        unzipper.CopyTo(writer);

        var decompressed = writer.ToArray();
        var textResult = Encoding.UTF8.GetString(decompressed);
        return textResult.TrimEnd('\0'); // remove a trailing NUL char
    }

    private static bool TryDecodeBase64Package(string base64Input, out string? decodedPackage)
    {
        decodedPackage = null;

        if (base64Input == null)
            return false;

        var bytes = Convert.FromBase64String(base64Input);

        var mappedCharBuffer = new Span<byte>(new byte[bytes.Length]);
        for (var i = 0; i < bytes.Length; i++)
        {
            mappedCharBuffer[i] = CharMap[bytes[i]];
        }

        const int hashSize = 20; // bytes
        var hashBuffer = mappedCharBuffer[..hashSize];
        var dataBuffer = mappedCharBuffer[hashSize..];

        using (var sha1 = SHA1.Create())
        {
            var computedHashBuffer = new Span<byte>(new byte[hashSize]);

            var areEqual = sha1.TryComputeHash(dataBuffer, computedHashBuffer, out _)
                && computedHashBuffer.SequenceEqual(hashBuffer);

            if (!areEqual)
                return false;
        }

        // need to skip zlib header bytes and trim trailing zlib checksum bytes to enable decompression
        var trimmedBuffer = ZlibToDeflate(dataBuffer);

        using var reader = new MemoryStream(trimmedBuffer.ToArray());
        using var unzipper = new DeflateStream(reader, CompressionMode.Decompress);
        using var writer = new MemoryStream(trimmedBuffer.Length);
        unzipper.CopyTo(writer);

        var decompressed = writer.ToArray();
        var textResult = Encoding.UTF8.GetString(decompressed);
        decodedPackage = textResult.TrimEnd('\0'); // remove a trailing NUL char

        return true;
    }

    private static bool IsValidBase64String(string input)
    {
        if (input.IsNullOrWhiteSpace())
            return false;

        var buffer = new Span<byte>(new byte[input.Length]);
        return Convert.TryFromBase64String(input, buffer, out _);
    }

    private static Span<byte> ZlibToDeflate(Span<byte> bytes)
    {
        const int zlibHeaderSize = 2;
        const int zlibTrailerSize = 4;

        var endPosition = bytes.Length - zlibHeaderSize - zlibTrailerSize + 1;
        return bytes[zlibHeaderSize..endPosition];
    }

    private static readonly byte[] CharMap = new byte[]
    {
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
        0x8D, 0x92, 0x4A, 0x11, 0x89, 0x74, 0x6B, 0x91, 0xFB, 0xFE, 0xC9, 0x01, 0xEA, 0x1B, 0xF7, 0xCE
    };

    private static readonly char[] SpaceChar = new[] { ' ' };
    private static readonly char[] CarriageReturn = new[] { '\r' };
    private static readonly char[] NewlineChars = new[] { '\r', '\n' };
    private static readonly OracleTokenizer Tokenizer = new();
}
