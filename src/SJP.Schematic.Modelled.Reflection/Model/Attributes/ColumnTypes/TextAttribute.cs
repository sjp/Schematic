using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class TextAttribute : DeclaredTypeAttribute
        {
            public TextAttribute()
            : base(DataType.Text, new[] { Dialect.All })
            {
            }

            public TextAttribute(params Type[] dialects)
                : base(DataType.Text, dialects)
            {
            }
        }
    }
}
