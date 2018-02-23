using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class ForeignKeyIsPrimaryKeyRule : Rule
    {
        protected ForeignKeyIsPrimaryKeyRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var foreignKeys = table.ParentKeys.ToList();
            foreach (var foreignKey in foreignKeys)
            {
                var childTableName = foreignKey.ChildKey.Table.Name;
                var parentTableName = foreignKey.ParentKey.Table.Name;
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

                var builder = new StringBuilder("A foreign key");
                if (!fkName.IsNullOrEmpty())
                {
                    builder.Append(" '")
                        .Append(fkName)
                        .Append("'");
                }

                builder.Append(" on ")
                    .Append(foreignKey.ChildKey.Table.Name)
                    .Append(" contains the same column set as the target key.");

                var ruleMessage = new RuleMessage(RuleTitle, Level, builder.ToString());
                result.Add(ruleMessage);
            }

            return result;
        }

        private const string RuleTitle = "Foreign key relationships contains the same columns as the target key.";
    }
}
