using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class UnicodeAttribute : DeclaredTypeAttribute
        {
            public UnicodeAttribute(int length, bool isFixedLength = false)
            : base(DataType.Unicode, length, isFixedLength, new[] { Dialect.All })
            {
            }

            public UnicodeAttribute(int length, bool isFixedLength = false, params Type[] dialects)
                : base(DataType.Unicode, length, isFixedLength, dialects)
            {
            }
        }
    }
}
