using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
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

            var message = BuildMessage(table.Name, columnCount);
            return new[] { message };
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, int columnCount)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = columnCount == 0
                ? $"The table { tableName } has too few columns. It has no columns, consider adding more."
                : $"The table { tableName } has too few columns. It has one column, consider adding more.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Only one column present on table.";
    }
}
