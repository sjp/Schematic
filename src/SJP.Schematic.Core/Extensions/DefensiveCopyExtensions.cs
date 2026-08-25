using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Methods used to take snapshots of collections provided by callers.
/// </summary>
internal static class DefensiveCopyExtensions
{
    /// <summary>
    /// Creates a copy of a collection, ensuring that neither the collection nor any of its elements are <see langword="null" />.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="paramName">The name of the parameter that <paramref name="source"/> was provided as.</param>
    /// <returns>A copy of <paramref name="source"/> that the caller is unable to modify.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <remarks><paramref name="source"/> is enumerated exactly once, so the validated contents are always the stored contents.</remarks>
    public static List<T> ToDefensiveCopy<T>(this IEnumerable<T> source, string paramName)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source, paramName);

        var copy = source.ToList();
        if (copy.AnyNull())
            throw new ArgumentNullException(paramName);

        return copy;
    }

    /// <summary>
    /// Creates a copy of a dictionary, preserving the key comparer where one is available.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="source">The source dictionary.</param>
    /// <param name="paramName">The name of the parameter that <paramref name="source"/> was provided as.</param>
    /// <returns>A dictionary equivalent to <paramref name="source"/> that the caller is unable to modify.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null" />.</exception>
    public static IReadOnlyDictionary<TKey, TValue> ToDefensiveCopy<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, string paramName)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source, paramName);

        // a frozen dictionary is already immutable, so it can be shared rather than copied
        if (source is FrozenDictionary<TKey, TValue> frozen)
            return frozen;

        var comparer = source is Dictionary<TKey, TValue> dictionary
            ? dictionary.Comparer
            : EqualityComparer<TKey>.Default;

        return source.ToReadOnlyDictionary(comparer);
    }
}
