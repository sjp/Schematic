using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public class ComputedColumn : IModelledColumn
    {
        public ComputedColumn(string expression, object param)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            Expression = new ModelledSqlExpression(expression, param);
        }

        public virtual Type DeclaredDbType { get; }

        public ISqlExpression Expression { get; }

        public virtual bool IsComputed { get; } = true;

        public virtual bool IsNullable { get; } = true;

        public PropertyInfo Property { get; internal set; }
    }
}
