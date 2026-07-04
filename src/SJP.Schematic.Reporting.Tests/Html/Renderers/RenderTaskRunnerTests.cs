using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class RenderTaskRunnerTests
{
    [Test]
    public static void RunAllAsync_GivenNullItems_ThrowsArgumentNullException()
    {
        Assert.That(
            () => RenderTaskRunner.RunAllAsync<string>(null!, static s => s, static (_, _) => Task.CompletedTask, CancellationToken.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void RunAllAsync_GivenNullDescribe_ThrowsArgumentNullException()
    {
        Assert.That(
            () => RenderTaskRunner.RunAllAsync<string>(["a"], null!, static (_, _) => Task.CompletedTask, CancellationToken.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void RunAllAsync_GivenNullAction_ThrowsArgumentNullException()
    {
        Assert.That(
            () => RenderTaskRunner.RunAllAsync<string>(["a"], static s => s, null!, CancellationToken.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RunAllAsync_GivenAllItemsSucceed_CompletesWithoutThrowing()
    {
        var items = new[] { "a", "b", "c" };
        var processed = new List<string>();

        await RenderTaskRunner.RunAllAsync(
            items,
            static s => s,
            (item, _) =>
            {
                lock (processed)
                    processed.Add(item);
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.That(processed, Is.EquivalentTo(items));
    }

    [Test]
    public static void RunAllAsync_GivenSingleFailingItem_ThrowsAggregateExceptionWithSingularMessage()
    {
        var items = new[] { "a" };

        var ex = Assert.ThrowsAsync<AggregateException>(() => RenderTaskRunner.RunAllAsync(
            items,
            static s => s,
            static (_, _) => throw new InvalidOperationException("boom"),
            CancellationToken.None));

        using (Assert.EnterMultipleScope())
        {
            // AggregateException.Message appends a per-inner-exception summary in parentheses, so
            // only the fixed prefix set by ThrowIfAnyFailed is asserted here.
            Assert.That(ex!.Message, Does.StartWith("A report rendering operation failed."));
            Assert.That(ex.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(ex.InnerExceptions[0], Is.InstanceOf<RenderException>());
        }
    }

    [Test]
    public static void RunAllAsync_GivenMultipleFailingItems_ThrowsAggregateExceptionWithPluralMessageOrderedByLabel()
    {
        var items = new[] { "charlie", "alpha", "bravo" };

        var ex = Assert.ThrowsAsync<AggregateException>(() => RenderTaskRunner.RunAllAsync(
            items,
            static s => s,
            static (item, _) => throw new InvalidOperationException($"failure for {item}"),
            CancellationToken.None));

        var targets = ex!.InnerExceptions.Cast<RenderException>().Select(static re => re.Target).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex.Message, Does.StartWith("3 report rendering operations failed."));
            Assert.That(targets, Is.EqualTo(new[] { "alpha", "bravo", "charlie" }));
        }
    }

    [Test]
    public static void RunAllAsync_GivenSuccessAndFailureItems_OnlyFailingItemsAreReported()
    {
        var items = new[] { "good", "bad" };

        var ex = Assert.ThrowsAsync<AggregateException>(() => RenderTaskRunner.RunAllAsync(
            items,
            static s => s,
            static (item, _) => item == "bad"
                ? throw new InvalidOperationException("boom")
                : Task.CompletedTask,
            CancellationToken.None));

        var targets = ex!.InnerExceptions.Cast<RenderException>().Select(static re => re.Target).ToList();
        Assert.That(targets, Is.EqualTo(new[] { "bad" }));
    }

    [Test]
    public static void RunAllAsync_GivenCancelledToken_PropagatesOperationCanceledExceptionInsteadOfFailure()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var items = new[] { "a" };

        Assert.ThrowsAsync<OperationCanceledException>(() => RenderTaskRunner.RunAllAsync(
            items,
            static s => s,
            (_, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            },
            cts.Token));
    }

    [Test]
    public static void ThrowIfAnyFailed_GivenNullFailures_ThrowsArgumentNullException()
    {
        Assert.That(() => RenderTaskRunner.ThrowIfAnyFailed(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void ThrowIfAnyFailed_GivenNoFailures_DoesNotThrow()
    {
        Assert.That(() => RenderTaskRunner.ThrowIfAnyFailed([]), Throws.Nothing);
    }

    [Test]
    public static void ThrowIfAnyFailed_GivenFailures_ThrowsAggregateExceptionOrderedByTarget()
    {
        var failures = new[]
        {
            new RenderException("zeta", new InvalidOperationException()),
            new RenderException("alpha", new InvalidOperationException()),
        };

        var ex = Assert.Throws<AggregateException>(() => RenderTaskRunner.ThrowIfAnyFailed(failures));
        var targets = ex!.InnerExceptions.Cast<RenderException>().Select(static re => re.Target).ToList();

        Assert.That(targets, Is.EqualTo(new[] { "alpha", "zeta" }));
    }
}
