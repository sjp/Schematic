using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
{
    public class TooManyColumnsRule : Rule
    {
        public TooManyColumnsRule(RuleLevel level, uint columnLimit = 100)
            : base(RuleTitle, level)
        {
            if (columnLimit == 0)
                throw new ArgumentOutOfRangeException("The column limit must be at least 1.", nameof(columnLimit));

            ColumnLimit = columnLimit;
        }

        protected uint ColumnLimit { get; }

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
            if (columnCount < ColumnLimit)
                return Enumerable.Empty<IRuleMessage>();

            var messageText = $"The table { table.Name } has too many columns. It has { columnCount.ToString() } columns.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        private const string RuleTitle = "Too many columns present on the table.";
    }
}
