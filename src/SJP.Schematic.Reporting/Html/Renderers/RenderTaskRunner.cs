using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Html.Renderers;

/// <summary>
/// Runs a set of rendering operations concurrently, letting every operation run to completion even
/// when others fault, then reports all failures together rather than aborting on the first.
/// </summary>
internal static class RenderTaskRunner
{
    /// <summary>
    /// Runs <paramref name="action"/> for each item concurrently. An item that throws is captured
    /// (labelled via <paramref name="describe"/>) instead of aborting its siblings. Once every item
    /// has finished, if any failed an <see cref="AggregateException"/> of <see cref="RenderException"/>s
    /// — one per failure, ordered by label — is thrown. Cancellation requested through
    /// <paramref name="cancellationToken"/> propagates as an <see cref="OperationCanceledException"/>
    /// and is never recorded as a failure.
    /// </summary>
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

        var tasks = items
            .Select(item => RunOneAsync(item, describe, action, failures, cancellationToken))
            .ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);

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
            await action(item, cancellationToken).ConfigureAwait(false);
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
