using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public interface INameResolverStrategy
    {
        IEnumerable<Identifier> GetResolutionOrder(Identifier identifier);
    }
}
