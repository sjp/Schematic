using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
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
