using System;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public sealed class NoIndexAttribute : ModelledSchemaAttribute
    {
        public NoIndexAttribute()
            : base(new[] { Dialect.All }) { }

        public NoIndexAttribute(params Type[] dialects)
            : base(dialects) { }
    }
}
