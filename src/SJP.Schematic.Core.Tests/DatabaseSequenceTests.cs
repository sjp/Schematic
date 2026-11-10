using System;
using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

internal static class DatabaseSequenceTests
{
    private static readonly IDbType SequenceType = TestDbTypes.BigInteger;

    private static DatabaseSequence CreateSequence(
        Identifier sequenceName,
        decimal start,
        decimal increment,
        Option<decimal> minValue,
        Option<decimal> maxValue,
        SequenceCache? cache = null,
        bool cycle = true,
        bool isOrdered = true
    ) => new(sequenceName, SequenceType, start, increment, minValue, maxValue, cycle, cache ?? new SequenceCache.None(), isOrdered);

    [Test]
    public static void Ctor_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => CreateSequence(null, 1, 1, Option<decimal>.None, Option<decimal>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceName")
        );
    }

    [Test]
    public static void Ctor_GivenNullType_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseSequence("test", null, 1, 1, Option<decimal>.None, Option<decimal>.None, true, new SequenceCache.None(), true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("type")
        );
    }

    [Test]
    public static void Ctor_GivenDefaultCache_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseSequence("test", SequenceType, 1, 1, Option<decimal>.None, Option<decimal>.None, true, default, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("cache")
        );
    }

    [Test]
    public static void Ctor_GivenZeroIncrement_ThrowsArgException()
    {
        Assert.That(
            () => CreateSequence("test", 1, 0, Option<decimal>.None, Option<decimal>.None),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("increment")
        );
    }

    [Test]
    public static void Ctor_GivenPositiveIncrementAndMinValueLargerThanStart_ThrowsArgumentException()
    {
        Assert.That(
            () => CreateSequence("test", 1, 1, Option<decimal>.Some(2), Option<decimal>.None),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("minValue")
        );
    }

    [Test]
    public static void Ctor_GivenPositiveIncrementAndMaxValueLessThanStart_ThrowsArgumentException()
    {
        Assert.That(
            () => CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.Some(-1)),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("maxValue")
        );
    }

    [Test]
    public static void Ctor_GivenNegativeIncrementAndMinValueLargerThanStart_ThrowsArgumentException()
    {
        Assert.That(
            () => CreateSequence("test", 1, -1, Option<decimal>.Some(2), Option<decimal>.None),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("minValue")
        );
    }

    [Test]
    public static void Ctor_GivenNegativeIncrementAndMaxValueLessThanStart_ThrowsArgumentException()
    {
        Assert.That(
            () => CreateSequence("test", 1, -1, Option<decimal>.None, Option<decimal>.Some(-1)),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("maxValue")
        );
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

    private static IEnumerable<TestCaseData> Caches()
    {
        yield return new TestCaseData((SequenceCache)new SequenceCache.Unknown()).SetArgDisplayNames("Unknown");
        yield return new TestCaseData((SequenceCache)new SequenceCache.None()).SetArgDisplayNames("None");
        yield return new TestCaseData((SequenceCache)new SequenceCache.Sized(20)).SetArgDisplayNames("Sized");
        yield return new TestCaseData((SequenceCache)new SequenceCache.EngineDefault()).SetArgDisplayNames("EngineDefault");
    }

    [TestCaseSource(nameof(Caches))]
    public static void Cache_PropertyGet_MatchesCtorArg(SequenceCache cache)
    {
        var sequence = CreateSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None, cache);

        Assert.That(sequence.Cache.Value, Is.SameAs(cache.Value));
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
