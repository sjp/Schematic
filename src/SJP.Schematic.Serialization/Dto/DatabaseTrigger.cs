namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database trigger.
/// </summary>
public sealed record DatabaseTrigger
{
    /// <summary>
    /// The name of the trigger.
    /// </summary>
    public required Identifier TriggerName { get; init; }

    /// <summary>
    /// The definition of the trigger.
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// When the trigger fires relative to the statement it is attached to.
    /// </summary>
    public required Core.TriggerQueryTiming QueryTiming { get; init; }

    /// <summary>
    /// The kinds of statement that cause the trigger to fire.
    /// </summary>
    public required Core.TriggerEvent TriggerEvent { get; init; }

    /// <summary>
    /// Whether the trigger is enabled.
    /// </summary>
    public required bool IsEnabled { get; init; }
}
