using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
{
    public class NoSurrogatePrimaryKeyRule : Rule
    {
        public NoSurrogatePrimaryKeyRule(RuleLevel level)
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

            var primaryKey = table.PrimaryKey;
            if (primaryKey == null || primaryKey.Columns.Count() == 1)
                return Enumerable.Empty<IRuleMessage>();

            var messageText = $"The table { table.Name } has a multi-column primary key. Consider introducing a surrogate primary key.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        private const string RuleTitle = "No surrogate primary key present on table.";
    }
}
