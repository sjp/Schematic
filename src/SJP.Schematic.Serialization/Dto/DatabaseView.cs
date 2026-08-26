using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database view.
/// </summary>
public class DatabaseView
{
    /// <summary>
    /// The name of the view.
    /// </summary>
    public required Identifier ViewName { get; init; }

    /// <summary>
    /// The query that defines the view.
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// The columns the view exposes.
    /// </summary>
    public required IEnumerable<DatabaseColumn> Columns { get; init; }

    /// <summary>
    /// Whether the view's results are stored rather than computed on each query.
    /// </summary>
    public required bool IsMaterialized { get; init; }
}
