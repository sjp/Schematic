using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class DateTimeAttribute : DeclaredTypeAttribute
        {
            public DateTimeAttribute()
            : base(DataType.DateTime, new[] { Dialect.All })
            {
            }

            public DateTimeAttribute(params Type[] dialects)
                : base(DataType.DateTime, dialects)
            {
            }
        }
    }
}
