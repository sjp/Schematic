using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules
{
    public class ForeignKeyIsPrimaryKeyRule : Rule
    {
        public ForeignKeyIsPrimaryKeyRule(RuleLevel level)
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

            var result = new List<IRuleMessage>();

            var foreignKeys = table.ParentKeys;
            foreach (var foreignKey in foreignKeys)
            {
                var childTableName = foreignKey.ChildTable;
                var parentTableName = foreignKey.ParentTable;
                if (childTableName != parentTableName)
                    continue;

                var childColumns = foreignKey.ChildKey.Columns;
                var parentColumns = foreignKey.ParentKey.Columns;

                var childColumnNames = childColumns.Select(c => c.Name).ToList();
                var parentColumnNames = parentColumns.Select(c => c.Name).ToList();

                var columnsEqual = childColumnNames.SequenceEqual(parentColumnNames);
                if (!columnsEqual)
                    continue;

                var fkName = foreignKey.ChildKey.Name?.ToString() ?? string.Empty;
                var message = BuildMessage(fkName, childTableName);
                result.Add(message);
            }

            return result;
        }

        protected virtual IRuleMessage BuildMessage(string foreignKeyName, Identifier childTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");
            if (!foreignKeyName.IsNullOrWhiteSpace())
            {
                builder.Append(" '")
                    .Append(foreignKeyName)
                    .Append("'");
            }

            builder.Append(" on ")
                .Append(childTableName)
                .Append(" contains the same column set as the target key.");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Foreign key relationships contains the same columns as the target key.";
    }
}
