using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public class IndexColumn : IModelledIndexColumn
    {
        public IndexColumn(ExpressionColumn column, IndexColumnOrder order = IndexColumnOrder.Ascending)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            Expression = column.Expression;
            Order = order;
        }

        public IndexColumn(ISqlExpression expression, IndexColumnOrder order = IndexColumnOrder.Ascending)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Order = order;
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
