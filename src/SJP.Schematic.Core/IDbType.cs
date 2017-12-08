using System;

namespace SJP.Schematic.Core
{
    public interface IDbType
    {
        Identifier TypeName { get; }

        DataType DataType { get; }

        string Definition { get; }

        bool IsFixedLength { get; }

        int MaxLength { get; }

        Type ClrType { get; }

        NumericPrecision NumericPrecision { get; }

        Identifier Collation { get; }
    }
}
