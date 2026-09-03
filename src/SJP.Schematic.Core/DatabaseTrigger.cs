using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database trigger.
/// </summary>
/// <seealso cref="IDatabaseTrigger" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseTrigger : IDatabaseTrigger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTrigger"/> class.
    /// </summary>
    /// <param name="name">The name. Only the local name is kept.</param>
    /// <param name="definition">The definition.</param>
    /// <param name="queryTiming">The query timing.</param>
    /// <param name="events">The events.</param>
    /// <param name="isEnabled">if set to <see langword="true" /> [is enabled].</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace. Also thrown when invalid enum values are provided for <paramref name="queryTiming"/> or <paramref name="events"/>, or when provided a <paramref name="events"/> value of <see cref="TriggerEvent.None"/>.</exception>
    public DatabaseTrigger(Identifier name, string definition, TriggerQueryTiming queryTiming, TriggerEvent events, bool isEnabled)
        : this(name, definition, queryTiming, events, isEnabled, TriggerGranularity.Unknown, Option<string>.None, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTrigger"/> class.
    /// </summary>
    /// <param name="name">The name. Only the local name is kept.</param>
    /// <param name="definition">The definition.</param>
    /// <param name="queryTiming">The query timing.</param>
    /// <param name="events">The events.</param>
    /// <param name="isEnabled">if set to <see langword="true" /> [is enabled].</param>
    /// <param name="granularity">How often the trigger fires for the statement that caused it to fire.</param>
    /// <param name="condition">The <c>WHEN</c> clause that gates the trigger body, if any.</param>
    /// <param name="updateColumns">The <c>UPDATE OF</c> column list, empty when updates to any column fire the trigger.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="definition"/> is <see langword="null" />, or <paramref name="updateColumns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace. Also thrown when invalid enum values are provided for <paramref name="queryTiming"/>, <paramref name="events"/> or <paramref name="granularity"/>, or when provided a <paramref name="events"/> value of <see cref="TriggerEvent.None"/>.</exception>
    public DatabaseTrigger(
        Identifier name,
        string definition,
        TriggerQueryTiming queryTiming,
        TriggerEvent events,
        bool isEnabled,
        TriggerGranularity granularity,
        Option<string> condition,
        IReadOnlyCollection<Identifier> updateColumns
    )
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        if (!queryTiming.IsValid())
            throw new ArgumentException($"The {nameof(TriggerQueryTiming)} provided must be a valid enum.", nameof(queryTiming));
        if (!events.IsValid())
            throw new ArgumentException($"The {nameof(TriggerEvent)} provided must be a valid enum.", nameof(events));
        if (events == TriggerEvent.None)
            throw new ArgumentException("Invalid trigger event flags given. Must include at least one event, e.g. INSERT, DELETE, UPDATE.", nameof(events));
        if (!granularity.IsValid())
            throw new ArgumentException($"The {nameof(TriggerGranularity)} provided must be a valid enum.", nameof(granularity));

        Name = name.LocalName;
        Definition = definition;
        QueryTiming = queryTiming;
        TriggerEvent = events;
        IsEnabled = isEnabled;
        Granularity = granularity;
        Condition = condition;
        UpdateColumns = updateColumns.ToDefensiveCopy(nameof(updateColumns));
    }

    /// <summary>
    /// The name of the database trigger.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// A trigger definition.
    /// </summary>
    /// <value>The trigger definition.</value>
    public string Definition { get; }

    /// <summary>
    /// Describes when a trigger should be executed within a particular query.
    /// </summary>
    /// <value>The execution timing within a query.</value>
    public TriggerQueryTiming QueryTiming { get; }

    /// <summary>
    /// The table events which cause this trigger to execute.
    /// </summary>
    /// <value>
    /// A bitwise value defining which events cause the trigger to fire.
    /// </value>
    public TriggerEvent TriggerEvent { get; }

    /// <summary>
    /// Indicates whether the trigger is enabled.
    /// </summary>
    /// <value><see langword="true" /> if this trigger is enabled; otherwise, <see langword="false" />.</value>
    public bool IsEnabled { get; }

    /// <summary>
    /// Describes how often the trigger fires for the statement that caused it to fire.
    /// </summary>
    /// <value>The trigger granularity, <see cref="TriggerGranularity.Unknown"/> when the database does not report one.</value>
    public TriggerGranularity Granularity { get; }

    /// <summary>
    /// An expression that must evaluate to <c>true</c> for the trigger body to run, i.e. a <c>WHEN</c> clause.
    /// </summary>
    /// <value>The trigger condition, if any.</value>
    public Option<string> Condition { get; }

    /// <summary>
    /// The columns that an <c>UPDATE</c> must touch for the trigger to fire, i.e. an <c>UPDATE OF</c> column list.
    /// </summary>
    /// <value>A collection of column names. Empty when the trigger fires for updates to any column.</value>
    public IReadOnlyCollection<Identifier> UpdateColumns { get; }

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Trigger: ")
                .Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
