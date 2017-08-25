using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class AutoIncrementAttribute : ModelledSchemaAttribute
    {
        public AutoIncrementAttribute()
            : base(new[] { Dialect.All })
        {
        }

        public AutoIncrementAttribute(params Type[] dialects)
            : base(dialects)
        {
        }
    }
}
