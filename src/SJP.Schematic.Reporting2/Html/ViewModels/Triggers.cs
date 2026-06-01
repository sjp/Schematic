using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The triggers summary payload (<c>data/triggers.json</c>): every trigger in the schema with its
/// owning table, timing, events, and definition. Triggers have no per-object detail page — they
/// also fold into the owning table's detail payload.
/// </summary>
public sealed class Triggers : ITemplateParameter
{
    public Triggers(IEnumerable<TriggerRow> triggers)
    {
        if (triggers.NullOrAnyNull())
            throw new ArgumentNullException(nameof(triggers));

        TriggersCount = triggers.UCount();
        AllTriggers = triggers;
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Triggers;

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
            TriggerEvent triggerEvent
        )
        {
            ArgumentNullException.ThrowIfNull(tableName);
            ArgumentNullException.ThrowIfNull(triggerName);

            Name = triggerName.ToVisibleName();
            TableName = tableName.ToVisibleName();
            TableUrl = UrlRouter.GetTableUrl(tableName);
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var queryFlags = queryTiming.GetFlags()
                .Select(static qt => TimingDescriptions[qt])
                .Order(StringComparer.Ordinal)
                .ToList();
            var eventFlags = triggerEvent.GetFlags()
                .Select(static te => EventDescriptions[te])
                .Order(StringComparer.Ordinal)
                .ToList();

            QueryTiming = queryFlags.Join(", ");
            Events = eventFlags.Join(", ");
        }

        public string Name { get; }

        public string TableName { get; }

        public string TableUrl { get; }

        public string Definition { get; }

        public string QueryTiming { get; }

        public string Events { get; }

        private static readonly IReadOnlyDictionary<TriggerQueryTiming, string> TimingDescriptions = new Dictionary<TriggerQueryTiming, string>
        {
            [TriggerQueryTiming.After] = "AFTER",
            [TriggerQueryTiming.Before] = "BEFORE",
            [TriggerQueryTiming.InsteadOf] = "INSTEAD OF",
        };

        private static readonly IReadOnlyDictionary<TriggerEvent, string> EventDescriptions = new Dictionary<TriggerEvent, string>
        {
            [TriggerEvent.Delete] = "DELETE",
            [TriggerEvent.Insert] = "INSERT",
            [TriggerEvent.Update] = "UPDATE",
        };
    }
}
