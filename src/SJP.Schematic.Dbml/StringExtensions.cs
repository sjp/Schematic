using System;
using System.Buffers;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml;

internal static class StringExtensions
{
    private static readonly char[] QuoteChars = ['\'', '"', '[', ']', '`'];
    private static readonly SearchValues<char> SearchQuoteChars = SearchValues.Create(['\'', '"', '[', ']', '`']);

    public static string RemoveQuotingCharacters(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return RemoveCharacters(input, QuoteChars);
    }

    private static string RemoveCharacters(string input, ReadOnlySpan<char> chars)
    {
        ArgumentNullException.ThrowIfNull(input);

        var inputChars = input.AsSpan();

        // fast path
        if (!inputChars.ContainsAnyExcept(SearchQuoteChars))
        {
            return input;
        }

        var builder = StringBuilderCache.Acquire();

        foreach (var c in input)
        {
            if (!chars.Contains(c))
                builder.Append(c);
        }

        return builder.GetStringAndRelease();
    }
}