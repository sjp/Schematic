using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public class ExpressionColumn
    {
        public ExpressionColumn(ISqlExpression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public static implicit operator ExpressionColumn(ModelledColumn input)
        {
            return new ExpressionColumn(Sql.Identity(input.Property.Name));
        }

        public static implicit operator ExpressionColumn(ModelledSqlExpression expression)
        {
            return new ExpressionColumn(expression);
        }

        public IndexColumnOrder Order { get; }

        public ISqlExpression Expression { get; }
    }
}
