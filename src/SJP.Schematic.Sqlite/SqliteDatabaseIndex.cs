using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDatabaseTableIndex : SqliteDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public SqliteDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
            : base(table, name, isUnique, columns, includedColumns)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public abstract class SqliteDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected SqliteDatabaseIndex(T parent, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
        {
            if (name == null || name.LocalName == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (includedColumns != null && includedColumns.AnyNull())
                throw new ArgumentNullException(nameof(includedColumns));

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Name = name.LocalName;
            IsUnique = isUnique;
            Columns = columns.ToList();
            IncludedColumns = includedColumns != null ? includedColumns.ToList() : Enumerable.Empty<IDatabaseColumn>();
        }

        public T Parent { get; }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IEnumerable<IDatabaseIndexColumn> Columns { get; }

        public IEnumerable<IDatabaseColumn> IncludedColumns { get; }

        public bool IsEnabled { get; } = true;
    }
}
