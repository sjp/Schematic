using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class PrimaryKey
    {
        public PrimaryKey(string constraintName, string columnName)
        {
            Name = constraintName;
            Columns = new IndexedColumn(columnName).ToEnumerable();
        }

        public PrimaryKey(string constraintName, IEnumerable<IndexedColumn> columns)
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
