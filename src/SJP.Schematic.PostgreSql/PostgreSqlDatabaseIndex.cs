using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseViewIndex : PostgreSqlDatabaseIndex<IRelationalDatabaseView>, IDatabaseViewIndex
    {
        public PostgreSqlDatabaseViewIndex(IRelationalDatabaseView view, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns)
            : base(view, name, isUnique, columns)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }

    public class PostgreSqlDatabaseTableIndex : PostgreSqlDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public PostgreSqlDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns)
            : base(table, name, isUnique, columns)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public abstract class PostgreSqlDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected PostgreSqlDatabaseIndex(T parent, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns)
        {
            if (name == null)
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

    public class PostgreSqlDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public PostgreSqlDatabaseIndexColumn(string expression, IndexColumnOrder order)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            _expression = expression;
            Order = order;
        }

        public PostgreSqlDatabaseIndexColumn(IDatabaseColumn column, IndexColumnOrder order)
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
            return _expression ?? DependentColumns
                .Select(c => dialect.QuoteName(c.Name))
                .Single();
        }

        private readonly string _expression;
    }
}
