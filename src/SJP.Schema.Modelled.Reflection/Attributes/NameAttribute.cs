using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public sealed class NameAttribute : AutoSchemaAttribute
    {
        public NameAttribute(string name)
            : base(new[] { Dialect.All })
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public NameAttribute(string name, params Type[] dialects)
            : base(dialects)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public string Name { get; }
    }

}
