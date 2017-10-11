using System;
using System.Data;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class OnUpdateRuleAttribute : ModelledSchemaAttribute
    {
        public OnUpdateRuleAttribute(Rule rule)
            : base(new[] { Dialect.All })
        {
            Rule = rule;
        }

        public OnUpdateRuleAttribute(Rule rule, params Type[] dialects)
            : base(dialects)
        {
            Rule = rule;
        }

        public Rule Rule { get; }
    }
}
