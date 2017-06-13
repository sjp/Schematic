using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
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

        public ISqlExpression Expression { get; }

        public PropertyInfo Property { get; set; }
    }
}
