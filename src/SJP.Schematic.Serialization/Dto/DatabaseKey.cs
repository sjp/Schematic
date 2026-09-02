using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized key constraint.
/// </summary>
public sealed record DatabaseKey
{
    /// <summary>
    /// The name of the key constraint, if available.
    /// </summary>
    public Identifier? Name { get; init; }

    /// <summary>
    /// The type of key constraint, e.g. primary, unique, foreign.
    /// </summary>
    public required Core.DatabaseKeyType KeyType { get; init; }

    /// <summary>
    /// The columns that define the key constraint.
    /// </summary>
    /// <remarks>
    /// Each column is written out in full rather than referenced by name; see <see cref="DatabaseColumn"/>
    /// for why.
    /// </remarks>
    public required IEnumerable<DatabaseColumn> Columns { get; init; }

    /// <summary>
    /// Whether the key constraint is enabled.
    /// </summary>
    public required bool IsEnabled { get; init; }

    /// <summary>
    /// The index enforcing the constraint, if the source database reported one.
    /// </summary>
    /// <remarks>
    /// The index is written out in full rather than referenced by name, because it does not appear
    /// in the owning table's indexes; see <see cref="DatabaseColumn"/> for why references are
    /// serialized by value.
    /// </remarks>
    public DatabaseIndex? BackingIndex { get; init; }
}
