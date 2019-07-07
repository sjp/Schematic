using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class TooManyColumnsRule : Rule, ITableRule
    {
        public TooManyColumnsRule(RuleLevel level, uint columnLimit = 100)
            : base(RuleTitle, level)
        {
            if (columnLimit == 0)
                throw new ArgumentOutOfRangeException(nameof(columnLimit), "The column limit must be at least 1.");

            ColumnLimit = columnLimit;
        }

        protected uint ColumnLimit { get; }

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
            if (columnCount <= ColumnLimit)
                return Array.Empty<IRuleMessage>();

            var message = BuildMessage(table.Name, columnCount);
            return new[] { message };
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, int columnCount)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table { tableName } has too many columns. It has { columnCount.ToString() } columns.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Too many columns present on the table.";
    }
}
