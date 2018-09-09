using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IIdentifierResolutionStrategy
    {
        IEnumerable<Identifier> GetResolutionOrder(Identifier identifier);
    }
}
