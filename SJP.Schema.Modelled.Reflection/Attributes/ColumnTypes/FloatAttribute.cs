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
        public sealed class FloatAttribute : DeclaredTypeAttribute
        {
            public FloatAttribute(int precision)
            : base(DataType.Float, precision, UnknownLength, new[] { Dialect.All })
            {
            }

            public FloatAttribute(int precision, params Type[] dialects)
                : base(DataType.Float, precision, UnknownLength, dialects)
            {
            }
        }
    }
}
