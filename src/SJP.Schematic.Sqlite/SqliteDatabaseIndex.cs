using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDatabaseIndex : IDatabaseIndex
    {
        public SqliteDatabaseIndex(Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (includedColumns != null && includedColumns.AnyNull())
                throw new ArgumentNullException(nameof(includedColumns));

            if (includedColumns == null)
                includedColumns = Array.Empty<IDatabaseColumn>();

            Name = name.LocalName;
            IsUnique = isUnique;
            Columns = columns.ToList();
            IncludedColumns = includedColumns.ToList();
        }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

        public bool IsEnabled { get; } = true;
    }
}
