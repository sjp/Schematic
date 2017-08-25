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
        public sealed class DateTimeAttribute : DeclaredTypeAttribute
        {
            public DateTimeAttribute()
            : base(DataType.DateTime, new[] { Dialect.All })
            {
            }

            public DateTimeAttribute(params Type[] dialects)
                : base(DataType.DateTime, dialects)
            {
            }
        }
    }
}
