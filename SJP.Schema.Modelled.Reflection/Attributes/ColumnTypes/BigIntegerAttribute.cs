using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public static partial class ColumnType
    {
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
