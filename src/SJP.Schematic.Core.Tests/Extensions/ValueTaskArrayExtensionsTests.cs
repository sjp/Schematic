using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
public static class ValueTaskArrayExtensionsTests
{
    [Test]
    public static void WhenAll_WhenGivenNullTasks_ThrowsArgNullException()
    {
        Assert.That(() => ValueTaskArrayExtensions.WhenAll<object>(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll_WhenGivenCompletedTasks_CompletesSynchronously()
    {
        var tasks = new[]
        {
            ValueTask.FromResult(2),
            ValueTask.FromResult(4),
        };

        var result = tasks.WhenAll();

        Assert.That(result.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public static async Task WhenAll_WhenGivenCompletedTasks_ReturnsExpectedResults()
    {
        var tasks = new[]
        {
            ValueTask.FromResult(2),
            ValueTask.FromResult(4),
            ValueTask.FromResult(6),
        };

        var results = await tasks.WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Has.Exactly(3).Items);
            Assert.That(results[0], Is.EqualTo(2));
            Assert.That(results[1], Is.EqualTo(4));
            Assert.That(results[2], Is.EqualTo(6));
        }
    }

    [Test]
    public static async Task WhenAll_WhenGivenIncompleteTasks_ReturnsExpectedResults()
    {
        var tasks = new[]
        {
            Incomplete(2),
            Incomplete(4),
            Incomplete(6),
        };

        var results = await tasks.WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Has.Exactly(3).Items);
            Assert.That(results[0], Is.EqualTo(2));
            Assert.That(results[1], Is.EqualTo(4));
            Assert.That(results[2], Is.EqualTo(6));
        }
    }

    private static async ValueTask<T> Incomplete<T>(T value)
    {
        await Task.Yield();

        return value;
    }
}
