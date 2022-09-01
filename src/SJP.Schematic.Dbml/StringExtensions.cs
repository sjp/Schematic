using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml;

internal static class StringExtensions
{
    public static string RemoveQuotingCharacters(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return RemoveCharacters(input, QuoteChars);
    }

    private static string RemoveCharacters(string input, IEnumerable<char> chars)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(chars);

        var builder = StringBuilderCache.Acquire();
        builder.Append(input);

        foreach (var c in chars)
            builder.Replace(c.ToString(), string.Empty);

        return builder.GetStringAndRelease();
    }

    private static readonly IEnumerable<char> QuoteChars = new[] { '\'', '"', '[', ']', '`' };
}