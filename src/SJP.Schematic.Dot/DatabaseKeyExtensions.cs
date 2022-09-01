using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dot;

internal static class DatabaseKeyExtensions
{
    public static int GetKeyHash(this IDatabaseKey key, Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(tableName);

        var builder = new HashCode();
        builder.Add(tableName.GetHashCode());
        builder.Add(key.KeyType.GetHashCode());

        foreach (var column in key.Columns)
            builder.Add(column.Name?.LocalName?.GetHashCode(StringComparison.Ordinal) ?? 0);

        return builder.ToHashCode();
    }
}