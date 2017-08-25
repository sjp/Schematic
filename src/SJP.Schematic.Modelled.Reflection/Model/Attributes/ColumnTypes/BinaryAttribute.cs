using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class BinaryAttribute : DeclaredTypeAttribute
        {
            public BinaryAttribute(int length, bool isFixedLength = false)
            : base(DataType.Binary, length, isFixedLength, new[] { Dialect.All })
            {
            }

            public BinaryAttribute(int length, bool isFixedLength = false, params Type[] dialects)
                : base(DataType.Binary, length, isFixedLength, dialects)
            {
            }
        }
    }
}
