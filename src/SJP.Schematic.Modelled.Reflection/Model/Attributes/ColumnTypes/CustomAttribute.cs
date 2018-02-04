using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class CustomAttribute : DeclaredTypeAttribute
        {
            public CustomAttribute()
            : base(DataType.Unknown)
            {
            }

            public CustomAttribute(params Type[] dialects)
                : base(DataType.Unknown, dialects)
            {
            }
        }
    }
}
