using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class ColumnWithNumericSuffix : Rule
    {
        public ColumnWithNumericSuffix(RuleLevel level)
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

            var regex = new Regex(".*[0-9]$");
            var columnsWithNumericSuffix = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(c => regex.IsMatch(c))
                .ToList();
            if (columnsWithNumericSuffix.Empty())
                return Array.Empty<IRuleMessage>();

            return columnsWithNumericSuffix
                .Select(c => BuildMessage(table.Name, c))
                .ToList();
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' has a column '{ columnName }' with a numeric suffix, indicating denormalization.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Column with a numeric suffix.";
    }
}
