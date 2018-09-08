using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql
{
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
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));

            return _expression ?? DependentColumns
                .Select(c => dialect.QuoteName(c.Name))
                .Single();
        }

        private readonly string _expression;
    }
}
