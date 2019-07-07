using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class OnlyOneColumnPresentRule : Rule, ITableRule
    {
        public OnlyOneColumnPresentRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var messages = AnalyseTables(tables);
            return Task.FromResult(messages);
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columnCount = table.Columns.Count;
            if (columnCount > 1)
                return Array.Empty<IRuleMessage>();

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
