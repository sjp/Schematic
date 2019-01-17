using System;
using LanguageExt;

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

        Option<INumericPrecision> NumericPrecision { get; }

        Option<Identifier> Collation { get; }
    }
}
