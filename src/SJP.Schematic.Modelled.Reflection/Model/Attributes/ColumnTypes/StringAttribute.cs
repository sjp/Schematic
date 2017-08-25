using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        public sealed class StringAttribute : DeclaredTypeAttribute
        {
            public StringAttribute(int length, bool isFixedLength = false)
            : base(DataType.String, length, isFixedLength, new[] { Dialect.All })
            {
            }

            public StringAttribute(int length, bool isFixedLength = false, params Type[] dialects)
                : base(DataType.String, length, isFixedLength, dialects)
            {
            }
        }
    }
}
