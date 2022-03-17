using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class VerbatimIdentifierResolutionStrategyTests
{
    [Test]
    public static void GetResolutionOrder_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var resolver = new VerbatimIdentifierResolutionStrategy();
        Assert.That(() => resolver.GetResolutionOrder(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetResolutionOrder_GivenIdentifier_ReturnsIdenticalIdentifier()
    {
        var resolver = new VerbatimIdentifierResolutionStrategy();
        var input = new Identifier("A", "B", "C", "D");

        var results = resolver.GetResolutionOrder(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Exactly(1).Items);
            Assert.That(results[0], Is.EqualTo(input));
        });
    }
}
