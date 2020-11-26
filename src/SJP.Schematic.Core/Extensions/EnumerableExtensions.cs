using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Extensions
{
    /// <summary>
    /// Convenience extension methods for working with <see cref="IEnumerable{T}"/> instances.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether a collection is empty.
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <returns><c>true</c> if the collection has no elements; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static bool Empty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return !source.Any();
        }

        /// <summary>
        /// Determines whether a collection has no elements matching a given predicate.
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="predicate">A filter to match against elements in <paramref name="source"/>.</param>
        /// <returns><c>true</c> if the collection has no elements matching the predicate; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
        public static bool Empty<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return !source.Any(predicate);
        }

        /// <summary>
        /// Determines whether a collection has elements which are <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <returns><c>true</c> if the collection has elements which are <c>null</c>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static bool AnyNull<T>(this IEnumerable<T> source) where T : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Any(static x => x == null);
        }

        /// <summary>
        /// Returns only the distinct elements in a collection, deriving uniqueness from a selector function. This uses the default comparer for <typeparamref name="TKey"/> to test uniqueness.
        /// </summary>
        /// <typeparam name="TSource">The type of objects to enumerate.</typeparam>
        /// <typeparam name="TKey">The type of key to test for uniqueness with.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="keySelector">The selector which returns a key used for uniqueness testing.</param>
        /// <returns>A collection that returns a unique set of results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return source.DistinctBy(keySelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Returns only the distinct elements in a collection, deriving uniqueness from a selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of objects to enumerate.</typeparam>
        /// <typeparam name="TKey">The type of key to test for uniqueness with.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="keySelector">The selector which returns a key used for uniqueness testing.</param>
        /// <param name="comparer">A comparer used for uniqueness testing.</param>
        /// <returns>A collection that returns a unique set of results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="comparer"/> is <c>null</c>.</exception>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
           Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            comparer ??= EqualityComparer<TKey>.Default;

            return _(); IEnumerable<TSource> _()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                        yield return element;
                }
            }
        }
    }
}
