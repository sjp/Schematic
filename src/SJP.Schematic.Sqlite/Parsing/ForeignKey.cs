using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using System.Linq;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class ForeignKey
    {
        public ForeignKey(string constraintName, string columnName, string parentTableName, IEnumerable<string> parentColumnNames)
            : this(constraintName, columnName.ToEnumerable(), parentTableName, parentColumnNames)
        {
        }

        public ForeignKey(string constraintName, IEnumerable<string> columnNames, string parentTableName, IEnumerable<string> parentColumnNames)
        {
            if (columnNames == null || columnNames.Empty() || columnNames.Any(c => c.IsNullOrWhiteSpace()))
                throw new ArgumentNullException(nameof(columnNames));
            if (parentTableName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(parentTableName));
            if (parentColumnNames == null || parentColumnNames.Empty() || parentColumnNames.Any(c => c.IsNullOrWhiteSpace()))
                throw new ArgumentNullException(nameof(parentColumnNames));
            if (columnNames.Count() != parentColumnNames.Count())
                throw new ArgumentException($"The number of source columns ({ columnNames.Count() }) does not match the number of target columns ({ parentColumnNames.Count() }).", nameof(parentColumnNames));

            Name = constraintName;
            Columns = columnNames;
            ParentTable = parentTableName;
            ParentColumns = parentColumnNames;
        }

        public string Name { get; }

        public IEnumerable<string> Columns { get; }

        public string ParentTable { get; }

        public IEnumerable<string> ParentColumns { get; }
    }
}
