using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class TimeAttribute : DeclaredTypeAttribute
        {
            public TimeAttribute()
            : base(DataType.Time, new[] { Dialect.All })
            {
            }

            public TimeAttribute(params Type[] dialects)
                : base(DataType.Time, dialects)
            {
            }
        }
    }
}
