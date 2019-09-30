using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class ExpressionColumn
    {
        public ExpressionColumn(IModelledSqlExpression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public static implicit operator ExpressionColumn(ModelledColumn input)
        {
            return new ExpressionColumn(Sql.Identity(input.Property!.Name));
        }

        public static implicit operator ExpressionColumn(ModelledSqlExpression expression)
        {
            return new ExpressionColumn(expression);
        }

        public IndexColumnOrder Order { get; }

        public IModelledSqlExpression Expression { get; }
    }
}
