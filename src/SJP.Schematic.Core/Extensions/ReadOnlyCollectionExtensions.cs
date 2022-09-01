using System;
using System.Collections.Generic;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods for working with read-only collections.
/// </summary>
public static class ReadOnlyCollectionExtensions
{
    /// <summary>
    /// Determines whether a collection is empty.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns><c>true</c> if the collection has no elements; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static bool Empty<T>(this IReadOnlyCollection<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Count == 0;
    }
}