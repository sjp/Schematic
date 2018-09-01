using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public interface INameResolverStrategy
    {
        IEnumerable<Identifier> GetResolutionOrder(Identifier identifier);
    }
}
