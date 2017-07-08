using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionIndexColumn : IDatabaseIndexColumn
    {
        public ReflectionIndexColumn(ISqlExpression expression, IReadOnlyList<IDatabaseColumn> columns, IndexColumnOrder order)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            DependentColumns = columns ?? throw new ArgumentNullException(nameof(columns));
            Order = order;
        }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string GetExpression(IDatabaseDialect dialect) => Expression.ToSql(dialect);

        private ISqlExpression Expression { get; }
    }
}
