using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        public sealed class DateAttribute : DeclaredTypeAttribute
        {
            public DateAttribute()
            : base(DataType.Date, new[] { Dialect.All })
            {
            }

            public DateAttribute(params Type[] dialects)
                : base(DataType.Date, dialects)
            {
            }
        }
    }
}
