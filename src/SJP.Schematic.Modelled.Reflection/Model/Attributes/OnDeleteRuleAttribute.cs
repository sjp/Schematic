using System;
using System.Data;
using EnumsNET;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class OnDeleteRuleAttribute : ModelledSchemaAttribute
    {
        public OnDeleteRuleAttribute(Rule rule)
            : base(new[] { Dialect.All })
        {
            if (!rule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(rule));

            Rule = rule;
        }

        public OnDeleteRuleAttribute(Rule rule, params Type[] dialects)
            : base(dialects)
        {
            if (!rule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(rule));

            Rule = rule;
        }

        public Rule Rule { get; }
    }
}
