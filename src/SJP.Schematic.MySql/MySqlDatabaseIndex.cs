using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabaseViewIndex : MySqlDatabaseIndex<IRelationalDatabaseView>, IDatabaseViewIndex
    {
        public MySqlDatabaseViewIndex(IRelationalDatabaseView view, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns)
            : base(view, name, isUnique, columns)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }

    public class MySqlDatabaseTableIndex : MySqlDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public MySqlDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns)
            : base(table, name, isUnique, columns)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public abstract class MySqlDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected MySqlDatabaseIndex(T parent, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns)
        {
            if (name == null || name.LocalName == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Name = name.LocalName;
            IsUnique = isUnique;
            Columns = columns;
        }

        public T Parent { get; }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IEnumerable<IDatabaseIndexColumn> Columns { get; }

        public IEnumerable<IDatabaseColumn> IncludedColumns { get; } = Enumerable.Empty<IDatabaseColumn>();

        public bool IsEnabled { get; } = true;
    }

    public class MySqlDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public MySqlDatabaseIndexColumn(IDatabaseColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            DependentColumns = new List<IDatabaseColumn> { column }.AsReadOnly();
        }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string GetExpression(IDatabaseDialect dialect)
        {
            return DependentColumns
                .Select(c => dialect.QuoteName(c.Name))
                .Single();
        }
    }
}
