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
    public class ForeignKeyColumnTypeMismatchRule : Rule, ITableRule
    {
        public ForeignKeyColumnTypeMismatchRule(RuleLevel level)
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

                var ruleMessage = BuildMessage(foreignKey.ChildKey.Name, foreignKey.ChildTable, foreignKey.ParentTable);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName, Identifier parentTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));
            if (parentTableName == null)
                throw new ArgumentNullException(nameof(parentTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");
            foreignKeyName.IfSome(name =>
            {
                builder.Append(" '")
                    .Append(name.LocalName)
                    .Append("'");
            });

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
