using Superpower.Model;
using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.SQLite
{
    public class SQLiteTriggerParser
    {
        public SQLiteTriggerParser(TokenList<SqlToken> tokens)
        {
            if (tokens == default(TokenList<SqlToken>) || tokens.Empty())
                throw new ArgumentNullException(nameof(tokens));

            var parseResult = ParseTokens(tokens);
            Timing = parseResult.Timing;
            Event = parseResult.Event;
        }

        public TriggerQueryTiming Timing { get; }

        public TriggerEvent Event { get; }

        private static ParseResult ParseTokens(TokenList<SqlToken> tokens)
        {
            var next = tokens.ConsumeToken();

            var timing = TriggerQueryTiming.After;
            var triggerEvent = TriggerEvent.None;

            var foundTiming = false;

            do
            {
                var kind = next.Value.Kind;
                var span = next.Value.Span;

                if (!foundTiming && kind == SqlToken.Keyword && TimingKeywords.Contains(span.ToStringValue()))
                {
                    foundTiming = true;
                    var timingSpan = span.ToStringValue();
                    if (timingSpan.Equals("INSTEAD", StringComparison.OrdinalIgnoreCase))
                        timingSpan = "InsteadOf";
                    if (!Enum.TryParse(timingSpan, true, out timing))
                        throw new Exception("Failed to parse timing span: " + timingSpan);
                }
                else if (foundTiming && kind == SqlToken.Keyword && EventKeywords.Contains(span.ToStringValue()))
                {
                    var eventSpan = span.ToStringValue();
                    if (!Enum.TryParse(eventSpan, true, out triggerEvent))
                        throw new Exception("Failed to event span: " + eventSpan);

                    return new ParseResult(timing, triggerEvent);
                }

                next = next.Remainder.ConsumeToken();
            }
            while (!next.Remainder.IsAtEnd);

            throw new Exception("Failed to parse trigger successfully.");
        }

        private static ISet<string> TimingKeywords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BEFORE", "AFTER", "INSTEAD" };
        private static ISet<string> EventKeywords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "DELETE", "INSERT", "UPDATE" };

        private class ParseResult
        {
            public ParseResult(TriggerQueryTiming timing, TriggerEvent triggerEvent)
            {
                Timing = timing;
                Event = triggerEvent;
            }

            public TriggerQueryTiming Timing { get; }

            public TriggerEvent Event { get; }
        }
    }
}
