using System;
using System.Collections.Generic;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
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
