using System;
using System.Buffers;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml;

internal static class StringExtensions
{
    private static readonly SearchValues<char> QuoteChars = SearchValues.Create(['\'', '"', '[', ']', '`']);

    public static string RemoveQuotingCharacters(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var inputChars = input.AsSpan();

        // fast path
        if (!inputChars.ContainsAny(QuoteChars))
        {
            return input;
        }

        var builder = StringBuilderCache.Acquire();

        foreach (var c in input)
        {
            if (!QuoteChars.Contains(c))
                builder.Append(c);
        }

        return builder.GetStringAndRelease();
    }
}