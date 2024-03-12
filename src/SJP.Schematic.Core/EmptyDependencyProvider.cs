using System;
using System.Collections.Generic;

namespace SJP.Schematic.Core;

/// <summary>
/// A dependency provider that always returns an empty set of dependencies regardless of the input.
/// </summary>
/// <seealso cref="IDependencyProvider" />
public sealed class EmptyDependencyProvider : IDependencyProvider
{
    /// <summary>
    /// Retrieves all dependencies for an expression.
    /// </summary>
    /// <param name="objectName">The name of an object defined by an expression (e.g. a computed column definition).</param>
    /// <param name="expression">A SQL expression that may contain dependent object names.</param>
    /// <returns>An empty collection of <see cref="Identifier"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <c>null</c>.</exception>
    public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
    {
        ArgumentNullException.ThrowIfNull(objectName);

        return [];
    }
}