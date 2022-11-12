#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information on columns contained within a table.
/// </summary>
public sealed record pragma_table_xinfo
{
    /// <summary>
    /// The rank of the column within the table.
    /// </summary>
    public required int cid { get; init; }

    /// <summary>
    /// The name of the column.
    /// </summary>
    public required string name { get; init; }

    /// <summary>
    /// The data type associated with the column.
    /// </summary>
    public required string type { get; init; }

    /// <summary>
    /// Whether the column has a not-null constraint present.
    /// </summary>
    public required bool notnull { get; init; }

    /// <summary>
    /// The default value for the column.
    /// </summary>
    public required string? dflt_value { get; init; }

    /// <summary>
    /// Returns <c>0</c> for columns not part of the primary key. Otherwise, stores the 1-based index of the column within the primary key.
    /// </summary>
    public required int pk { get; init; }

    /// <summary>
    /// Determines whether the column is a hidden column as part of a virtual table. Alternatively whether the column is a virtual column.
    /// </summary>
    public required HiddenColumnType hidden { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles