namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database trigger.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
/// <seealso cref="IDatabaseOptional" />
public interface IDatabaseTrigger : IDatabaseEntity, IDatabaseOptional
{
    /// <summary>
    /// A trigger definition.
    /// </summary>
    /// <value>The trigger definition.</value>
    string Definition { get; }

    /// <summary>
    /// Describes when a trigger should be executed within a particular query.
    /// </summary>
    /// <value>The execution timing within a query.</value>
    TriggerQueryTiming QueryTiming { get; }

    /// <summary>
    /// The table events which cause this trigger to execute.
    /// </summary>
    /// <value>A bitwise value defining which events cause the trigger to fire.</value>
    TriggerEvent TriggerEvent { get; }
}