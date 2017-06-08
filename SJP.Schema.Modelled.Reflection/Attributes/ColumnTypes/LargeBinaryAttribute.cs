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
        public sealed class LargeBinaryAttribute : DeclaredTypeAttribute
        {
            public LargeBinaryAttribute()
            : base(DataType.LargeBinary, new[] { Dialect.All })
            {
            }

            public LargeBinaryAttribute(params Type[] dialects)
                : base(DataType.LargeBinary, dialects)
            {
            }
        }
    }
}
