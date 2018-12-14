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
    public class ForeignKeyColumnTypeMismatchRule : Rule
    {
        public ForeignKeyColumnTypeMismatchRule(RuleLevel level)
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
            var tables = await database.TablesAsync(cancellationToken).ConfigureAwait(false);
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
                var childColumns = foreignKey.ChildKey.Columns;
                var parentColumns = foreignKey.ParentKey.Columns;

                var childColumnsInfo = childColumns.Select(c => c.Type.Definition).ToList();
                var parentColumnsInfo = parentColumns.Select(c => c.Type.Definition).ToList();

                var columnsEqual = childColumnsInfo.SequenceEqual(parentColumnsInfo);
                if (columnsEqual)
                    continue;

                var fkName = foreignKey.ChildKey.Name?.ToString() ?? string.Empty;
                var ruleMessage = BuildMessage(fkName, foreignKey.ChildTable, foreignKey.ParentTable);

                result.Add(ruleMessage);
            }

            return result;
        }

        protected virtual IRuleMessage BuildMessage(string foreignKeyName, Identifier childTableName, Identifier parentTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));
            if (parentTableName == null)
                throw new ArgumentNullException(nameof(parentTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");
            if (!foreignKeyName.IsNullOrWhiteSpace())
            {
                builder.Append(" '")
                    .Append(foreignKeyName)
                    .Append("'");
            }

            builder.Append(" from ")
                .Append(childTableName)
                .Append(" to ")
                .Append(parentTableName)
                .Append(" contains mismatching column types. These should be the same in order to ensure that foreign keys can always hold the same information as the target key.");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Foreign key relationships contain mismatching types.";
    }
}
