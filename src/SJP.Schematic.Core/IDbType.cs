using System;

namespace SJP.Schematic.Core
{
    public interface IDbType
    {
        DataType Type { get; }

        bool IsFixedLength { get; }

        int Length { get; }

        Type ClrType { get; }
    }
}
