using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
{
    public class UniqueIndexWithNullableColumnsRule : Rule
    {
        protected UniqueIndexWithNullableColumnsRule(RuleLevel level)
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

            var uniqueIndexes = table.Indexes.Where(i => i.IsUnique).ToList();
            if (uniqueIndexes.Count == 0)
                return Enumerable.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var index in uniqueIndexes)
            {
                var nullableColumns = index.Columns
                    .SelectMany(c => c.DependentColumns)
                    .Where(c => c.IsNullable)
                    .ToList();

                if (nullableColumns.Count == 0)
                    continue;

                var messageText = BuildMessage(table.Name, index.Name, nullableColumns);
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected static string BuildMessage(Identifier tableName, Identifier indexName, IEnumerable<IDatabaseColumn> nullableColumns)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (nullableColumns == null || nullableColumns.Empty())
                throw new ArgumentNullException(nameof(nullableColumns));

            var builder = new StringBuilder("The table ")
                .Append(tableName.ToString())
                .Append(" has a unique index ");

            if (indexName != null)
            {
                builder.Append("'")
                    .Append(indexName.LocalName)
                    .Append("' ");
            }

            var pluralText = nullableColumns.Skip(1).Any()
                ? "which contains nullable columns: "
                : "which contains a nullable column: ";
            builder.Append(pluralText);

            var columnNames = nullableColumns.Select(c => c.Name.ToString()).Join(", ");
            builder.Append(columnNames);

            return builder.ToString();
        }

        private const string RuleTitle = "Unique index contains nullable columns.";
    }
}
