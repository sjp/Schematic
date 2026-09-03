using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The triggers summary payload (<c>data/triggers.json</c>): every trigger in the schema with its
/// owning table, timing, granularity, events, condition, and definition. Triggers have no
/// per-object detail page — they also fold into the owning table's detail payload.
/// </summary>
public sealed class Triggers
{
    public Triggers(IEnumerable<TriggerRow> triggers)
    {
        if (triggers.NullOrAnyNull())
            throw new ArgumentNullException(nameof(triggers));

        TriggersCount = triggers.UCount();
        AllTriggers = triggers;
    }

    public uint TriggersCount { get; }

    public IEnumerable<TriggerRow> AllTriggers { get; }

    /// <summary>
    /// A row in the triggers summary list: a trigger and a hash-route link to the table it belongs
    /// to. Named distinctly from <see cref="Table.Trigger"/> so the JSON source generator emits
    /// non-colliding metadata.
    /// </summary>
    public sealed class TriggerRow
    {
        public TriggerRow(
            Identifier tableName,
            Identifier triggerName,
            string definition,
            TriggerQueryTiming queryTiming,
            TriggerEvent triggerEvent,
            TriggerGranularity granularity,
            Option<string> condition,
            IEnumerable<Identifier> updateColumns
        )
        {
            ArgumentNullException.ThrowIfNull(tableName);
            ArgumentNullException.ThrowIfNull(triggerName);
            ArgumentNullException.ThrowIfNull(updateColumns);

            Name = triggerName.ToVisibleName();
            TableName = tableName.ToVisibleName();
            TableUrl = UrlRouter.GetTableUrl(tableName);
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var eventFlags = triggerEvent.GetFlags()
                .Select(static te => GetEventDescription(te))
                .Order(StringComparer.Ordinal)
                .ToList();

            QueryTiming = GetTimingDescription(queryTiming);
            Events = eventFlags.Join(", ");
            Granularity = GetGranularityDescription(granularity);
            Condition = condition.MatchUnsafe(static c => c, static () => string.Empty) ?? string.Empty;
            UpdateColumns = updateColumns.Select(static c => c.LocalName).Join(", ");
        }

        public string Name { get; }

        public string TableName { get; }

        public string TableUrl { get; }

        public string Definition { get; }

        public string QueryTiming { get; }

        public string Events { get; }

        /// <summary>How often the trigger fires. Empty when the database did not report a granularity.</summary>
        public string Granularity { get; }

        /// <summary>The trigger's <c>WHEN</c> clause. Empty when the trigger is unconditional.</summary>
        public string Condition { get; }

        /// <summary>The trigger's <c>UPDATE OF</c> column list. Empty when updates to any column fire it.</summary>
        public string UpdateColumns { get; }

        private static string GetTimingDescription(TriggerQueryTiming timing) => timing switch
        {
            TriggerQueryTiming.After => "AFTER",
            TriggerQueryTiming.Before => "BEFORE",
            TriggerQueryTiming.InsteadOf => "INSTEAD OF",
            TriggerQueryTiming.Compound => "COMPOUND",
            _ => throw new ArgumentOutOfRangeException(nameof(timing)),
        };

        private static string GetGranularityDescription(TriggerGranularity granularity) => granularity switch
        {
            TriggerGranularity.Row => "FOR EACH ROW",
            TriggerGranularity.Statement => "FOR EACH STATEMENT",
            TriggerGranularity.Unknown => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(granularity)),
        };

        private static string GetEventDescription(TriggerEvent triggerEvent) => triggerEvent switch
        {
            TriggerEvent.Delete => "DELETE",
            TriggerEvent.Insert => "INSERT",
            TriggerEvent.Update => "UPDATE",
            TriggerEvent.Truncate => "TRUNCATE",
            TriggerEvent.Other => "OTHER",
            _ => throw new ArgumentOutOfRangeException(nameof(triggerEvent)),
        };
    }
}
