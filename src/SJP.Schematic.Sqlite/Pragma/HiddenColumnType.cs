
namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// An enumerated value that determines the type of a hidden column.
/// </summary>
public enum HiddenColumnType
{
    /// <summary>
    /// The column is not a hidden column.
    /// </summary>
    None = 0,

    /// <summary>
    /// The column is a regular hidden column.
    /// </summary>
    Hidden = 1,

    /// <summary>
    /// The column is a generated column, whose value is generated at query-time.
    /// </summary>
    VirtualGeneratedColumn = 2,

    /// <summary>
    /// The column is a generated column, whose value is generated as the row is inserted.
    /// </summary>
    StoredGeneratedColumn = 3
}
