using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    public interface IDatabaseEntity
    {
        Identifier Name { get; }
    }
}
