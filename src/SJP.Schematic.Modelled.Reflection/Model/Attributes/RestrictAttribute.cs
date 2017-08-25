using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class RestrictAttribute : ModelledSchemaAttribute
    {
        public RestrictAttribute(params Type[] dialects)
           : base(dialects) { }
    }
}
