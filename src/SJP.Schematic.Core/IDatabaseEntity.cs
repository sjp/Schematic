using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IDatabaseEntity
    {
        Identifier Name { get; }
    }
}
