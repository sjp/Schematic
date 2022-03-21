using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// A data container that holds parsed table information from a SQLite <c>CREATE TRIGGER</c> statement.
/// </summary>
public sealed class ParsedTriggerData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedTriggerData"/> class.
    /// </summary>
    /// <param name="queryTiming">A query timing.</param>
    /// <param name="events">Trigger events.</param>
    /// <exception cref="ArgumentException"><paramref name="queryTiming"/> or <paramref name="events"/> are invalid enum values.</exception>
    public ParsedTriggerData(TriggerQueryTiming queryTiming, TriggerEvent events)
    {
        if (!queryTiming.IsValid())
            throw new ArgumentException($"The { nameof(TriggerQueryTiming) } provided must be a valid enum.", nameof(queryTiming));
        if (!events.IsValid())
            throw new ArgumentException($"The { nameof(TriggerEvent) } provided must be a valid enum.", nameof(events));

        Timing = queryTiming;
        Event = events;
    }

    /// <summary>
    /// The parsed trigger query timing from the <c>CREATE TRIGGER</c> statement.
    /// </summary>
    /// <value>A trigger query timing.
    /// </value>
    public TriggerQueryTiming Timing { get; }

    /// <summary>
    /// The parsed trigger event from the <c>CREATE TRIGGER</c> statement.
    /// </summary>
    /// <value>A trigger event.</value>
    public TriggerEvent Event { get; }
}