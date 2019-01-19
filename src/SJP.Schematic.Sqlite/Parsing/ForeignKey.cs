using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class ForeignKey
    {
        public ForeignKey(Option<string> constraintName, string columnName, Identifier parentTable, IReadOnlyCollection<string> parentColumnNames)
            : this(constraintName, new[] { columnName }, parentTable, parentColumnNames)
        {
        }

        public ForeignKey(Option<string> constraintName, IReadOnlyCollection<string> columnNames, Identifier parentTable, IReadOnlyCollection<string> parentColumnNames)
        {
            if (columnNames == null || columnNames.Empty() || columnNames.Any(c => c.IsNullOrWhiteSpace()))
                throw new ArgumentNullException(nameof(columnNames));
            if (parentColumnNames == null || parentColumnNames.Empty() || parentColumnNames.Any(c => c.IsNullOrWhiteSpace()))
                throw new ArgumentNullException(nameof(parentColumnNames));
            if (columnNames.Count != parentColumnNames.Count)
                throw new ArgumentException($"The number of source columns ({ columnNames.Count }) does not match the number of target columns ({ parentColumnNames.Count }).", nameof(parentColumnNames));

            ParentTable = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
            Name = constraintName;
            Columns = columnNames;
            ParentColumns = parentColumnNames;
        }

        public Option<string> Name { get; }

        public IEnumerable<string> Columns { get; }

        public Identifier ParentTable { get; }

        public IEnumerable<string> ParentColumns { get; }
    }
}
