using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Extension methods for running many per-item asynchronous operations with a bounded degree of concurrency,
/// while preserving the input ordering in the results.
/// </summary>
public static class BoundedConcurrencyExtensions
{
    /// <summary>
    /// Applies an asynchronous selector to every element of a source collection, running at most
    /// <paramref name="maxDegreeOfParallelism"/> operations at once.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TResult">The type of the projected results.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="selector">An asynchronous transform function to apply to each source element.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of selectors to run concurrently.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The projected results, in the same order as <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxDegreeOfParallelism"/> is zero or negative.</exception>
    /// <remarks>
    /// On the first exception, items that have not yet started are cancelled instead of also being started; this
    /// matches <see cref="System.Threading.Tasks.Parallel.ForEachAsync{TSource}(IEnumerable{TSource}, ParallelOptions, Func{TSource, CancellationToken, ValueTask})"/>'s behaviour.
    /// </remarks>
    public static async Task<TResult[]> SelectBoundedAsync<TSource, TResult>(
        this IReadOnlyList<TSource> source,
        Func<TSource, CancellationToken, Task<TResult>> selector,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxDegreeOfParallelism);

        var results = new TResult[source.Count];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, source.Count),
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cancellationToken },
            async (i, ct) => results[i] = await selector(source[i], ct)
        );

        return results;
    }

    /// <summary>
    /// Applies an asynchronous selector to every element of a source sequence, keeping up to <paramref name="window"/>
    /// selectors in flight at once, and yielding results in the same order as <paramref name="source"/> as soon as
    /// each one completes.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TResult">The type of the projected results.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">An asynchronous transform function to apply to each source element.</param>
    /// <param name="window">The maximum number of selectors to keep in flight at once.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The projected results, in the same order as <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="window"/> is zero or negative.</exception>
    /// <remarks>
    /// Up to <paramref name="window"/> selectors are started ahead of the consumer asking for them, so a consumer
    /// that stops enumerating early wastes up to <paramref name="window"/> operations. If the caller stops early or
    /// an item throws, every task still in the window is cancelled and awaited (its result, or exception, is
    /// discarded) so it is not left unobserved.
    /// </remarks>
    public static IAsyncEnumerable<TResult> SelectOrderedPrefetchAsync<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, CancellationToken, Task<TResult>> selector,
        int window,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(window);

        return SelectOrderedPrefetchAsyncCore(source, selector, window, cancellationToken);
    }

    private static async IAsyncEnumerable<TResult> SelectOrderedPrefetchAsyncCore<TSource, TResult>(
        IEnumerable<TSource> source,
        Func<TSource, CancellationToken, Task<TResult>> selector,
        int window,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var pending = new Queue<Task<TResult>>(window);
        try
        {
            foreach (var item in source)
            {
                pending.Enqueue(selector(item, cts.Token));
                if (pending.Count >= window)
                    yield return await pending.Dequeue();
            }

            while (pending.Count > 0)
                yield return await pending.Dequeue();
        }
        finally
        {
            await cts.CancelAsync();

            // observe abandoned tasks so their exceptions are not left unobserved
            foreach (var task in pending)
            {
                try
                {
                    await task;
                }
                catch
                {
                    // abandoned
                }
            }
        }
    }
}
