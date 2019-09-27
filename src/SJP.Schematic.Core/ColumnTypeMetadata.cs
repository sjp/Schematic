using System;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public class ColumnTypeMetadata
    {
        public Identifier? TypeName { get; set; }

        public DataType DataType { get; set; }

        public bool IsFixedLength { get; set; }

        public int MaxLength { get; set; }

        public Type? ClrType { get; set; }

        public Option<Identifier> Collation { get; set; }

        public Option<INumericPrecision> NumericPrecision { get; set; }
    }
}
