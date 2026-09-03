using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseSequenceTests
{
    private static readonly IDbType SequenceType = TestDbTypes.BigInteger;

    private static DatabaseSequence CreateSequence(
        Identifier sequenceName,
        decimal start,
        decimal increment,
        Option<decimal> minValue,
        Option<decimal> maxValue,
        SequenceCacheMode cacheMode = SequenceCacheMode.None,
        Option<int> cacheSize = default,
        bool cycle = true,
        bool isOrdered = true
    ) => new(sequenceName, SequenceType, start, increment, minValue, maxValue, cycle, cacheMode, cacheSize, isOrdered);

    [Test]
    public static void Ctor_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(() => CreateSequence(null, 1, 1, Option<decimal>.None, Option<decimal>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullType_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseSequence("test", null, 1, 1, Option<decimal>.None, Option<decimal>.None, true, SequenceCacheMode.None, Option<int>.None, true),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenInvalidCacheMode_ThrowsArgException()
    {
        const SequenceCacheMode cacheMode = (SequenceCacheMode)55;
        Assert.That(
            () => new DatabaseSequence("test", SequenceType, 1, 1, Option<decimal>.None, Option<decimal>.None, true, cacheMode, Option<int>.None, true),
            Throws.ArgumentException
        );
    }

    [Test]
    public static void Ctor_GivenZeroIncrement_ThrowsArgException()
    {
        Assert.That(() => CreateSequence("test", 1, 0, Option<decimal>.None, Option<decimal>.None), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenPositiveIncrementAndMinValueLargerThanStart_ThrowsArgumentException()
    {
        Assert.That(() => CreateSequence("test", 1, 1, Option<decimal>.Some(2), Option<decimal>.None), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenPositiveIncrementAndMaxValueLessThanStart_ThrowsArgumentException()
    {
        Assert.That(() => CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.Some(-1)), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNegativeIncrementAndMinValueLargerThanStart_ThrowsArgumentException()
    {
        Assert.That(() => CreateSequence("test", 1, -1, Option<decimal>.Some(2), Option<decimal>.None), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNegativeIncrementAndMaxValueLessThanStart_ThrowsArgumentException()
    {
        Assert.That(() => CreateSequence("test", 1, -1, Option<decimal>.None, Option<decimal>.Some(-1)), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNegativeIncrementAndStartingAtMaxValue_DoesNotThrow()
    {
        Assert.That(() => CreateSequence("test", 100, -1, Option<decimal>.Some(1), Option<decimal>.Some(100)), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenNegativeIncrementAndStartWithinMinAndMaxValues_DoesNotThrow()
    {
        Assert.That(() => CreateSequence("test", 50, -1, Option<decimal>.Some(1), Option<decimal>.Some(100)), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenPositiveIncrementAndStartWithinMinAndMaxValues_DoesNotThrow()
    {
        Assert.That(() => CreateSequence("test", 50, 1, Option<decimal>.Some(1), Option<decimal>.Some(100)), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenCacheSizeWithoutASizedCache_DiscardsCacheSize()
    {
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None, SequenceCacheMode.EngineDefault, Option<int>.Some(20));

        Assert.That(sequence.CacheSize, Is.EqualTo(Option<int>.None));
    }

    [Test]
    public static void Name_PropertyGet_MatchesCtorArg()
    {
        var sequenceName = new Identifier("test");
        var sequence = CreateSequence(sequenceName, 1, 1, Option<decimal>.None, Option<decimal>.None);

        Assert.That(sequence.Name, Is.EqualTo(sequenceName));
    }

    [Test]
    public static void Type_PropertyGet_MatchesCtorArg()
    {
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None);

        Assert.That(sequence.Type, Is.EqualTo(SequenceType));
    }

    [Test]
    public static void CacheMode_PropertyGet_MatchesCtorArg()
    {
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None, SequenceCacheMode.EngineDefault);

        Assert.That(sequence.CacheMode, Is.EqualTo(SequenceCacheMode.EngineDefault));
    }

    [Test]
    public static void CacheSize_PropertyGet_MatchesCtorArg()
    {
        const int cacheSize = 20;
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None, SequenceCacheMode.Sized, Option<int>.Some(cacheSize));

        Assert.That(sequence.CacheSize.UnwrapSome(), Is.EqualTo(cacheSize));
    }

    [Test]
    public static void Cycle_PropertyGet_MatchesCtorArg()
    {
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None);

        Assert.That(sequence.Cycle, Is.True);
    }

    [Test]
    public static void IsOrdered_PropertyGet_MatchesCtorArg()
    {
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None, isOrdered: false);

        Assert.That(sequence.IsOrdered, Is.False);
    }

    [Test]
    public static void Increment_PropertyGet_MatchesCtorArg()
    {
        const int increment = 100;
        var sequence = CreateSequence("test", 1, increment, Option<decimal>.None, Option<decimal>.None);

        Assert.That(sequence.Increment, Is.EqualTo(increment));
    }

    [Test]
    public static void MaxValue_PropertyGet_MatchesCtorArg()
    {
        const int maxValue = 100;
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.Some(maxValue));

        Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(maxValue));
    }

    [Test]
    public static void MinValue_PropertyGet_MatchesCtorArg()
    {
        const int minValue = 100;
        var sequence = CreateSequence("test", minValue, 1, Option<decimal>.Some(minValue), Option<decimal>.None);

        Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(minValue));
    }

    [Test]
    public static void Start_PropertyGet_MatchesCtorArg()
    {
        const int start = 100;
        var sequence = CreateSequence("test", start, 1, Option<decimal>.None, Option<decimal>.None);

        Assert.That(sequence.Start, Is.EqualTo(start));
    }

    [TestCase("", "test_sequence", "Sequence: test_sequence")]
    [TestCase("test_schema", "test_sequence", "Sequence: test_schema.test_sequence")]
    public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
    {
        var sequenceName = Identifier.CreateQualifiedIdentifier(schema, localName);
        var sequence = CreateSequence(sequenceName, 1, 1, Option<decimal>.None, Option<decimal>.None);

        var result = sequence.ToString();

        Assert.That(result, Is.EqualTo(expectedOutput));
    }
}
