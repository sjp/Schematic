#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information on the foreign key constraints present on a table.
/// </summary>
public sealed record pragma_foreign_key_list
{
    /// <summary>
    /// The order of the foreign key constraint as it was created on a table.
    /// </summary>
    public required long id { get; init; }

    /// <summary>
    /// The order of the column as it appears within the foreign key constraint.
    /// </summary>
    public required int seq { get; init; }

    /// <summary>
    /// The name of the table referenced by the foreign key constraint.
    /// </summary>
    public required string table { get; init; }

    /// <summary>
    /// The name of the column that is present within a foreign key constraint.
    /// </summary>
    public required string from { get; init; }

    /// <summary>
    /// The name of the column that is referenced by <see cref="from"/> in the parent key constraint in <see cref="table"/>.
    /// </summary>
    public required string to { get; init; }

    /// <summary>
    /// The action to take when a referenced foreign key entity is updated.
    /// </summary>
    public required string on_update { get; init; }

    /// <summary>
    /// The action to take when a referenced foreign key entity is deleted.
    /// </summary>
    public required string on_delete { get; init; }

    /// <summary>
    /// The action to take on a <c>MATCH</c> clause.
    /// </summary>
    public required string match { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles