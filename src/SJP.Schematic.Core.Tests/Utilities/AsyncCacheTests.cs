using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class AsyncCacheTests
{
    [Test]
    public static void Ctor_GivenNullFactory_ThrowsArgumentNullException()
    {
        Assert.That(() => new AsyncCache<object, object, object>(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetByKeyAsync_GivenNullKey_ThrowsArgumentNullException()
    {
        var cache = new AsyncCache<object, object, object>((_, __, ___) => Task.FromResult(new object()));

        Assert.That(() => cache.GetByKeyAsync(null, new object()), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetByKeyAsync_GivenNullCache_ThrowsArgumentNullException()
    {
        var cache = new AsyncCache<object, object, object>((_, __, ___) => Task.FromResult(new object()));

        Assert.That(() => cache.GetByKeyAsync(new object(), null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetByKeyAsync_WhenCalledTwiceWithSameKey_InvokesFactoryOnlyOnce()
    {
        var counter = 0;
        var cache = new AsyncCache<string, string, string>((_, __, ___) =>
        {
            counter++;
            return Task.FromResult(string.Empty);
        });

        await cache.GetByKeyAsync("a", "cache_ignore");
        await cache.GetByKeyAsync("a", "cache_ignore");

        Assert.That(counter, Is.EqualTo(1));
    }

    [Test]
    public static async Task GetByKeyAsync_WhenCalledConcurrentlyWithSameKey_InvokesFactoryOnlyOnce()
    {
        var counter = 0;
        var completionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cache = new AsyncCache<string, string, string>((_, __, ___) =>
        {
            Interlocked.Increment(ref counter);
            return completionSource.Task;
        });

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => cache.GetByKeyAsync("a", "cache_ignore"))
            .ToList();
        completionSource.SetResult("test");

        var results = await Task.WhenAll(tasks);

        Assert.Multiple(() =>
        {
            Assert.That(counter, Is.EqualTo(1));
            Assert.That(results, Is.All.EqualTo("test"));
        });
    }

    [Test]
    public static async Task GetByKeyAsync_WhenFactoryFails_InvokesFactoryAgainOnNextCall()
    {
        var counter = 0;
        var cache = new AsyncCache<string, string, string>((_, __, ___) =>
        {
            counter++;
            return counter == 1
                ? Task.FromException<string>(new InvalidOperationException())
                : Task.FromResult("test");
        });

        Assert.That(async () => await cache.GetByKeyAsync("a", "cache_ignore"), Throws.InstanceOf<InvalidOperationException>());

        var result = await cache.GetByKeyAsync("a", "cache_ignore");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("test"));
            Assert.That(counter, Is.EqualTo(2));
        });
    }

    [Test]
    public static async Task GetByKeyAsync_WhenFirstCallerCancels_DoesNotCancelRemainingCallers()
    {
        var completionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cache = new AsyncCache<string, string, string>((_, __, ___) => completionSource.Task);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellingCaller = cache.GetByKeyAsync("a", "cache_ignore", cancellationTokenSource.Token);
        var waitingCaller = cache.GetByKeyAsync("a", "cache_ignore");

        await cancellationTokenSource.CancelAsync();
        Assert.That(async () => await cancellingCaller, Throws.InstanceOf<OperationCanceledException>());

        completionSource.SetResult("test");

        Assert.That(await waitingCaller, Is.EqualTo("test"));
    }

    [Test]
    public static async Task GetByKeyAsync_WhenFirstCallerCancels_DoesNotCancelFactoryExecution()
    {
        var factoryTokens = new ConcurrentQueue<CancellationToken>();
        var completionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cache = new AsyncCache<string, string, string>((_, __, token) =>
        {
            factoryTokens.Enqueue(token);
            return completionSource.Task;
        });

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellingCaller = cache.GetByKeyAsync("a", "cache_ignore", cancellationTokenSource.Token);

        await cancellationTokenSource.CancelAsync();
        Assert.That(async () => await cancellingCaller, Throws.InstanceOf<OperationCanceledException>());

        completionSource.SetResult("test");

        var result = await cache.GetByKeyAsync("a", "cache_ignore");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("test"));
            Assert.That(factoryTokens, Has.Exactly(1).Items.And.All.EqualTo(CancellationToken.None));
        });
    }
}
