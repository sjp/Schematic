using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml
{
    internal static class StringExtensions
    {
        public static string RemoveCharacters(this string input, IEnumerable<char> chars)
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
    }
}
