using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database index.
/// </summary>
public class DatabaseIndex
{
    /// <summary>
    /// The name of the index.
    /// </summary>
    public required Identifier IndexName { get; init; }

    /// <summary>
    /// The index columns that form the primary basis of the index.
    /// </summary>
    public required IEnumerable<DatabaseIndexColumn> Columns { get; init; }

    /// <summary>
    /// The included or leaf columns that are also available once the key columns have been searched.
    /// </summary>
    /// <remarks>
    /// Each column is written out in full rather than referenced by name; see <see cref="DatabaseColumn"/>
    /// for why.
    /// </remarks>
    public required IEnumerable<DatabaseColumn> IncludedColumns { get; init; }

    /// <summary>
    /// Whether the covered index columns must be unique across the index column set.
    /// </summary>
    public required bool IsUnique { get; init; }

    /// <summary>
    /// Whether the index is enabled.
    /// </summary>
    public required bool IsEnabled { get; init; }

    /// <summary>
    /// The expression restricting a filtered index to a subset of rows, if any.
    /// </summary>
    public string? FilterDefinition { get; init; }
}
