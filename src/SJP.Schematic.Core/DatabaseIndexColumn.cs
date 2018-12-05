using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class DatabaseIndexColumn : IDatabaseIndexColumn
    {
        public DatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            Expression = expression;
            DependentColumns = new List<IDatabaseColumn> { column }.AsReadOnly();
            Order = order;
        }

        public DatabaseIndexColumn(string expression, IEnumerable<IDatabaseColumn> dependentColumns, IndexColumnOrder order)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (dependentColumns == null || dependentColumns.AnyNull())
                throw new ArgumentNullException(nameof(dependentColumns));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            Expression = expression;
            DependentColumns = dependentColumns.ToReadOnlyList();
            Order = order;
        }

        public string Expression { get; }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }
    }
}
