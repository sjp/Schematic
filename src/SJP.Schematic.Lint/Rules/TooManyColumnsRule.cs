using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class TooManyColumnsRule : Rule
    {
        public TooManyColumnsRule(RuleLevel level, uint columnLimit = 100)
            : base(RuleTitle, level)
        {
            if (columnLimit == 0)
                throw new ArgumentOutOfRangeException(nameof(columnLimit), "The column limit must be at least 1.");

            ColumnLimit = columnLimit;
        }

        protected uint ColumnLimit { get; }

        public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return AnalyseDatabaseAsyncCore(database, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var tables = await database.GetAllTables(cancellationToken).ConfigureAwait(false);
            return tables.SelectMany(AnalyseTable).ToList();
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
