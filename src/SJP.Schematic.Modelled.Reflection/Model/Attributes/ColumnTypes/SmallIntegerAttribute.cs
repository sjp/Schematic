using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class SmallIntegerAttribute : DeclaredTypeAttribute
        {
            public SmallIntegerAttribute()
            : base(DataType.SmallInteger, new[] { Dialect.All })
            {
            }

            public SmallIntegerAttribute(params Type[] dialects)
                : base(DataType.SmallInteger, dialects)
            {
            }
        }
    }
}
