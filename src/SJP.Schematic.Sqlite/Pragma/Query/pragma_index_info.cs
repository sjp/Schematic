#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information on key columns within an index.
/// </summary>
public sealed record pragma_index_info
{
    /// <summary>
    /// The rank of the column within the index.
    /// </summary>
    public required long seqno { get; init; }

    /// <summary>
    /// The rank of the column within the table.
    /// </summary>
    public required int cid { get; init; }

    /// <summary>
    /// The name of the column being indexed. This column is <c>null</c> if the column is the rowid or an expression.
    /// </summary>
    public required string? name { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles