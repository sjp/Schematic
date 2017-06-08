using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseRelationalKeyAsync
    {
        IDatabaseKeyAsync ParentKey { get; }

        IDatabaseKeyAsync ChildKey { get; }
    }
}
