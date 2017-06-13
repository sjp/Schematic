using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        public sealed class IntegerAttribute : DeclaredTypeAttribute
        {
            public IntegerAttribute()
            : base(DataType.Integer, new[] { Dialect.All })
            {
            }

            public IntegerAttribute(params Type[] dialects)
                : base(DataType.Integer, dialects)
            {
            }
        }
    }
}
