using System;
using System.Collections.Generic;

namespace SJP.Schematic.Sqlite.Parsing;

internal static class ParsingExtensions
{
    public static IEnumerable<T> ToEnumerable<T>(this T input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return [input];
    }
}
