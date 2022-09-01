using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using Superpower;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing;

internal static class ParsingExtensions
{
    public static IEnumerable<T> ToEnumerable<T>(this T input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new[] { input };
    }

    // useful for turning array objects to enumerables
    public static IEnumerable<T> AsEnumerable<T>(this IEnumerable<T> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input;
    }

    public static TokenListParser<TKind, Token<TKind>> NotEqualTo<TKind>(this IEnumerable<TKind> kinds)
    {
        var expectations = new[]
        {
            kinds.Select(k => k?.ToString() ?? string.Empty)
                .Where(static k => k.Length > 0)
                .Join(", ")
        };

        return input =>
        {
            var next = input.ConsumeToken();
            if (!next.HasValue || kinds.Any(k => next.Value.Kind != null && next.Value.Kind.Equals(k)))
                return TokenListParserResult.Empty<TKind, Token<TKind>>(input, expectations);

            return next;
        };
    }
}