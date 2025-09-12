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
public class VersionResolvingDictionary<T> : IVersionedLookup<T>
    where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionResolvingDictionary{T}"/> class.
    /// </summary>
    /// <param name="lookup">A lookup to retrieve versions by value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="lookup"/> is empty.</exception>
    public VersionResolvingDictionary(IReadOnlyDictionary<Version, T> lookup)
    {
        ArgumentNullException.ThrowIfNull(lookup);
        if (lookup.Empty())
            throw new ArgumentException("At least one value must be present in the given lookup.", nameof(lookup));

        _lookup = lookup;
    }

    /// <summary>
    /// Retrieves the value that is applicable for the given version.
    /// </summary>
    /// <param name="version">A version.</param>
    /// <returns>An object of type <typeparamref name="T" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="version"/> is <see langword="null" />.</exception>
    public T GetValue(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);

        var versionKeys = _lookup.Keys.Order().ToList();
        var firstVersion = versionKeys[0];
        if (version <= firstVersion)
            return _lookup[firstVersion];

        // we want to find the version that's *at least* the version
        // but we want to use the highest version possible
        versionKeys.Reverse();

        var matchingVersion = versionKeys.Find(v => version >= v);
        if (matchingVersion == null)
            return _lookup[firstVersion];

        return _lookup[matchingVersion];
    }

    private readonly IReadOnlyDictionary<Version, T> _lookup;
}