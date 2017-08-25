using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class BooleanAttribute : DeclaredTypeAttribute
        {
            public BooleanAttribute()
            : base(DataType.Boolean, 1, true, new[] { Dialect.All })
            {
            }

            public BooleanAttribute(params Type[] dialects)
                : base(DataType.Boolean, 1, true, dialects)
            {
            }
        }
    }
}
