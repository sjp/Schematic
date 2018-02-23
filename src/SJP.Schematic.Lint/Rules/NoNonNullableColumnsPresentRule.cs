using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class NoNonNullableColumnsPresentRule : Rule
    {
        public NoNonNullableColumnsPresentRule(RuleLevel level)
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

            var containsNotNullColumns = table.Columns.Any(c => !c.IsNullable);
            if (containsNotNullColumns)
                return Enumerable.Empty<IRuleMessage>();

            var messageText = $"The table '{ table.Name }' has no not-nullable columns present. Consider adding one to ensure that each record contains data.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        private const string RuleTitle = "No not-null columns present on the table.";
    }
}
