using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class LargeBinaryAttribute : DeclaredTypeAttribute
        {
            public LargeBinaryAttribute()
            : base(DataType.LargeBinary, new[] { Dialect.All })
            {
            }

            public LargeBinaryAttribute(params Type[] dialects)
                : base(DataType.LargeBinary, dialects)
            {
            }
        }
    }
}
