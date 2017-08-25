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
        public sealed class UnicodeAttribute : DeclaredTypeAttribute
        {
            public UnicodeAttribute(int length, bool isFixedLength = false)
            : base(DataType.Unicode, length, isFixedLength, new[] { Dialect.All })
            {
            }

            public UnicodeAttribute(int length, bool isFixedLength = false, params Type[] dialects)
                : base(DataType.Unicode, length, isFixedLength, dialects)
            {
            }
        }
    }
}
