using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class BigIntegerAttribute : DeclaredTypeAttribute
        {
            public BigIntegerAttribute()
            : base(DataType.BigInteger, new[] { Dialect.All })
            {
            }

            public BigIntegerAttribute(params Type[] dialects)
                : base(DataType.BigInteger, dialects)
            {
            }
        }
    }
}
