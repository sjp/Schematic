using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class UnicodeTextAttribute : DeclaredTypeAttribute
        {
            public UnicodeTextAttribute()
            : base(DataType.UnicodeText, new[] { Dialect.All })
            {
            }

            public UnicodeTextAttribute(params Type[] dialects)
                : base(DataType.UnicodeText, dialects)
            {
            }
        }
    }
}
