using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer
{
    public abstract class SqlServerDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected SqlServerDatabaseIndex(T parent, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns, bool isEnabled)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (includedColumns != null && includedColumns.AnyNull())
                throw new ArgumentNullException(nameof(includedColumns));

            if (includedColumns == null)
                includedColumns = Array.Empty<IDatabaseColumn>();

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Name = name.LocalName;
            IsUnique = isUnique;
            Columns = columns;
            IncludedColumns = includedColumns;
            IsEnabled = isEnabled;
        }

        public T Parent { get; }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

        public bool IsEnabled { get; }
    }
}
