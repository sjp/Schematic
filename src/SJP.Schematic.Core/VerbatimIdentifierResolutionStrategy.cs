using System;
using System.Collections.Generic;

namespace SJP.Schematic.Core;

/// <summary>
/// A simple resolution strategy that resolves to only the identifier itself.
/// </summary>
/// <seealso cref="IIdentifierResolutionStrategy" />
public class VerbatimIdentifierResolutionStrategy : IIdentifierResolutionStrategy
{
    /// <summary>
    /// Constructs the set of identifiers (in order) that should be used to query the database for an object.
    /// </summary>
    /// <param name="identifier">A database identifier.</param>
    /// <returns>A set of identifiers to query with.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null" />.</exception>
    public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return [identifier];
    }
}