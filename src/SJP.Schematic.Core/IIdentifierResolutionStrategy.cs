using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Declares how database identifiers should be resolved for scenarios like case normalization.
    /// </summary>
    public interface IIdentifierResolutionStrategy
    {
        /// <summary>
        /// Constructs the set of identifiers (in order) that should be used to query the database for an object.
        /// </summary>
        /// <param name="identifier">A database identifier.</param>
        /// <returns>A set of identifiers to query with.</returns>
        IEnumerable<Identifier> GetResolutionOrder(Identifier identifier);
    }
}
