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
    InsteadOf
}
