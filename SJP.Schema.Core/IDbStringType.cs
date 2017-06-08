using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDbStringType : IDbType
    {
        bool IsUnicode { get; }

        string Collation { get; }
    }
}
