#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information on the indexes present on a table.
/// </summary>
public sealed record pragma_index_list
{
    /// <summary>
    /// A sequence number assigned to each index for internal tracking purposes.
    /// </summary>
    public required long seq { get; init; }

    /// <summary>
    /// The name of the index.
    /// </summary>
    public required string name { get; init; }

    /// <summary>
    /// Whether the index is unique.
    /// </summary>
    public required bool unique { get; init; }

    /// <summary>
    /// How the index was created. <c>c</c> if the index was created by a create index statement, <c>u</c> if the index was created by a unique constraint, or <c>pk</c> if the index was created by a primary key constraint.
    /// </summary>
    public required string origin { get; init; }

    /// <summary>
    /// Whether the index is a partial index.
    /// </summary>
    public required bool partial { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles