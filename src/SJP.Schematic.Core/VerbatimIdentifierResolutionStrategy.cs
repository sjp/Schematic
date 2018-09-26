using System;
using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public class VerbatimIdentifierResolutionStrategy : IIdentifierResolutionStrategy
    {
        public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            return new[] { identifier };
        }
    }
}
