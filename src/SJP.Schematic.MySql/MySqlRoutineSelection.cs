using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace SJP.Schematic.MySql;

/// <summary>
/// Picks one routine per name from catalog rows.
/// </summary>
/// <remarks>
/// MySQL keys routines on (schema, name, type), so a schema may hold a function and a procedure that share a name.
/// Only one routine is exposed per name: the function, which also carries a return type. The procedure is hidden.
/// </remarks>
internal static class MySqlRoutineSelection
{
    /// <summary>
    /// Keeps one row per routine name, preferring a function's row over a procedure's.
    /// </summary>
    public static IEnumerable<T> OnePerName<T>(IEnumerable<T> rows, Func<T, string> nameSelector, Func<T, string> typeSelector)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(nameSelector);
        ArgumentNullException.ThrowIfNull(typeSelector);

        // routine names are case-insensitive in MySQL
        return rows
            .GroupBy(nameSelector, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderBy(row => TypeRank(typeSelector(row))).First());
    }

    /// <summary>
    /// Picks the row for the given routine name, preferring a function's row over a procedure's.
    /// </summary>
    /// <remarks>
    /// The catalog compares names under its own collation, which may also match a differently accented name, so
    /// rows are matched to the name again here.
    /// </remarks>
    public static Option<T> ForName<T>(IEnumerable<T> rows, string routineName, Func<T, string> nameSelector, Func<T, string> typeSelector)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(routineName);
        ArgumentNullException.ThrowIfNull(nameSelector);
        ArgumentNullException.ThrowIfNull(typeSelector);

        return rows
            .Where(row => string.Equals(nameSelector(row), routineName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => TypeRank(typeSelector(row)))
            .HeadOrNone();
    }

    private static int TypeRank(string routineType) => string.Equals(routineType, Function, StringComparison.OrdinalIgnoreCase) ? 0 : 1;

    private const string Function = "FUNCTION";
}
