using System.Collections.Generic;

namespace SJP.Schematic.Core.Utilities
{
    public interface IIdentifierResolutionStrategy
    {
        IEnumerable<Identifier> GetResolutionOrder(Identifier identifier);
    }
}
