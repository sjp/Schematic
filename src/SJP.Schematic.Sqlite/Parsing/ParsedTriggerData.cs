using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Parsing
{
    public sealed class ParsedTriggerData
    {
        public ParsedTriggerData(TriggerQueryTiming queryTiming, TriggerEvent events)
        {
            if (!queryTiming.IsValid())
                throw new ArgumentException($"The { nameof(TriggerQueryTiming) } provided must be a valid enum.", nameof(queryTiming));
            if (!events.IsValid())
                throw new ArgumentException($"The { nameof(TriggerEvent) } provided must be a valid enum.", nameof(events));

            Timing = queryTiming;
            Event = events;
        }

        public TriggerQueryTiming Timing { get; }

        public TriggerEvent Event { get; }
    }
}
