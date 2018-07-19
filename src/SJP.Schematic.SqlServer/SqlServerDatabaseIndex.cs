using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseViewIndex : SqlServerDatabaseIndex<IRelationalDatabaseView>, IDatabaseViewIndex
    {
        public SqlServerDatabaseViewIndex(IRelationalDatabaseView view, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseViewColumn> includedColumns, bool isEnabled)
            : base(view, name, isUnique, columns, includedColumns, isEnabled)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }

    public class SqlServerDatabaseTableIndex : SqlServerDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public SqlServerDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseTableColumn> includedColumns, bool isEnabled)
            : base(table, name, isUnique, columns, includedColumns, isEnabled)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public abstract class SqlServerDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected SqlServerDatabaseIndex(T parent, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns, bool isEnabled)
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
            Columns = columns.ToList();
            IncludedColumns = includedColumns.ToList();
            IsEnabled = isEnabled;
        }

        public T Parent { get; }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

        public bool IsEnabled { get; }
    }

    public class SqlServerDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public SqlServerDatabaseIndexColumn(IDatabaseColumn column, IndexColumnOrder order)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            DependentColumns = new List<IDatabaseColumn> { column }.AsReadOnly();
            Order = order;
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
