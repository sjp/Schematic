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
        public sealed class IntervalAttribute : DeclaredTypeAttribute
        {
            public IntervalAttribute(int secondPrecision)
            : base(DataType.Interval, secondPrecision, false, new[] { Dialect.All })
            {
            }

            public IntervalAttribute(int secondPrecision, params Type[] dialects)
                : base(DataType.Interval, secondPrecision, false, dialects)
            {
            }
        }
    }
}
