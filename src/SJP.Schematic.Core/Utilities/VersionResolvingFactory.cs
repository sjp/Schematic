using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// A lookup container that is intended to retrieve database implementations for a given version, avoiding the need to provide version ranges manually.
/// </summary>
/// <typeparam name="T">The type of value to retrieve from the lookup.</typeparam>
/// <seealso cref="IVersionedLookup{T}" />
public class VersionResolvingFactory<T> : IVersionedLookup<T>
    where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionResolvingFactory{T}"/> class.
    /// </summary>
    /// <param name="lookup">A lookup where the version keys are the minimum supported version for the associated values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="lookup"/> is empty.</exception>
    public VersionResolvingFactory(IReadOnlyDictionary<Version, Func<T>> lookup)
    {
        ArgumentNullException.ThrowIfNull(lookup);
        if (lookup.Empty())
            throw new ArgumentException("At least one value must be present in the given lookup.", nameof(lookup));

        _lookup = lookup;
        _descendingVersions = [.. lookup.Keys.OrderDescending()];
    }

    /// <summary>
    /// Retrieves the value that is applicable for the given version.
    /// </summary>
    /// <param name="version">A version.</param>
    /// <returns>An object of type <typeparamref name="T" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="version"/> is <see langword="null" />.</exception>
    /// <remarks>The factory invoked is the one stored against the highest version that does not exceed <paramref name="version"/>. A version lower than every version in the lookup resolves to the factory stored against the lowest version.</remarks>
    public T GetValue(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);

        // we want to find the version that's *at least* the version
        // but we want to use the highest version possible
        var matchingVersion = _descendingVersions.Find(v => version >= v)
            ?? _descendingVersions[^1];

        var resultFactory = _lookup[matchingVersion];
        return resultFactory.Invoke();
    }

    private readonly IReadOnlyDictionary<Version, Func<T>> _lookup;
    private readonly List<Version> _descendingVersions;
}