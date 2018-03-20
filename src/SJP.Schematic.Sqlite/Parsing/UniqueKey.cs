using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class UniqueKey
    {
        public UniqueKey(string constraintName, string columnName)
        {
            Name = constraintName;
            Columns = new IndexedColumn(columnName).ToEnumerable();
        }

        public UniqueKey(string constraintName, IEnumerable<IndexedColumn> columns)
        {
            if (columns == null || columns.Empty())
                throw new ArgumentNullException(nameof(columns));

            Name = constraintName;
            Columns = columns;
        }

        public string Name { get; }

        public IEnumerable<IndexedColumn> Columns { get; }
    }
}
