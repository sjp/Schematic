using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
{
    public class PrimaryKeyColumnNotFirstColumnRule : Rule
    {
        public PrimaryKeyColumnNotFirstColumnRule(RuleLevel level)
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
            if (primaryKey == null)
                return Enumerable.Empty<IRuleMessage>();

            var pkColumns = primaryKey.Columns.ToList();
            if (pkColumns.Count != 1)
                return Enumerable.Empty<IRuleMessage>();

            var tableColumns = table.Columns;
            if (tableColumns.Count == 0)
                return Enumerable.Empty<IRuleMessage>();

            var pkColumnName = pkColumns[0].Name;
            var firstColumnName = table.Columns[0].Name;
            if (pkColumnName == firstColumnName)
                return Enumerable.Empty<IRuleMessage>();

            var messageText = $"The table { table.Name } has a primary key whose column is the first column in the table.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        private const string RuleTitle = "Table primary key whose only column is not the first column in the table.";
    }
}
