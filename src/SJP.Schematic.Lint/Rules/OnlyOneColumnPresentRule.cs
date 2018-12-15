using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class OnlyOneColumnPresentRule : Rule
    {
        public OnlyOneColumnPresentRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

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
