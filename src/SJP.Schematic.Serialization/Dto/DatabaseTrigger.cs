using System.Collections.Generic;

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

    /// <summary>
    /// How often the trigger fires for the statement that caused it to fire.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before triggers carried a granularity still reads
    /// back, as an unknown granularity.
    /// </remarks>
    public Core.TriggerGranularity Granularity { get; init; }

    /// <summary>
    /// The <c>WHEN</c> clause that gates the trigger body, if any.
    /// </summary>
    public string? Condition { get; init; }

    /// <summary>
    /// The <c>UPDATE OF</c> column list. Empty when updates to any column fire the trigger.
    /// </summary>
    public IEnumerable<Identifier> UpdateColumns { get; init; } = [];
}
