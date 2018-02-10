using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis
{
    public class ForeignKeyIndexRule : Rule
    {
        protected ForeignKeyIndexRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<RuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<RuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<RuleMessage>();

            var indexes = table.Indexes.ToList();
            var foreignKeys = table.ParentKeys.Select(fk => fk.ChildKey).ToList();

            foreach (var foreignKey in foreignKeys)
            {
                var columns = foreignKey.Columns;

                var isIndexedKey = indexes.Select(i => i.Columns).Any(ic => ColumnsHaveIndex(columns, ic));
                if (!isIndexedKey)
                {
                    var messageText = BuildMessage(table.Name, foreignKey.Name, foreignKey.Columns);
                    var message = new RuleMessage(RuleTitle, Level, "test"); // TODO fix message
                    result.Add(message);
                }
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

            var columnNames = columns.Select(c => c.ToString()).Join(", ");
            builder.Append(" ")
                .Append(columnNames);

            return builder.ToString();
        }

        private const string RuleTitle = "Indexes missing on foreign key";
    }
}
