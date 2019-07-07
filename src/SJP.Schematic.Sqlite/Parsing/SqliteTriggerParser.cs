using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class SqliteTriggerParser
    {
        public ParsedTriggerData ParseTokens(TokenList<SqliteToken> tokens)
        {
            if (tokens == default || tokens.Empty())
                throw new ArgumentNullException(nameof(tokens));

            var timing = TriggerQueryTiming.After;
            var evt = TriggerEvent.None;

            var triggerDef = SqliteTokenParsers.TriggerDefinition(tokens);
            if (triggerDef.HasValue)
            {
                timing = triggerDef.Value.timing;
                evt = triggerDef.Value.evt;
            }

            return new ParsedTriggerData(timing, evt);
        }

        public TriggerQueryTiming Timing { get; }

        public TriggerEvent Event { get; }
    }
}
