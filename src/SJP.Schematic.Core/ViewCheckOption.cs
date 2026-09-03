namespace SJP.Schematic.Core;

/// <summary>
/// Describes the <c>WITH CHECK OPTION</c> applied to an updatable view, which constrains the rows
/// that may be written through the view to those the view's definition would return.
/// </summary>
public enum ViewCheckOption
{
    /// <summary>
    /// The view has no check option, or the database does not support one.
    /// </summary>
    None,

    /// <summary>
    /// Rows written through the view are checked against this view's definition only,
    /// i.e. <c>WITH LOCAL CHECK OPTION</c>.
    /// </summary>
    Local,

    /// <summary>
    /// Rows written through the view are checked against this view's definition and those of any
    /// views it is defined over, i.e. <c>WITH CASCADED CHECK OPTION</c>.
    /// </summary>
    Cascaded,
}
