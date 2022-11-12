#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information on errors encountered when checking validity of foreign key.
/// </summary>
public sealed record pragma_foreign_key_check
{
    /// <summary>
    /// The name of the table containing an invalid foreign key reference.
    /// </summary>
    public required string table { get; init; }

    /// <summary>
    /// The internal row number containing an invalid foreign key reference.
    /// </summary>
    public required long rowid { get; init; }

    /// <summary>
    /// The name of the table that is being referenced by the foreign key constraint.
    /// </summary>
    public required string parent { get; init; }

    /// <summary>
    /// The index of the foreign key constraint that is invalid for the table referred to by <see cref="table"/>.
    /// </summary>
    public required int fkid { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles