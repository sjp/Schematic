using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A PostgreSQL database trigger definition.
/// </summary>
/// <seealso cref="IDatabaseTrigger" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class PostgreSqlDatabaseTrigger : IDatabaseTrigger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseTrigger"/> class.
    /// </summary>
    /// <param name="name">A trigger name.</param>
    /// <param name="definition">The definition of the trigger.</param>
    /// <param name="queryTiming">A trigger query timing.</param>
    /// <param name="events">Table events that cause the trigger to fire.</param>
    /// <param name="isEnabled">If <see langword="true"/>, determines that the trigger is currently enabled.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>. Alternatively if <paramref name="definition"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="queryTiming"/> or <paramref name="events"/> is an invalid enum or has invalid values.</exception>
    public PostgreSqlDatabaseTrigger(Identifier name, string definition, TriggerQueryTiming queryTiming, TriggerEvent events, bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (definition.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(definition));
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
        IsEnabled = isEnabled;
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
    /// <value><c>true</c> if this trigger is enabled; otherwise, <c>false</c>.</value>
    public bool IsEnabled { get; }

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