using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class DateAttribute : DeclaredTypeAttribute
        {
            public DateAttribute()
            : base(DataType.Date, new[] { Dialect.All })
            {
            }

            public DateAttribute(params Type[] dialects)
                : base(DataType.Date, dialects)
            {
            }
        }
    }
}
