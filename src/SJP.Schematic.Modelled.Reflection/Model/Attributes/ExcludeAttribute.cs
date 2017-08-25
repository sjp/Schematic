using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ExcludeAttribute : ModelledSchemaAttribute
    {
        public ExcludeAttribute()
            : base(new[] { Dialect.All }) { }

        public ExcludeAttribute(params Type[] dialects)
            : base(dialects) { }
    }
}
