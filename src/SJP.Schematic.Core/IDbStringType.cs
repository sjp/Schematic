using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDbStringType : IDbType
    {
        bool IsUnicode { get; }

        string Collation { get; }
    }
}
