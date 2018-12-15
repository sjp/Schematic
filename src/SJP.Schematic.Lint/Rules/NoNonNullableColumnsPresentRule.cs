using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class NoNonNullableColumnsPresentRule : Rule
    {
        public NoNonNullableColumnsPresentRule(RuleLevel level)
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

            var containsNotNullColumns = table.Columns.Any(c => !c.IsNullable);
            if (containsNotNullColumns)
                return Array.Empty<IRuleMessage>();

            var message = BuildMessage(table.Name);
            return new[] { message };
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table '{ tableName }' has no not-nullable columns present. Consider adding one to ensure that each record contains data.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "No not-null columns present on the table.";
    }
}
