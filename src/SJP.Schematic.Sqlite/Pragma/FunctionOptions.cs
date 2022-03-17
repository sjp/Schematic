using System;

namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// See: https://sqlite.org/c3ref/c_deterministic.html
/// </summary>
[Flags]
public enum FunctionOptions
{
    /// <summary>
    /// No flags set.
    /// </summary>
    None = 0,

    /// <summary>
    /// The deterministic flag means that the new function always gives the same output when the input parameters are the same.
    /// </summary>
    Deterministic = 1 << 11, // 0x000000800

    /// <summary>
    /// The direct only flag means that the function may only be invoked from top-level SQL, and cannot be used in VIEWs or TRIGGERs nor in schema structures such as CHECK constraints, DEFAULT clauses, expression indexes, partial indexes, or generated columns.
    /// </summary>
    DirectOnly = 1 << 23, // 0x000080000

    /// <summary>
    /// The sub type flag indicates to SQLite that a function may call <c>sqlite3_value_subtype()</c> to inspect the sub-types of its arguments.
    /// </summary>
    SubType = 1 << 24, // 0x000100000

    /// <summary>
    /// The innocuous flag means that the function is unlikely to cause problems even if misused.
    /// </summary>
    Innocuous = 1 << 25 // 0x000200000
}
