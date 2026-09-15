using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[CancelAfter(30 * 1000)]
internal static class BoundedConcurrencyExtensionsTests
{
    [Test]
    public static void SelectBoundedAsync_GivenNullSource_ThrowsArgumentNullException()
    {
        IReadOnlyList<int> source = null;

        Assert.That(
            () => source.SelectBoundedAsync(static (i, _) => Task.FromResult(i), 1, CancellationToken.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void SelectBoundedAsync_GivenNullSelector_ThrowsArgumentNullException()
    {
        IReadOnlyList<int> source = [1];

        Assert.That(
            () => source.SelectBoundedAsync<int, int>(null, 1, CancellationToken.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("selector")
        );
    }

    [TestCase(0)]
    [TestCase(-1)]
    public static void SelectBoundedAsync_GivenZeroOrNegativeDegree_ThrowsArgumentOutOfRangeException(int maxDegreeOfParallelism)
    {
        IReadOnlyList<int> source = [1];

        Assert.That(
            () => source.SelectBoundedAsync(static (i, _) => Task.FromResult(i), maxDegreeOfParallelism, CancellationToken.None),
            Throws.InstanceOf<ArgumentOutOfRangeException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("maxDegreeOfParallelism")
        );
    }

    [Test]
    public static async Task SelectBoundedAsync_GivenEmptySource_ReturnsEmptyArray()
    {
        IReadOnlyList<int> source = [];

        var results = await source.SelectBoundedAsync(static (i, _) => Task.FromResult(i), 4, CancellationToken.None);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public static async Task SelectBoundedAsync_WhenLaterItemsCompleteFirst_ReturnsResultsInSourceOrder()
    {
        IReadOnlyList<int> source = Enumerable.Range(0, 20).ToList();

        var results = await source.SelectBoundedAsync(
            static async (i, ct) =>
            {
                // earlier items take longer, so completion order is the reverse of source order
                await Task.Delay(TimeSpan.FromMilliseconds((20 - i) * 2), ct);
                return i * 10;
            },
            8,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(results, Is.EqualTo(source.Select(static i => i * 10)));
    }

    [Test]
    public static async Task SelectBoundedAsync_GivenMoreItemsThanDegree_RunsNoMoreThanDegreeAtOnce()
    {
        const int maxDegreeOfParallelism = 3;
        IReadOnlyList<int> source = Enumerable.Range(0, maxDegreeOfParallelism * 5).ToList();
        var tracker = new InFlightTracker();

        await source.SelectBoundedAsync(
            async (i, ct) =>
            {
                using var _ = tracker.Enter();
                await Task.Delay(TimeSpan.FromMilliseconds(20), ct);
                return i;
            },
            maxDegreeOfParallelism,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(tracker.Peak, Is.EqualTo(maxDegreeOfParallelism));
    }

    [Test]
    public static void SelectBoundedAsync_WhenAnItemThrows_DoesNotStartRemainingItems()
    {
        IReadOnlyList<int> source = Enumerable.Range(0, 10).ToList();
        var startedCount = 0;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                async () => await source.SelectBoundedAsync<int, int>(
                    (_, _) =>
                    {
                        Interlocked.Increment(ref startedCount);
                        throw new InvalidOperationException("failed");
                    },
                    1,
                    TestContext.CurrentContext.CancellationToken),
                Throws.InvalidOperationException);
            Assert.That(startedCount, Is.EqualTo(1));
        }
    }

    [Test]
    public static void SelectOrderedPrefetchAsync_GivenNullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int> source = null;

        Assert.That(
            () => source.SelectOrderedPrefetchAsync(static (i, _) => Task.FromResult(i), 1, CancellationToken.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void SelectOrderedPrefetchAsync_GivenNullSelector_ThrowsArgumentNullException()
    {
        IEnumerable<int> source = [1];

        Assert.That(
            () => source.SelectOrderedPrefetchAsync<int, int>(null, 1, CancellationToken.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("selector")
        );
    }

    [TestCase(0)]
    [TestCase(-1)]
    public static void SelectOrderedPrefetchAsync_GivenZeroOrNegativeWindow_ThrowsArgumentOutOfRangeException(int window)
    {
        IEnumerable<int> source = [1];

        Assert.That(
            () => source.SelectOrderedPrefetchAsync(static (i, _) => Task.FromResult(i), window, CancellationToken.None),
            Throws.InstanceOf<ArgumentOutOfRangeException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("window")
        );
    }

    [Test]
    public static async Task SelectOrderedPrefetchAsync_WhenLaterItemsCompleteFirst_YieldsResultsInSourceOrder()
    {
        var source = Enumerable.Range(0, 20).ToList();

        var results = await source
            .SelectOrderedPrefetchAsync(
                static async (i, ct) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds((20 - i) * 2), ct);
                    return i * 10;
                },
                8,
                TestContext.CurrentContext.CancellationToken)
            .ToListAsync(TestContext.CurrentContext.CancellationToken);

        Assert.That(results, Is.EqualTo(source.Select(static i => i * 10)));
    }

    [Test]
    public static async Task SelectOrderedPrefetchAsync_GivenMoreItemsThanWindow_KeepsNoMoreThanWindowInFlight()
    {
        const int window = 3;
        var source = Enumerable.Range(0, window * 5);
        var tracker = new InFlightTracker();

        var results = await source
            .SelectOrderedPrefetchAsync(
                async (i, ct) =>
                {
                    using var _ = tracker.Enter();
                    await Task.Delay(TimeSpan.FromMilliseconds(20), ct);
                    return i;
                },
                window,
                TestContext.CurrentContext.CancellationToken)
            .ToListAsync(TestContext.CurrentContext.CancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Is.EqualTo(source));
            Assert.That(tracker.Peak, Is.EqualTo(window));
        }
    }

    [Test]
    public static async Task SelectOrderedPrefetchAsync_WhenConsumerStopsEarly_CancelsAndAwaitsItemsStillInFlight()
    {
        const int window = 4;
        var tokens = new List<CancellationToken>();
        var completedCount = 0;

        var source = Enumerable.Range(0, 100);
        var results = source.SelectOrderedPrefetchAsync(
            async (i, ct) =>
            {
                lock (tokens)
                    tokens.Add(ct);

                try
                {
                    // only the first item finishes promptly; the rest wait until they are cancelled
                    if (i > 0)
                        await Task.Delay(Timeout.InfiniteTimeSpan, ct);
                    return i;
                }
                finally
                {
                    Interlocked.Increment(ref completedCount);
                }
            },
            window,
            TestContext.CurrentContext.CancellationToken);

        await foreach (var _ in results)
            break;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tokens, Has.Count.EqualTo(window));
            Assert.That(tokens.Select(static t => t.IsCancellationRequested), Is.All.True);
            Assert.That(completedCount, Is.EqualTo(window));
        }
    }

    [Test]
    public static void SelectOrderedPrefetchAsync_WhenAnItemThrows_PropagatesException()
    {
        var source = Enumerable.Range(0, 10);
        var results = source.SelectOrderedPrefetchAsync(
            static async (i, ct) =>
            {
                await Task.Yield();
                return i == 2
                    ? throw new InvalidOperationException("failed")
                    : i;
            },
            4,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(async () => await results.ToListAsync(TestContext.CurrentContext.CancellationToken), Throws.InvalidOperationException);
    }

    private sealed class InFlightTracker
    {
        public int Peak
        {
            get
            {
                lock (_lock)
                    return _peak;
            }
        }

        public IDisposable Enter()
        {
            lock (_lock)
            {
                _current++;
                _peak = Math.Max(_peak, _current);
            }

            return new Exit(this);
        }

        private void Leave()
        {
            lock (_lock)
                _current--;
        }

        private readonly Lock _lock = new();
        private int _current;
        private int _peak;

        private sealed class Exit(InFlightTracker tracker) : IDisposable
        {
            public void Dispose() => tracker.Leave();
        }
    }
}
