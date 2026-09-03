using System.Collections.Generic;
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

    /// <summary>
    /// How the relationship treats child rows whose key columns are only partially <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before relationships carried a match type still reads
    /// back, as <see cref="ForeignKeyMatchType.Simple"/>.
    /// </remarks>
    public ForeignKeyMatchType MatchType { get; init; }

    /// <summary>
    /// The child key columns set to <c>null</c> when <see cref="DeleteAction"/> is
    /// <see cref="ReferentialAction.SetNull"/>.
    /// </summary>
    /// <remarks>
    /// Each column is written out in full rather than referenced by name; see <see cref="DatabaseColumn"/>
    /// for why.
    /// </remarks>
    public IEnumerable<DatabaseColumn> SetNullColumns { get; init; } = [];
}
