using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class OrphanedTableRule : Rule
    {
        public OrphanedTableRule(RuleLevel level)
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

            if (table.ParentKeys.Any())
                return Enumerable.Empty<IRuleMessage>();

            if (table.ChildKeys.Any())
                return Enumerable.Empty<IRuleMessage>();

            var messageText = $"The table { table.Name } is not related to any other table. Consider adding relations or removing the table.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        private const string RuleTitle = "No relations on a table. The table is orphaned.";
    }
}
