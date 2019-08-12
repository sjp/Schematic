using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql
{
    // TODO: remove this when the dependent columns can be parsed out
    public class PostgreSqlDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public PostgreSqlDatabaseIndexColumn(string expression, IndexColumnOrder order)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            Expression = expression;
            Order = order;
            DependentColumns = Array.Empty<IDatabaseColumn>();
        }

        public PostgreSqlDatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            Expression = expression;
            DependentColumns = new[] { column };
            Order = order;
        }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string Expression { get; }
    }
}
