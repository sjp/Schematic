using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized foreign key relationship between two tables.
/// </summary>
public sealed record DatabaseRelationalKey
{
    /// <summary>
    /// The name of the table that holds the foreign key.
    /// </summary>
    public required Identifier ChildTable { get; init; }

    /// <summary>
    /// The foreign key defined on the child table.
    /// </summary>
    public required DatabaseKey ChildKey { get; init; }

    /// <summary>
    /// The name of the table that the foreign key points to.
    /// </summary>
    public required Identifier ParentTable { get; init; }

    /// <summary>
    /// The primary or unique key on the parent table that the foreign key points to.
    /// </summary>
    public required DatabaseKey ParentKey { get; init; }

    /// <summary>
    /// The action applied to the child rows when a parent row is deleted.
    /// </summary>
    public required ReferentialAction DeleteAction { get; init; }

    /// <summary>
    /// The action applied to the child rows when the parent key's values are updated.
    /// </summary>
    public required ReferentialAction UpdateAction { get; init; }
}
