using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(counter, Is.EqualTo(1));
            Assert.That(results, Is.All.EqualTo("test"));
        }
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo("test"));
            Assert.That(counter, Is.EqualTo(2));
        }
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo("test"));
            Assert.That(factoryTokens, Has.Exactly(1).Items.And.All.EqualTo(CancellationToken.None));
        }
    }

    [Test]
    public static void Ctor_GivenNullFactoryWithLifetimeToken_ThrowsArgumentNullException()
    {
        Assert.That(() => new AsyncCache<object, object, object>(null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetByKeyAsync_GivenLifetimeToken_PassesLifetimeTokenToFactory()
    {
        using var lifetime = new CancellationTokenSource();
        var factoryTokens = new ConcurrentQueue<CancellationToken>();
        var cache = new AsyncCache<string, string, string>((_, __, token) =>
        {
            factoryTokens.Enqueue(token);
            return Task.FromResult("test");
        }, lifetime.Token);

        using var callerCancellation = new CancellationTokenSource();
        await cache.GetByKeyAsync("a", "cache_ignore", callerCancellation.Token);

        Assert.That(factoryTokens, Has.Exactly(1).Items.And.All.EqualTo(lifetime.Token));
    }

    [Test]
    public static async Task GetByKeyAsync_WhenLifetimeTokenCancelled_CancelsRunningFactoryAndAllCallers()
    {
        using var lifetime = new CancellationTokenSource();
        var factoryStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var factoryObservedCancellation = false;
        var cache = new AsyncCache<string, string, string>(async (_, __, token) =>
        {
            factoryStarted.SetResult();
            try
            {
                await Task.Delay(Timeout.Infinite, token);
            }
            catch (OperationCanceledException)
            {
                factoryObservedCancellation = true;
                throw;
            }

            return "test";
        }, lifetime.Token);

        var firstCaller = cache.GetByKeyAsync("a", "cache_ignore");
        var secondCaller = cache.GetByKeyAsync("a", "cache_ignore");
        await factoryStarted.Task;

        await lifetime.CancelAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(async () => await firstCaller, Throws.InstanceOf<OperationCanceledException>());
            Assert.That(async () => await secondCaller, Throws.InstanceOf<OperationCanceledException>());
            Assert.That(factoryObservedCancellation, Is.True);
        }
    }

    [Test]
    public static void GetByKeyAsync_WhenLifetimeTokenAlreadyCancelled_FactoryReceivesCancelledTokenOnEveryCall()
    {
        using var lifetime = new CancellationTokenSource();
        lifetime.Cancel();

        var counter = 0;
        var cache = new AsyncCache<string, string, string>((_, __, token) =>
        {
            Interlocked.Increment(ref counter);
            return Task.FromCanceled<string>(token);
        }, lifetime.Token);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(async () => await cache.GetByKeyAsync("a", "cache_ignore"), Throws.InstanceOf<OperationCanceledException>());
            Assert.That(async () => await cache.GetByKeyAsync("a", "cache_ignore"), Throws.InstanceOf<OperationCanceledException>());
            Assert.That(counter, Is.EqualTo(2));
        }
    }

    [Test]
    public static void TryAdd_GivenNullKey_ThrowsArgumentNullException()
    {
        var cache = new AsyncCache<object, object, object>((_, __, ___) => Task.FromResult(new object()));

        Assert.That(() => cache.TryAdd(null, new object()), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task TryAdd_GivenNewKey_ReturnsTrueAndGetByKeyAsyncReturnsValueWithoutInvokingFactory()
    {
        var counter = 0;
        var cache = new AsyncCache<string, string, string>((_, __, ___) =>
        {
            counter++;
            return Task.FromResult("factory");
        });

        var added = cache.TryAdd("a", "added");
        var result = await cache.GetByKeyAsync("a", "cache_ignore");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added, Is.True);
            Assert.That(result, Is.EqualTo("added"));
            Assert.That(counter, Is.Zero);
        }
    }

    [Test]
    public static async Task TryAdd_GivenKeyAlreadyRetrieved_ReturnsFalseAndKeepsExistingValue()
    {
        var cache = new AsyncCache<string, string, string>((_, __, ___) => Task.FromResult("factory"));

        await cache.GetByKeyAsync("a", "cache_ignore");
        var added = cache.TryAdd("a", "added");
        var result = await cache.GetByKeyAsync("a", "cache_ignore");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added, Is.False);
            Assert.That(result, Is.EqualTo("factory"));
        }
    }

    [Test]
    public static async Task TryAdd_GivenKeyAlreadyAdded_ReturnsFalseAndKeepsFirstValue()
    {
        var cache = new AsyncCache<string, string, string>((_, __, ___) => Task.FromResult("factory"));

        cache.TryAdd("a", "first");
        var added = cache.TryAdd("a", "second");
        var result = await cache.GetByKeyAsync("a", "cache_ignore");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added, Is.False);
            Assert.That(result, Is.EqualTo("first"));
        }
    }
}
