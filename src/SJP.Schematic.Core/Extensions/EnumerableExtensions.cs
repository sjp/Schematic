using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods for working with <see cref="IEnumerable{T}"/> instances.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Determines whether a collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns><c>true</c> if the collection is <c>null</c> or has no elements; otherwise <c>false</c>.</returns>
    public static bool NullOrEmpty<T>(this IEnumerable<T> source) => source?.Any() != true;

    /// <summary>
    /// Determines whether a collection is empty.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns><c>true</c> if the collection has no elements; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static bool Empty<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return !source.Any();
    }

    /// <summary>
    /// Determines whether a collection is <c>null</c> or has elements which are <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns><c>true</c> if the collection is <c>null</c> or has elements which are <c>null</c>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static bool NullOrAnyNull<T>(this IEnumerable<T> source) where T : notnull
        => source?.Any(static x => x == null) != false;

    /// <summary>
    /// Determines whether a collection has elements which are <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns><c>true</c> if the collection has elements which are <c>null</c>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static bool AnyNull<T>(this IEnumerable<T> source) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Any(static x => x == null);
    }

    /// <summary>
    /// An eagerly evaluating group by implementation that is faster and lower in memory allocation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used for uniqueness testing.</typeparam>
    /// <typeparam name="TValue">The type of the value used for each group member.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="keySelector">The selector which returns a key used for uniqueness testing.</param>
    /// <returns>A <see cref="IReadOnlyDictionary{TKey, TValue}"/> whose keys are the group keys, and values are members of the given group.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IReadOnlyDictionary<TKey, List<TValue>> GroupAsDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector)
        where TKey : notnull
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var dictionary = new Dictionary<TKey, List<TValue>>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!dictionary.TryGetValue(key, out var grouping))
            {
                grouping = new List<TValue>(1);
                dictionary.Add(key, grouping);
            }
            grouping.Add(item);
        }
        return dictionary;
    }

    /// <summary>
    /// An eagerly evaluating group by implementation that is faster and lower in memory allocation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used for uniqueness testing.</typeparam>
    /// <typeparam name="TValue">The type of the value used for each group member.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="keySelector">The selector which returns a key used for uniqueness testing.</param>
    /// <param name="comparer">A comparer used for uniqueness testing. Defaults to <see cref="EqualityComparer{TKey}.Default"/> when <c>null</c>.</param>
    /// <returns>A <see cref="IReadOnlyDictionary{TKey, TValue}"/> whose keys are the group keys, and values are members of the given group.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IReadOnlyDictionary<TKey, List<TValue>> GroupAsDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
        where TKey : notnull
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        comparer ??= EqualityComparer<TKey>.Default;

        var dictionary = new Dictionary<TKey, List<TValue>>(comparer);
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!dictionary.TryGetValue(key, out var grouping))
            {
                grouping = new List<TValue>(1);
                dictionary.Add(key, grouping);
            }
            grouping.Add(item);
        }
        return dictionary;
    }
}