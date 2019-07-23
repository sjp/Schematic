using System;
using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDependencyProvider : IDependencyProvider
    {
        public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));

            return Array.Empty<Identifier>();
        }
    }
}
