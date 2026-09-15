using System;
using System.Collections.Generic;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// Compares strings ignoring the case of ASCII letters only, which is how SQLite matches table names.
/// Letters outside the ASCII range must match exactly.
/// </summary>
internal sealed class AsciiCaseInsensitiveStringComparer : IEqualityComparer<string>
{
    private AsciiCaseInsensitiveStringComparer()
    {
    }

    /// <summary>
    /// The comparer instance.
    /// </summary>
    public static AsciiCaseInsensitiveStringComparer Instance { get; } = new();

    public bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x == null || y == null || x.Length != y.Length)
            return false;

        for (var i = 0; i < x.Length; i++)
        {
            if (ToLowerAscii(x[i]) != ToLowerAscii(y[i]))
                return false;
        }

        return true;
    }

    public int GetHashCode(string obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var hash = new HashCode();
        foreach (var c in obj)
            hash.Add(ToLowerAscii(c));

        return hash.ToHashCode();
    }

    private static char ToLowerAscii(char c) => char.IsAsciiLetterUpper(c) ? (char)(c | 0x20) : c;
}
