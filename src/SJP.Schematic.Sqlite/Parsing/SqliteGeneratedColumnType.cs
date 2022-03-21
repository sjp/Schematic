namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// Represents the method used to calculate the value of a computed column.
/// </summary>
public enum SqliteGeneratedColumnType
{
    /// <summary>
    /// The column is not generated.
    /// </summary>
    None,

    /// <summary>
    /// The value of a VIRTUAL column is computed when read.
    /// </summary>
    Virtual,

    /// <summary>
    /// The value of a STORED column is computed when the row is written.
    /// </summary>
    Stored
}