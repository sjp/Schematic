using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Reporting;

internal static class CollectionExtensions
{
    public static uint UCount<T>(this IReadOnlyCollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        return (uint)collection.Count;
    }

    /// <summary>
    /// Returns the element count as a <see cref="uint"/>. This overload fully enumerates the
    /// sequence; prefer the <see cref="IReadOnlyCollection{T}"/> overload when the count is known.
    /// </summary>
    public static uint UCount<T>(this IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        return (uint)collection.Count();
    }
}
