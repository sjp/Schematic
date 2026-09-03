namespace SJP.Schematic.Core;

/// <summary>
/// Describes when the stored results of a materialized view are refreshed.
/// </summary>
public enum MaterializedViewRefreshMode
{
    /// <summary>
    /// The database did not report a refresh mode, or reported one that this enumeration
    /// does not describe.
    /// </summary>
    Unknown,

    /// <summary>
    /// The view is only refreshed when explicitly asked to be, e.g. by PostgreSQL's
    /// <c>REFRESH MATERIALIZED VIEW</c> or Oracle's <c>DBMS_MVIEW.REFRESH</c>.
    /// </summary>
    OnDemand,

    /// <summary>
    /// The view is refreshed as part of committing a transaction that modifies the data
    /// the view is defined over.
    /// </summary>
    OnCommit,

    /// <summary>
    /// The view is never refreshed.
    /// </summary>
    Never,
}
