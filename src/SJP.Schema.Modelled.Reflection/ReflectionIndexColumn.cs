using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionIndexColumn : IDatabaseIndexColumn
    {
        public ReflectionIndexColumn(ISqlExpression expression, IList<IDatabaseColumn> columns, IndexColumnOrder order)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            DependentColumns = columns.ToImmutableList();
            Order = order;
        }

        public IList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string GetExpression(IDatabaseDialect dialect) => Expression.ToSql(dialect);

        private ISqlExpression Expression { get; }
    }
}
