using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class NumericAttribute : DeclaredTypeAttribute
        {
            public NumericAttribute(int precision, int scale = 0)
            : base(DataType.Numeric, precision, scale, new[] { Dialect.All })
            {
            }

            public NumericAttribute(int precision, int scale = 0, params Type[] dialects)
                : base(DataType.Numeric, precision, scale, dialects)
            {
            }
        }
    }
}
