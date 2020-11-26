using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Trigger : ITemplateParameter
    {
        public Trigger(
            Identifier tableName,
            Identifier triggerName,
            string rootPath,
            string definition,
            TriggerQueryTiming queryTiming,
            TriggerEvent triggerEvent
        )
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));

            Name = triggerName.ToVisibleName();
            TableName = tableName.ToVisibleName();
            TableUrl = rootPath + UrlRouter.GetTableUrl(tableName);
            TriggerUrl = rootPath + UrlRouter.GetTriggerUrl(tableName, triggerName);
            RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var queryFlags = queryTiming.GetFlags()
                .Select(static qt => TimingDescriptions[qt])
                .OrderBy(static qt => qt)
                .ToList();
            var eventFlags = triggerEvent.GetFlags()
                .Select(static te => EventDescriptions[te])
                .OrderBy(static te => te)
                .ToList();

            QueryTiming = queryFlags.Join(", ");
            Events = eventFlags.Join(", ");
        }

        public ReportTemplate Template { get; } = ReportTemplate.Trigger;

        public string RootPath { get; }

        public string Name { get; }

        public string TriggerUrl { get; }

        public string TableName { get; }

        public string TableUrl { get; }

        public string Definition { get; }

        public string QueryTiming { get; }

        public string Events { get; }

        private static readonly IReadOnlyDictionary<TriggerQueryTiming, string> TimingDescriptions = new Dictionary<TriggerQueryTiming, string>
        {
            [TriggerQueryTiming.After] = "AFTER",
            [TriggerQueryTiming.Before] = "BEFORE",
            [TriggerQueryTiming.InsteadOf] = "INSTEAD OF"
        };

        private static readonly IReadOnlyDictionary<TriggerEvent, string> EventDescriptions = new Dictionary<TriggerEvent, string>
        {
            [TriggerEvent.Delete] = "DELETE",
            [TriggerEvent.Insert] = "INSERT",
            [TriggerEvent.Update] = "UPDATE"
        };
    }
}
