using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Analysis.Rules
{
    public class ForeignKeyColumnTypeMismatchRule : Rule
    {
        protected ForeignKeyColumnTypeMismatchRule(RuleLevel level)
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
                var childColumns = foreignKey.ChildKey.Columns;
                var parentColumns = foreignKey.ParentKey.Columns;

                var childColumnsInfo = childColumns.Select(c => new { c.Type.DataType, c.Type.MaxLength }).ToList();
                var parentColumnsInfo = parentColumns.Select(c => new { c.Type.DataType, c.Type.MaxLength }).ToList();

                var columnsEqual = childColumnsInfo.SequenceEqual(parentColumnsInfo);
                if (columnsEqual)
                    continue;

                var fkName = foreignKey.ChildKey.Name?.ToString() ?? string.Empty;

                var builder = new StringBuilder("A foreign key");
                if (!fkName.IsNullOrEmpty())
                {
                    builder.Append(" '")
                        .Append(fkName)
                        .Append("'");
                }

                builder.Append(" from ")
                    .Append(foreignKey.ChildKey.Table.Name)
                    .Append(" to ")
                    .Append(foreignKey.ParentKey.Table.Name)
                    .Append(" contains mismatching column types. These should be the same in order to ensure that foreign keys can always hold the same information as the target key.");

                var ruleMessage = new RuleMessage(RuleTitle, Level, builder.ToString());
                result.Add(ruleMessage);
            }

            return result;
        }

        private const string RuleTitle = "Foreign key relationships contain mismatching types.";
    }
}
