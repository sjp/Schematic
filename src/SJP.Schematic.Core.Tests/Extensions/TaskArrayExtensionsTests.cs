using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
public static class TaskArrayExtensionsTests
{
    [Test]
    public static void WhenAll_WhenGivenNullTasks_ThrowsArgNullException()
    {
        Assert.That(() => TaskArrayExtensions.WhenAll<object>(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task WhenAll_WhenGivenValid_ReturnsExpectedResults()
    {
        var tasks = new[]
        {
            Task.FromResult(2),
            Task.FromResult(4),
            Task.FromResult(6),
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
}