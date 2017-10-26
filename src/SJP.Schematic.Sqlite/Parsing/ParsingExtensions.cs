using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using Superpower;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal static class ParsingExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T input)
        {
            if (ReferenceEquals(input, null))
                throw new ArgumentNullException(nameof(input));

            return new[] { input };
        }

        // useful for turning array objects to enumerables
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable<T> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input;
        }

        public static TokenListParser<TKind, Token<TKind>> NotEqualTo<TKind>(params TKind[] kinds)
        {
            var expectations = new[] { kinds.Select(k => k.ToString()).Join(", ") };

            return input =>
            {
                var next = input.ConsumeToken();
                if (!next.HasValue || kinds.Any(k => next.Value.Kind.Equals(k)))
                    return TokenListParserResult.Empty<TKind, Token<TKind>>(input, expectations);

                return next;
            };
        }
    }
}
