using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SJP.Schema.Core;

namespace SJP.Schema.SQLite
{
    public class SQLiteDatabaseTableIndex : SQLiteDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public SQLiteDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
            : base(table, name, isUnique, columns, includedColumns)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            Table = table;
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public abstract class SQLiteDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected SQLiteDatabaseIndex(T parent, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (includedColumns != null && includedColumns.AnyNull())
                throw new ArgumentNullException(nameof(includedColumns));

            Parent = parent;
            Name = name;
            IsUnique = isUnique;
            Columns = columns.ToImmutableList();
            IncludedColumns = includedColumns.ToImmutableList() ?? Enumerable.Empty<IDatabaseColumn>();
        }

        public T Parent { get; }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IEnumerable<IDatabaseIndexColumn> Columns { get; }

        public IEnumerable<IDatabaseColumn> IncludedColumns { get; }
    }
}
