using System.Collections.Generic;
using NUnit.Framework;

namespace SJP.Schematic.Reporting.Tests;

[TestFixture]
internal static class CollectionExtensionsTests
{
    [Test]
    public static void UCount_GivenNullReadOnlyCollection_ThrowsArgumentNullException()
    {
        IReadOnlyCollection<string> collection = null!;
        Assert.That(() => collection!.UCount(), Throws.ArgumentNullException);
    }

    [Test]
    public static void UCount_GivenEmptyReadOnlyCollection_ReturnsZero()
    {
        var collection = System.Array.Empty<string>();
        Assert.That(collection.UCount(), Is.Zero);
    }

    [Test]
    public static void UCount_GivenNonEmptyReadOnlyCollection_ReturnsCount()
    {
        var collection = new[] { "a", "b", "c" };
        Assert.That(collection.UCount(), Is.EqualTo(3U));
    }

    [Test]
    public static void UCount_GivenNullEnumerable_ThrowsArgumentNullException()
    {
        IEnumerable<string> collection = null!;
        Assert.That(() => collection!.UCount(), Throws.ArgumentNullException);
    }

    [Test]
    public static void UCount_GivenEmptyEnumerable_ReturnsZero()
    {
        var collection = EnumerateEmpty();
        Assert.That(collection.UCount(), Is.Zero);
    }

    [Test]
    public static void UCount_GivenNonEmptyEnumerable_ReturnsCount()
    {
        var collection = EnumerateThree();
        Assert.That(collection.UCount(), Is.EqualTo(3U));
    }

    private static IEnumerable<string> EnumerateEmpty()
    {
        yield break;
    }

    private static IEnumerable<string> EnumerateThree()
    {
        yield return "a";
        yield return "b";
        yield return "c";
    }
}
