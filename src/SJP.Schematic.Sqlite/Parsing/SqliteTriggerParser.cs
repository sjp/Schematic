using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    /// <summary>
    /// A parser for SQLite <c>CREATE TRIGGER</c> definitions.
    /// </summary>
    public class SqliteTriggerParser
    {
        /// <summary>
        /// Parses the tokens into structured trigger definition.
        /// </summary>
        /// <param name="tokens">A collection of tokens from the trigger definition.</param>
        /// <returns>Parsed data for a <c>CREATE TRIGGER</c> definition.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tokens"/> is empty.</exception>
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

        /// <summary>
        /// Gets the timing of when the trigger will execute.
        /// </summary>
        /// <value>The trigger timing.</value>
        public TriggerQueryTiming Timing { get; }

        /// <summary>
        /// Gets the table events that cause the event to fire.
        /// </summary>
        /// <value>The table DML events.</value>
        public TriggerEvent Event { get; }
    }
}
