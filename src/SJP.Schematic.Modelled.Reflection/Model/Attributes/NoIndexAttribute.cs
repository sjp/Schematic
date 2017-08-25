using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class NoIndexAttribute : ModelledSchemaAttribute
    {
        public NoIndexAttribute()
            : base(new[] { Dialect.All }) { }

        public NoIndexAttribute(params Type[] dialects)
            : base(dialects) { }
    }
}
