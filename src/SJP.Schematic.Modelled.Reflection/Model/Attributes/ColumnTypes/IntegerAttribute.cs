using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class IntegerAttribute : DeclaredTypeAttribute
        {
            public IntegerAttribute()
            : base(DataType.Integer, new[] { Dialect.All })
            {
            }

            public IntegerAttribute(params Type[] dialects)
                : base(DataType.Integer, dialects)
            {
            }
        }
    }
}
