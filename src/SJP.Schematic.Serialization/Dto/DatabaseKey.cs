using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized key constraint.
/// </summary>
public class DatabaseKey
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
}
