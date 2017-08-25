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
        public sealed class UnicodeTextAttribute : DeclaredTypeAttribute
        {
            public UnicodeTextAttribute()
            : base(DataType.UnicodeText, new[] { Dialect.All })
            {
            }

            public UnicodeTextAttribute(params Type[] dialects)
                : base(DataType.UnicodeText, dialects)
            {
            }
        }
    }
}
