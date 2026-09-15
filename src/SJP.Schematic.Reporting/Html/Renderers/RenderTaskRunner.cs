using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Html.Renderers;

/// <summary>
/// Runs a set of rendering operations in parallel, letting every operation run to completion even
/// when others fault, then reports all failures together rather than aborting on the first.
/// </summary>
internal static class RenderTaskRunner
{
    /// <summary>
    /// Runs <paramref name="action"/> for each item in parallel on the thread pool, with at most
    /// <see cref="Environment.ProcessorCount"/> items running at once. An item that throws is captured
    /// (labelled via <paramref name="describe"/>) instead of aborting its siblings. Once every item
    /// has finished, if any failed an <see cref="AggregateException"/> of <see cref="RenderException"/>s
    /// — one per failure, ordered by label — is thrown. Cancellation requested through
    /// <paramref name="cancellationToken"/> propagates as an <see cref="OperationCanceledException"/>
    /// and is never recorded as a failure; items that have not started by then are not started.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="items"/>, <paramref name="describe"/> or <paramref name="action"/> is <see langword="null" />.</exception>
    public static async Task RunAllAsync<T>(
        IEnumerable<T> items,
        Func<T, string> describe,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(describe);
        ArgumentNullException.ThrowIfNull(action);

        var failures = new ConcurrentBag<RenderException>();

        // Mapping and serializing an item is CPU work that runs synchronously before its first
        // write yields, so starting every item from this thread would do all of that work on one
        // core and leave every output file open at once. Running items on the thread pool, a bounded
        // number at a time, spreads the work across cores and caps the number of files being written.
        // Each item catches its own failures, so only cancellation can stop items from starting.
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cancellationToken,
        };
        await Parallel.ForEachAsync(
            items,
            options,
            (item, _) => new ValueTask(RunOneAsync(item, describe, action, failures, cancellationToken)));

        ThrowIfAnyFailed(failures);
    }

    private static async Task RunOneAsync<T>(
        T item,
        Func<T, string> describe,
        Func<T, CancellationToken, Task> action,
        ConcurrentBag<RenderException> failures,
        CancellationToken cancellationToken)
    {
        try
        {
            await action(item, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            failures.Add(new RenderException(describe(item), ex));
        }
    }

    /// <summary>
    /// Throws a single <see cref="AggregateException"/> containing <paramref name="failures"/>
    /// (ordered by label) when any are present; otherwise does nothing.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="failures"/> is <see langword="null" />.</exception>
    public static void ThrowIfAnyFailed(IEnumerable<RenderException> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        var ordered = failures
            .OrderBy(static f => f.Target, StringComparer.Ordinal)
            .ToList();

        if (ordered.Count == 0)
            return;

        var message = ordered.Count == 1
            ? "A report rendering operation failed."
            : $"{ordered.Count} report rendering operations failed.";

        throw new AggregateException(message, ordered);
    }
}
