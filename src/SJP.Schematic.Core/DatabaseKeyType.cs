namespace SJP.Schematic.Core;

/// <summary>
/// Describes the type of a key constraint on a table.
/// </summary>
public enum DatabaseKeyType
{
    /// <summary>
    /// Primary key.
    /// </summary>
    Primary,

    /// <summary>
    /// Unique key.
    /// </summary>
    Unique,

    /// <summary>
    /// Foreign key.
    /// </summary>
    Foreign
}
