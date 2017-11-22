using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class IndexColumn : IModelledIndexColumn
    {
        public IndexColumn(ExpressionColumn column, IndexColumnOrder order = IndexColumnOrder.Ascending)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            Expression = column.Expression;
            Order = order;
        }

        public IndexColumn(IModelledSqlExpression expression, IndexColumnOrder order = IndexColumnOrder.Ascending)
        {
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Order = order;
        }

        public static implicit operator IndexColumn(ModelledColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            ExpressionColumn expression = column;
            return new IndexColumn(expression);
        }

        public static implicit operator IndexColumn(ExpressionColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new IndexColumn(column);
        }

        public static implicit operator IndexColumn(ModelledSqlExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return new IndexColumn(expression: expression);
        }

        public IndexColumnOrder Order { get; }

        public IModelledSqlExpression Expression { get; }
    }
}
