using System;

namespace SJP.Schema.Modelled.Reflection.Model
{
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
