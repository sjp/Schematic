using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class FloatAttribute : DeclaredTypeAttribute
        {
            public FloatAttribute(int precision)
            : base(DataType.Float, precision, UnknownLength, new[] { Dialect.All })
            {
            }

            public FloatAttribute(int precision, params Type[] dialects)
                : base(DataType.Float, precision, UnknownLength, dialects)
            {
            }
        }
    }
}
