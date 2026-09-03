namespace SJP.Schematic.Core;

/// <summary>
/// Describes how often a trigger fires for the statement that caused it to fire.
/// </summary>
public enum TriggerGranularity
{
    /// <summary>
    /// The database did not report a granularity, or the trigger fires at more than one
    /// granularity, e.g. an Oracle compound trigger.
    /// </summary>
    Unknown,

    /// <summary>
    /// The trigger fires once for every row affected by the statement, i.e. <c>FOR EACH ROW</c>.
    /// </summary>
    Row,

    /// <summary>
    /// The trigger fires once for the statement, regardless of how many rows it affects,
    /// i.e. <c>FOR EACH STATEMENT</c>.
    /// </summary>
    Statement,
}
