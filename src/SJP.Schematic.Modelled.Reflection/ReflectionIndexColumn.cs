using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionIndexColumn : IDatabaseIndexColumn
    {
        public ReflectionIndexColumn(IModelledSqlExpression expression, IReadOnlyList<IDatabaseColumn> columns, IndexColumnOrder order)
        {
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            DependentColumns = columns ?? throw new ArgumentNullException(nameof(columns));
            Order = order;
        }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string GetExpression(IDatabaseDialect dialect) => _expression.ToSql(dialect);

        private readonly IModelledSqlExpression _expression;
    }
}
