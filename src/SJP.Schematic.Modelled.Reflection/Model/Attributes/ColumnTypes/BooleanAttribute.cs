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
        public sealed class BooleanAttribute : DeclaredTypeAttribute
        {
            public BooleanAttribute()
            : base(DataType.Boolean, 1, true, new[] { Dialect.All })
            {
            }

            public BooleanAttribute(params Type[] dialects)
                : base(DataType.Boolean, 1, true, dialects)
            {
            }
        }
    }
}
