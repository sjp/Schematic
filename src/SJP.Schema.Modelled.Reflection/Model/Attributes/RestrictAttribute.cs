using System;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public sealed class RestrictAttribute : ModelledSchemaAttribute
    {
        public RestrictAttribute(params Type[] dialects)
           : base(dialects) { }
    }
}
