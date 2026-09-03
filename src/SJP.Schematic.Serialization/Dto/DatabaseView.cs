using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database view.
/// </summary>
public sealed record DatabaseView
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

    /// <summary>
    /// The triggers defined on the view.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before views carried triggers still reads back,
    /// as a view without any.
    /// </remarks>
    public IEnumerable<DatabaseTrigger> Triggers { get; init; } = [];

    /// <summary>
    /// The indexes defined on the view.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before views carried indexes still reads back,
    /// as a view without any.
    /// </remarks>
    public IEnumerable<DatabaseIndex> Indexes { get; init; } = [];

    /// <summary>
    /// The check option constraining rows written through the view.
    /// </summary>
    public Core.ViewCheckOption CheckOption { get; init; }

    /// <summary>
    /// Whether rows can be written through the view.
    /// </summary>
    public bool IsUpdatable { get; init; }

    /// <summary>
    /// When a materialized view's stored results are refreshed. Ignored when the view is not
    /// materialized.
    /// </summary>
    public Core.MaterializedViewRefreshMode RefreshMode { get; init; }

    /// <summary>
    /// How a materialized view's stored results are recomputed on a refresh, where the database has
    /// more than one method. Ignored when the view is not materialized.
    /// </summary>
    public string? RefreshMethod { get; init; }

    /// <summary>
    /// Whether a materialized view currently holds data. Ignored when the view is not materialized.
    /// </summary>
    public bool IsPopulated { get; init; }
}
