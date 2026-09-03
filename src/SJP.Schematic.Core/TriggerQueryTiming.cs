namespace SJP.Schematic.Core;

/// <summary>
/// Describes when a trigger should be executed.
/// </summary>
public enum TriggerQueryTiming
{
    /// <summary>
    /// Occurs before a SQL statement on a table is executed.
    /// </summary>
    Before,

    /// <summary>
    /// Occurs after a SQL statement on a table is executed.
    /// </summary>
    After,

    /// <summary>
    /// Occurs instead of a SQL statement when a statement would modify a table.
    /// </summary>
    InsteadOf,

    /// <summary>
    /// Occurs at more than one timing point within a single statement. Only Oracle supports this,
    /// via a compound trigger, whose sections fire before and after the statement and each row.
    /// </summary>
    Compound,
}
