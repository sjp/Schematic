using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public sealed class AliasAttribute : ModelledSchemaAttribute
    {
        public AliasAttribute(string name)
            : base(new[] { Dialect.All })
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            Alias = name;
        }

        public AliasAttribute(string name, params Type[] dialects)
            : base(dialects)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            Alias = name;
        }

        public string Alias { get; }
    }

}
