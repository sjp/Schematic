using System;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
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
