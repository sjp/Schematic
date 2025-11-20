using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class AsyncEnumerableExtensionsTests
{
    [Test]
    public static async Task SelectAwait_WithNullSourceForValueTaskSelector_ThrowsArgNullException()
    {
        IAsyncEnumerable<string> source = null;
        Assert.That(() => source.SelectAwait(_ => ValueTask.FromResult(_)), Throws.ArgumentNullException);
    }

    [Test]
    public static void SelectAwait_WithNullSelectorForValueTaskSelector_ThrowsArgNullException()
    {
        var source = AsyncEnumerable.Empty<string>();
        Func<string, ValueTask<string>> selector = null;

        Assert.That(() => source.SelectAwait(selector), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task SelectAwait_WithValidaSelectorForValueTaskSelector_ReturnsExpectedResults()
    {
        var ints = new[] { 1, 2, 3 };
        var source = ints.ToAsyncEnumerable();
        static ValueTask<int> selector(int input) => ValueTask.FromResult(input * input);

        var result = await source.SelectAwait(selector).ToListAsync();

        var expected = new[] { 1, 4, 9 };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static async Task SelectAwait_WithNullSourceForValueTaskCancellationSelector_ThrowsArgNullException()
    {
        IAsyncEnumerable<string> source = null;
        Assert.That(() => source.SelectAwait((_, __) => ValueTask.FromResult(_)), Throws.ArgumentNullException);
    }

    [Test]
    public static void SelectAwait_WithNullSelectorForValueTaskCancellationSelector_ThrowsArgNullException()
    {
        var source = AsyncEnumerable.Empty<string>();
        Func<string, CancellationToken, ValueTask<string>> selector = null;

        Assert.That(() => source.SelectAwait(selector), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task SelectAwait_WithValidaSelectorForValueTaskCancellationSelector_ReturnsExpectedResults()
    {
        var ints = new[] { 1, 2, 3 };
        var source = ints.ToAsyncEnumerable();
        static ValueTask<int> selector(int input, CancellationToken ct)
        {
            return !ct.IsCancellationRequested
                ? ValueTask.FromResult(input * input)
                : ValueTask.FromResult(input);
        }

        var result = await source.SelectAwait(selector).ToListAsync();

        var expected = new[] { 1, 4, 9 };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static async Task SelectAwait_WithNullSourceForTaskSelector_ThrowsArgNullException()
    {
        IAsyncEnumerable<string> source = null;
        Assert.That(() => source.SelectAwait(_ => Task.FromResult(_)), Throws.ArgumentNullException);
    }

    [Test]
    public static void SelectAwait_WithNullSelectorForTaskSelector_ThrowsArgNullException()
    {
        var source = AsyncEnumerable.Empty<string>();
        Func<string, Task<string>> selector = null;

        Assert.That(() => source.SelectAwait(selector), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task SelectAwait_WithValidaSelectorForTaskSelector_ReturnsExpectedResults()
    {
        var ints = new[] { 1, 2, 3 };
        var source = ints.ToAsyncEnumerable();
        static Task<int> selector(int input) => Task.FromResult(input * input);

        var result = await source.SelectAwait(selector).ToListAsync();

        var expected = new[] { 1, 4, 9 };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static async Task SelectAwait_WithNullSourceForTaskCancellationSelector_ThrowsArgNullException()
    {
        IAsyncEnumerable<string> source = null;
        Assert.That(() => source.SelectAwait((_, __) => Task.FromResult(_)), Throws.ArgumentNullException);
    }

    [Test]
    public static void SelectAwait_WithNullSelectorForTaskCancellationSelector_ThrowsArgNullException()
    {
        var source = AsyncEnumerable.Empty<string>();
        Func<string, CancellationToken, Task<string>> selector = null;

        Assert.That(() => source.SelectAwait(selector), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task SelectAwait_WithValidaSelectorForTaskCancellationSelector_ReturnsExpectedResults()
    {
        var ints = new[] { 1, 2, 3 };
        var source = ints.ToAsyncEnumerable();
        static Task<int> selector(int input, CancellationToken ct)
        {
            return !ct.IsCancellationRequested
                ? Task.FromResult(input * input)
                : Task.FromResult(input);
        }

        var result = await source.SelectAwait(selector).ToListAsync();

        var expected = new[] { 1, 4, 9 };
        Assert.That(result, Is.EqualTo(expected));
    }
}