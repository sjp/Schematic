using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class ColumnWithNumericSuffix : Rule, ITableRule
    {
        public ColumnWithNumericSuffix(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columnsWithNumericSuffix = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(c => NumericSuffixRegex.IsMatch(c))
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

        private static readonly Regex NumericSuffixRegex = new Regex(".*[0-9]$");
    }
}
