using System;

namespace SJP.Schema.Modelled.Reflection
{
    public sealed class ExcludeAttribute : AutoSchemaAttribute
    {
        public ExcludeAttribute()
            : base(new[] { Dialect.All }) { }

        public ExcludeAttribute(params Type[] dialects)
            : base(dialects) { }
    }
}
