using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class CandidateKeyMissingRule : Rule
    {
        protected CandidateKeyMissingRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (table.PrimaryKey != null || table.UniqueKeys.Any())
                return Enumerable.Empty<IRuleMessage>();

            var messageText = $"The table { table.Name } has no candidate (primary or unique) keys. Consider adding one to ensure records are unique.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        private const string RuleTitle = "Table missing a candidate (primary or unique) key.";
    }
}
