#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information on a tables or views in a schema.
/// </summary>
public sealed record pragma_table_list
{
    /// <summary>
    /// The schema in which the table or view appears (for example <c>main</c> or <c>temp</c>).
    /// </summary>
    public required string schema { get; init; }

    /// <summary>
    /// The name of the table or view.
    /// </summary>
    public required string name { get; init; }

    /// <summary>
    /// The type of object. One of <c>table</c>, <c>view</c>, <c>shadow</c> (for shadow tables), or <c>virtual</c> for virtual tables.
    /// </summary>
    public required string type { get; init; }

    /// <summary>
    /// The number of columns in the table, including generated columns and hidden columns.
    /// </summary>
    public required int ncol { get; init; }

    /// <summary>
    /// Whether the table is a <c>WITHOUT ROWID</c> table (<see langword="false" /> if it is not).
    /// </summary>
    public required bool wr { get; init; }

    /// <summary>
    /// Whether if the table is a STRICT table (<see langword="false" /> if it is not).
    /// </summary>
    public required bool strict { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles