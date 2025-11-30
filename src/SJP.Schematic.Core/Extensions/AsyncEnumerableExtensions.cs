using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods for working with <see cref="IAsyncEnumerable{T}"/> instances.
/// Ideally these wouldn't be required but are necessary for bridging feature parity between .NET 10 and <c>System.Linq.Async</c>.
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Projects each element of an async-enumerable sequence into a new form by applying an asynchronous selector function to each member of the source sequence and awaiting the result.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by running the selector function for each element in the source sequence and awaiting the result.</typeparam>
    /// <param name="source">A sequence of elements to invoke a transform function on.</param>
    /// <param name="selector">An asynchronous transform function to apply to each source element.</param>
    /// <returns>An async-enumerable sequence whose elements are the result of invoking the transform function on each element of the source sequence and awaiting the result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return source.Select((TSource element, CancellationToken _) => selector(element));
    }

    /// <summary>
    /// Projects each element of an async-enumerable sequence into a new form by applying an asynchronous selector function to each member of the source sequence and awaiting the result.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by running the selector function for each element in the source sequence and awaiting the result.</typeparam>
    /// <param name="source">A sequence of elements to invoke a transform function on.</param>
    /// <param name="selector">An asynchronous transform function to apply to each source element.</param>
    /// <returns>An async-enumerable sequence whose elements are the result of invoking the transform function on each element of the source sequence and awaiting the result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return source.Select(selector);
    }

    /// <summary>
    /// Projects each element of an async-enumerable sequence into a new form by applying an asynchronous selector function to each member of the source sequence and awaiting the result.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by running the selector function for each element in the source sequence and awaiting the result.</typeparam>
    /// <param name="source">A sequence of elements to invoke a transform function on.</param>
    /// <param name="selector">An asynchronous transform function to apply to each source element.</param>
    /// <returns>An async-enumerable sequence whose elements are the result of invoking the transform function on each element of the source sequence and awaiting the result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return source.Select((TSource element, CancellationToken _) => selector(element).ToValue());
    }

    /// <summary>
    /// Projects each element of an async-enumerable sequence into a new form by applying an asynchronous selector function to each member of the source sequence and awaiting the result.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by running the selector function for each element in the source sequence and awaiting the result.</typeparam>
    /// <param name="source">A sequence of elements to invoke a transform function on.</param>
    /// <param name="selector">An asynchronous transform function to apply to each source element.</param>
    /// <returns>An async-enumerable sequence whose elements are the result of invoking the transform function on each element of the source sequence and awaiting the result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return source.Select((element, ct) => selector(element, ct).ToValue());
    }
}