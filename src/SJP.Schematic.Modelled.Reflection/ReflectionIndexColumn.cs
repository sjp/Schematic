using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionIndexColumn : IDatabaseIndexColumn
    {
        public ReflectionIndexColumn(IModelledSqlExpression expression, IReadOnlyList<IDatabaseColumn> columns, IndexColumnOrder order)
        {
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
