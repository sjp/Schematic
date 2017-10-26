using System;
using SJP.Schematic.Core;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class SqliteTriggerParser
    {
        public SqliteTriggerParser(TokenList<SqliteToken> tokens)
        {
            if (tokens == default(TokenList<SqliteToken>) || tokens.Empty())
                throw new ArgumentNullException(nameof(tokens));

            var timing = TriggerQueryTiming.After;
            var evt = TriggerEvent.None;

            var triggerDef = SqliteTokenParsers.TriggerDefinition(tokens);
            if (triggerDef.HasValue)
            {
                timing = triggerDef.Value.timing;
                evt = triggerDef.Value.evt;
            }

            Timing = timing;
            Event = evt;
        }

        public TriggerQueryTiming Timing { get; }

        public TriggerEvent Event { get; }
    }
}
