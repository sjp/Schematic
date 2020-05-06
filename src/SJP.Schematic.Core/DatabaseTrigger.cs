using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
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
        /// <param name="name">The name.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="queryTiming">The query timing.</param>
        /// <param name="events">The events.</param>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>, or <paramref name="definition"/> is <c>null, empty, or whitespace</c>.</exception>
        /// <exception cref="ArgumentException">If invalid enum values are provided for <paramref name="queryTiming"/> or <paramref name="events"/>. Additionally this will be thrown when provided a <paramref name="events"/> value of <see cref="TriggerEvent.None"/>.</exception>
        public DatabaseTrigger(Identifier name, string definition, TriggerQueryTiming queryTiming, TriggerEvent events, bool isEnabled)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (!queryTiming.IsValid())
                throw new ArgumentException($"The { nameof(TriggerQueryTiming) } provided must be a valid enum.", nameof(queryTiming));
            if (!events.IsValid())
                throw new ArgumentException($"The { nameof(TriggerEvent) } provided must be a valid enum.", nameof(events));
            if (events == TriggerEvent.None)
                throw new ArgumentException("Invalid trigger event flags given. Must include at least one event, e.g. INSERT, DELETE, UPDATE.", nameof(events));

            Name = name.LocalName;
            Definition = definition;
            QueryTiming = queryTiming;
            TriggerEvent = events;
            IsEnabled = isEnabled;
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
}
