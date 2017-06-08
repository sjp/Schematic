using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{

    public sealed class DefaultAttribute : AutoSchemaAttribute
    {
        public DefaultAttribute(string defaultValue)
            : base(new[] { Dialect.All })
        {
            if (defaultValue.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(defaultValue));

            DefaultValue = defaultValue;
        }

        public DefaultAttribute(string defaultValue, params Type[] dialects)
            : base(dialects)
        {
            if (defaultValue.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(defaultValue));

            DefaultValue = defaultValue;
        }

        // maybe this should be an expression?
        // thinking at the moment not to but to parse it as one via the dialect?
        // would give us access to expressions that can be parsed correctly
        // this would be consistent with how other attributes and properties are declared
        public string DefaultValue { get; }
    }

}
