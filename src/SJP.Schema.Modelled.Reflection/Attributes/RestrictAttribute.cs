using System;

namespace SJP.Schema.Modelled.Reflection
{
    public sealed class RestrictAttribute : AutoSchemaAttribute
    {
        public RestrictAttribute(params Type[] dialects)
           : base(dialects) { }
    }
}
