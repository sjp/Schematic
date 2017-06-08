using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDbType
    {
        DataType Type { get; }

        bool IsFixedLength { get; }

        int Length { get; }

        Type ClrType { get; }
    }
}
