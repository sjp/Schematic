namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// Represents the collation applied to a textual database column.
/// </summary>
public enum SqliteCollation
{
    /// <summary>
    /// An invalid state, used for when the column is non-textual.
    /// </summary>
    None,

    /// <summary>
    /// Compares values by bitwise comparison.
    /// </summary>
    Binary,

    /// <summary>
    /// Compares values by lower-casing ASCII values and performing a bitwise comparison.
    /// </summary>
    NoCase,

    /// <summary>
    /// The same as <see cref="Binary"/> but with trailing whitespace ignored.
    /// </summary>
    Rtrim
}