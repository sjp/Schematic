using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
{
    public class RedundantIndexesRule : Rule
    {
        public RedundantIndexesRule(RuleLevel level)
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

            var indexes = table.Indexes.ToList();
            foreach (var index in indexes)
            {
                var indexColumnList = index.Columns.SelectMany(c => c.DependentColumns).Select(c => c.Name);

                var otherIndexes = indexes.Where(i => i.Name != index.Name);
                foreach (var otherIndex in otherIndexes)
                {
                    var otherIndexColumnList = otherIndex.Columns.SelectMany(c => c.DependentColumns).Select(c => c.Name);
                }

                //TODO add code to check whether a column list is a prefix of another
            }

            return result;
        }

        protected static bool ColumnsHaveIndex(IEnumerable<IDatabaseColumn> columns, IEnumerable<IDatabaseIndexColumn> indexColumns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (indexColumns == null)
                throw new ArgumentNullException(nameof(indexColumns));

            var columnList = columns.ToList();
            var dependentColumns = indexColumns.SelectMany(ic => ic.DependentColumns).ToList();

            // can only check for regular indexes, not functional ones (functions may be composed of multiple columns)
            if (columnList.Count != dependentColumns.Count)
                return false;

            var columnNames = columnList.Select(c => c.Name).ToList();
            var indexColumnNames = dependentColumns.Select(ic => ic.Name).ToList();

            var comparer = IdentifierComparer.OrdinalIgnoreCase;
            return columnNames.SequenceEqual(indexColumnNames, comparer);
        }

        protected static string BuildMessage(Identifier tableName, Identifier foreignKeyName, IEnumerable<IDatabaseColumn> columns)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null || columns.Empty())
                throw new ArgumentNullException(nameof(columns));

            var builder = new StringBuilder("The table ")
                .Append(tableName.ToString())
                .Append(" has a foreign key ");

            if (foreignKeyName != null)
            {
                builder.Append("'")
                    .Append(foreignKeyName.LocalName)
                    .Append("' ");
            }

            builder.Append("which is missing an index on the column");

            // plural check
            if (columns.Skip(1).Any())
                builder.Append("s");

            var columnNames = columns.Select(c => c.Name.ToString()).Join(", ");
            builder.Append(" ")
                .Append(columnNames);

            return builder.ToString();
        }

        private const string RuleTitle = "Indexes missing on foreign key.";
    }
}
