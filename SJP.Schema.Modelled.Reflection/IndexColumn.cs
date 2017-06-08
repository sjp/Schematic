using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class IndexColumn : IModelledIndexColumn
    {
        public IndexColumn(ExpressionColumn column, bool isAscending = true)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            Expression = column.Expression;
            Order = isAscending ? IndexColumnOrder.Ascending : IndexColumnOrder.Descending;
        }

        public IndexColumn(ISqlExpression expression, bool isAscending = true)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Order = isAscending ? IndexColumnOrder.Ascending : IndexColumnOrder.Descending;
        }

        public static implicit operator IndexColumn(ModelledColumn column)
        {
            ExpressionColumn expression = column;
            return new IndexColumn(expression);
        }

        public static implicit operator IndexColumn(ExpressionColumn column)
        {
            return new IndexColumn(column);
        }

        public static implicit operator IndexColumn(ModelledSqlExpression expression)
        {
            return new IndexColumn(expression: expression);
        }

        public IndexColumnOrder Order { get; }

        public ISqlExpression Expression { get; }
    }
}
