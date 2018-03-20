using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using System.Linq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class ForeignKey
    {
        public ForeignKey(string constraintName, string columnName, Identifier parentTable, IEnumerable<string> parentColumnNames)
            : this(constraintName, columnName.ToEnumerable(), parentTable, parentColumnNames)
        {
        }

        public ForeignKey(string constraintName, IEnumerable<string> columnNames, Identifier parentTable, IEnumerable<string> parentColumnNames)
        {
            if (columnNames == null || columnNames.Empty() || columnNames.Any(c => c.IsNullOrWhiteSpace()))
                throw new ArgumentNullException(nameof(columnNames));
            if (parentColumnNames == null || parentColumnNames.Empty() || parentColumnNames.Any(c => c.IsNullOrWhiteSpace()))
                throw new ArgumentNullException(nameof(parentColumnNames));
            if (columnNames.Count() != parentColumnNames.Count())
                throw new ArgumentException($"The number of source columns ({ columnNames.Count() }) does not match the number of target columns ({ parentColumnNames.Count() }).", nameof(parentColumnNames));

            ParentTable = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
            Name = constraintName;
            Columns = columnNames;
            ParentColumns = parentColumnNames;
        }

        public string Name { get; }

        public IEnumerable<string> Columns { get; }

        public Identifier ParentTable { get; }

        public IEnumerable<string> ParentColumns { get; }
    }
}
