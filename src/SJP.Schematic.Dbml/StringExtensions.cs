using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml;

internal static class StringExtensions
{
    public static string RemoveQuotingCharacters(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return RemoveCharacters(input, QuoteChars);
    }

    private static string RemoveCharacters(string input, IEnumerable<char> chars)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        if (chars == null)
            throw new ArgumentNullException(nameof(chars));

        var builder = StringBuilderCache.Acquire();
        builder.Append(input);

        foreach (var c in chars)
            builder.Replace(c.ToString(), string.Empty);

        return builder.GetStringAndRelease();
    }

    private static readonly IEnumerable<char> QuoteChars = new[] { '\'', '"', '[', ']', '`' };
}
