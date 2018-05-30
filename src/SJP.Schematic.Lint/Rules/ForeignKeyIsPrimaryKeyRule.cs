using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class ForeignKeyIsPrimaryKeyRule : Rule
    {
        public ForeignKeyIsPrimaryKeyRule(RuleLevel level)
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
                var message = BuildMessage(fkName, foreignKey.ChildKey.Table.Name);
                result.Add(message);
            }

            return result;
        }

        protected virtual IRuleMessage BuildMessage(string foreignKeyName, Identifier childTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));

            var builder = new StringBuilder("A foreign key");
            if (!foreignKeyName.IsNullOrWhiteSpace())
            {
                builder.Append(" '")
                    .Append(foreignKeyName)
                    .Append("'");
            }

            builder.Append(" on ")
                .Append(childTableName)
                .Append(" contains the same column set as the target key.");

            return new RuleMessage(RuleTitle, Level, builder.ToString());
        }

        protected static string RuleTitle { get; } = "Foreign key relationships contains the same columns as the target key.";
    }
}
