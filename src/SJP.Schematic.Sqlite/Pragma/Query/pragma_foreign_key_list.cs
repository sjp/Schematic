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
    public long id { get; init; }

    /// <summary>
    /// The order of the column as it appears within the foreign key constraint.
    /// </summary>
    public int seq { get; init; }

    /// <summary>
    /// The name of the table referenced by the foreign key constraint.
    /// </summary>
    public string table { get; init; } = default!;

    /// <summary>
    /// The name of the column that is present within a foreign key constraint.
    /// </summary>
    public string from { get; init; } = default!;

    /// <summary>
    /// The name of the column that is referenced by <see cref="from"/> in the parent key constraint in <see cref="table"/>.
    /// </summary>
    public string to { get; init; } = default!;

    /// <summary>
    /// The action to take when a referenced foreign key entity is updated.
    /// </summary>
    public string on_update { get; init; } = default!;

    /// <summary>
    /// The action to take when a referenced foreign key entity is deleted.
    /// </summary>
    public string on_delete { get; init; } = default!;

    /// <summary>
    /// The action to take on a <c>MATCH</c> clause.
    /// </summary>
    public string match { get; init; } = default!;
}
#pragma warning restore IDE1006, S101 // Naming Styles