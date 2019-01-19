using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class UniqueKey
    {
        public UniqueKey(Option<string> constraintName, string columnName)
        {
            Name = constraintName;
            Columns = new IndexedColumn(columnName).ToEnumerable();
        }

        public UniqueKey(Option<string> constraintName, IEnumerable<IndexedColumn> columns)
        {
            if (columns == null || columns.Empty())
                throw new ArgumentNullException(nameof(columns));

            Name = constraintName;
            Columns = columns;
        }

        public Option<string> Name { get; }

        public IEnumerable<IndexedColumn> Columns { get; }
    }
}
