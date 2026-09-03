using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql;

/// <summary>
/// A MySQL database trigger definition.
/// </summary>
/// <seealso cref="IDatabaseTrigger" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class MySqlDatabaseTrigger : IDatabaseTrigger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseTrigger"/> class.
    /// </summary>
    /// <param name="name">A trigger name.</param>
    /// <param name="definition">The definition of the trigger.</param>
    /// <param name="queryTiming">A trigger query timing.</param>
    /// <param name="events">Table events that cause the trigger to fire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="queryTiming"/> or <paramref name="events"/> is an invalid enum or has invalid values.</exception>
    public MySqlDatabaseTrigger(Identifier name, string definition, TriggerQueryTiming queryTiming, TriggerEvent events)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        if (!queryTiming.IsValid())
            throw new ArgumentException($"The {nameof(TriggerQueryTiming)} provided must be a valid enum.", nameof(queryTiming));
        if (!events.IsValid())
            throw new ArgumentException($"The {nameof(TriggerEvent)} provided must be a valid enum.", nameof(events));
        if (events == TriggerEvent.None)
            throw new ArgumentException("Invalid trigger event flags given. Must include at least one event, e.g. INSERT, DELETE, UPDATE.", nameof(events));

        Name = name.LocalName;
        Definition = definition;
        QueryTiming = queryTiming;
        TriggerEvent = events;
    }

    /// <summary>
    /// A trigger definition.
    /// </summary>
    /// <value>The trigger definition.</value>
    public string Definition { get; }

    /// <summary>
    /// The name of the database trigger.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// Describes when a trigger should be executed within a particular query.
    /// </summary>
    /// <value>The execution timing within a query.</value>
    public TriggerQueryTiming QueryTiming { get; }

    /// <summary>
    /// The table events which cause this trigger to execute.
    /// </summary>
    /// <value>A bitwise value defining which events cause the trigger to fire.</value>
    public TriggerEvent TriggerEvent { get; }

    /// <summary>
    /// Indicates whether this trigger is enabled.
    /// </summary>
    /// <value>Always <see langword="true" />.</value>
    public bool IsEnabled { get; } = true;

    /// <summary>
    /// Describes how often the trigger fires for the statement that caused it to fire.
    /// </summary>
    /// <value>Always <see cref="TriggerGranularity.Row"/>, as MySQL only supports row-level triggers.</value>
    public TriggerGranularity Granularity { get; } = TriggerGranularity.Row;

    /// <summary>
    /// An expression that must evaluate to <c>true</c> for the trigger body to run, i.e. a <c>WHEN</c> clause.
    /// </summary>
    /// <value>Always <see cref="Option{A}.None"/>, as MySQL has no <c>WHEN</c> clause on triggers.</value>
    public Option<string> Condition { get; } = Option<string>.None;

    /// <summary>
    /// The columns that an <c>UPDATE</c> must touch for the trigger to fire, i.e. an <c>UPDATE OF</c> column list.
    /// </summary>
    /// <value>Always empty, as MySQL has no <c>UPDATE OF</c> clause on triggers.</value>
    public IReadOnlyCollection<Identifier> UpdateColumns { get; } = [];

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

            builder.Append("Trigger: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}