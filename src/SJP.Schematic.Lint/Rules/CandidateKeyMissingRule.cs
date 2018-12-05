using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class CandidateKeyMissingRule : Rule
    {
        public CandidateKeyMissingRule(RuleLevel level)
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

            if (table.PrimaryKey.IsSome || table.UniqueKeys.Count > 0)
                return Array.Empty<IRuleMessage>();

            var ruleMessage = BuildMessage(table.Name);
            return new[] { ruleMessage };
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table { tableName } has no candidate (primary or unique) keys. Consider adding one to ensure records are unique.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Table missing a candidate (primary or unique) key.";
    }
}
