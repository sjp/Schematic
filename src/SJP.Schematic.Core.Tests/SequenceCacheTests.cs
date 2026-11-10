using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

internal static class SequenceCacheTests
{
    [Test]
    public static void Equals_GivenSizedCachesOfSameSize_ReturnsTrue()
    {
        SequenceCache first = new SequenceCache.Sized(20);
        SequenceCache second = new SequenceCache.Sized(20);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public static void Equals_GivenSizedCachesOfDifferentSizes_ReturnsFalse()
    {
        SequenceCache first = new SequenceCache.Sized(20);
        SequenceCache second = new SequenceCache.Sized(10);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public static void Equals_GivenSameCaseWithoutSize_ReturnsTrue()
    {
        SequenceCache first = new SequenceCache.EngineDefault();
        SequenceCache second = new SequenceCache.EngineDefault();

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public static void Equals_GivenDifferentCasesWithoutSize_ReturnsFalse()
    {
        SequenceCache first = new SequenceCache.Unknown();
        SequenceCache second = new SequenceCache.None();

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public static void Size_PropertyGet_MatchesCtorArg()
    {
        var cache = new SequenceCache.Sized(20);

        Assert.That(cache.Size, Is.EqualTo(20));
    }

    [Test]
    public static void ToString_WhenSized_ContainsSize()
    {
        SequenceCache cache = new SequenceCache.Sized(20);

        Assert.That(cache.ToString(), Does.Contain("20"));
    }

    [Test]
    public static void ToString_WhenDefault_ReturnsEmptyString()
    {
        SequenceCache cache = default;

        Assert.That(cache.ToString(), Is.Empty);
    }
}
