using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
{
    public class OnlyOneColumnPresentRule : Rule
    {
        public OnlyOneColumnPresentRule(RuleLevel level)
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

            var columnCount = table.Columns.Count;
            if (columnCount > 1)
                return Enumerable.Empty<IRuleMessage>();

            var messageText = columnCount == 0
                ? $"The table { table.Name } has too few columns. It has no columns, consider adding more."
                : $"The table { table.Name } has too few columns. It has { columnCount.ToString() } column, consider adding more.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        private const string RuleTitle = "Only one column present on table.";
    }
}
