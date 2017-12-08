using System;

namespace SJP.Schematic.Core
{
    public class ColumnTypeMetadata
    {
        public Identifier TypeName { get; set; }

        public DataType DataType { get; set; }

        public bool IsFixedLength { get; set; }

        public int MaxLength { get; set; }

        public Type ClrType { get; set; }

        public Identifier Collation { get; set; }

        public NumericPrecision NumericPrecision { get; set; }
    }
}
