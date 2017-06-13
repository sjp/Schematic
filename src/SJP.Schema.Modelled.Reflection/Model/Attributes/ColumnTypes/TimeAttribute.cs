using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        public sealed class TimeAttribute : DeclaredTypeAttribute
        {
            public TimeAttribute()
            : base(DataType.Time, new[] { Dialect.All })
            {
            }

            public TimeAttribute(params Type[] dialects)
                : base(DataType.Time, dialects)
            {
            }
        }
    }
}
