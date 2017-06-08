using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseRelationalKey
    {
        IDatabaseKey ParentKey { get; }

        IDatabaseKey ChildKey { get; }
    }
}
