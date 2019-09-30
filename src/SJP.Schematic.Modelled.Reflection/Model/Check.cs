using System;
using System.Reflection;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class Check : IModelledCheckConstraint
    {
        public Check(string expression, object param)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            Expression = new ModelledSqlExpression(expression, param);
        }

        public IModelledSqlExpression Expression { get; }

        public PropertyInfo? Property { get; set; }
    }
}
