using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a materialized database view, i.e. a view whose results are stored rather than
/// computed on each query.
/// </summary>
/// <seealso cref="IDatabaseView" />
public interface IDatabaseMaterializedView : IDatabaseView
{
    /// <summary>
    /// Describes when the stored results of the view are refreshed.
    /// </summary>
    /// <value>A refresh mode.</value>
    MaterializedViewRefreshMode RefreshMode { get; }

    /// <summary>
    /// How the stored results of the view are recomputed when it is refreshed, e.g. Oracle's
    /// <c>COMPLETE</c>, <c>FAST</c> or <c>FORCE</c>. None when the database has only one
    /// refresh method.
    /// </summary>
    /// <value>A refresh method, if available.</value>
    Option<string> RefreshMethod { get; }

    /// <summary>
    /// Determines whether the view currently holds data, i.e. whether it has been populated by a
    /// refresh since it was created.
    /// </summary>
    /// <value><see langword="true" /> if this view holds data; otherwise, <see langword="false" />.</value>
    bool IsPopulated { get; }
}
