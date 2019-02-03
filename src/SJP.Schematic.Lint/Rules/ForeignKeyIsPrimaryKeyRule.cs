using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules
{
    public class ForeignKeyIsPrimaryKeyRule : Rule, ITableRule
    {
        public ForeignKeyIsPrimaryKeyRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default(CancellationToken))
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

                var message = BuildMessage(foreignKey.ChildKey.Name, childTableName);
                result.Add(message);
            }

            return result;
        }

        protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");
            foreignKeyName.IfSome(name =>
            {
                builder.Append(" '")
                    .Append(name.LocalName)
                    .Append("'");
            });

            builder.Append(" on ")
                .Append(childTableName)
                .Append(" contains the same column set as the target key.");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Foreign key relationships contains the same columns as the target key.";
    }
}
