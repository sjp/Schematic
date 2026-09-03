using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

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
        : this(queryTiming, events, Option<string>.None, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedTriggerData"/> class.
    /// </summary>
    /// <param name="queryTiming">A query timing.</param>
    /// <param name="events">Trigger events.</param>
    /// <param name="condition">The <c>WHEN</c> clause that gates the trigger body, if any.</param>
    /// <param name="updateColumns">The <c>UPDATE OF</c> column list, empty when updates to any column fire the trigger.</param>
    /// <exception cref="ArgumentNullException"><paramref name="updateColumns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="queryTiming"/> or <paramref name="events"/> are invalid enum values.</exception>
    public ParsedTriggerData(TriggerQueryTiming queryTiming, TriggerEvent events, Option<string> condition, IReadOnlyCollection<Identifier> updateColumns)
    {
        if (!queryTiming.IsValid())
            throw new ArgumentException($"The {nameof(TriggerQueryTiming)} provided must be a valid enum.", nameof(queryTiming));
        if (!events.IsValid())
            throw new ArgumentException($"The {nameof(TriggerEvent)} provided must be a valid enum.", nameof(events));
        if (updateColumns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(updateColumns));

        Timing = queryTiming;
        Event = events;
        Condition = condition;
        UpdateColumns = updateColumns.ToList();
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

    /// <summary>
    /// The parsed <c>WHEN</c> clause from the <c>CREATE TRIGGER</c> statement.
    /// </summary>
    /// <value>A trigger condition, if any.</value>
    public Option<string> Condition { get; }

    /// <summary>
    /// The parsed <c>UPDATE OF</c> column list from the <c>CREATE TRIGGER</c> statement.
    /// </summary>
    /// <value>A collection of column names. Empty when the trigger fires for updates to any column.</value>
    public IReadOnlyCollection<Identifier> UpdateColumns { get; }
}
