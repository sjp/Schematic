using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public sealed class RestrictAttribute : ModelledSchemaAttribute
    {
        public RestrictAttribute(params Type[] dialects)
           : base(dialects) { }
    }
}
