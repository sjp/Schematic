using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class InvalidSequenceConfigurationRuleTests
{
    // The concrete DatabaseSequence type validates most of these states at construction,
    // so IDatabaseSequence is mocked to exercise the rule against arbitrary providers.
    private static IDatabaseSequence CreateSequence(decimal start, decimal increment, Option<decimal> minValue, Option<decimal> maxValue, bool cycle)
    {
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(s => s.Name).Returns("test_sequence");
        sequence.Setup(s => s.Start).Returns(start);
        sequence.Setup(s => s.Increment).Returns(increment);
        sequence.Setup(s => s.MinValue).Returns(minValue);
        sequence.Setup(s => s.MaxValue).Returns(maxValue);
        sequence.Setup(s => s.Cycle).Returns(cycle);
        sequence.Setup(s => s.Cache).Returns(0);
        return sequence.Object;
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new InvalidSequenceConfigurationRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseSequences(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseSequences_GivenValidSequence_ProducesNoMessages()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequence = CreateSequence(1, 1, Option<decimal>.Some(1), Option<decimal>.Some(100), cycle: true);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenZeroIncrement_ProducesMessages()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequence = CreateSequence(1, 0, Option<decimal>.None, Option<decimal>.None, cycle: false);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenMinValueGreaterThanMaxValue_ProducesMessages()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequence = CreateSequence(50, 1, Option<decimal>.Some(100), Option<decimal>.Some(10), cycle: false);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenStartBelowMinValue_ProducesMessages()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequence = CreateSequence(1, 1, Option<decimal>.Some(10), Option<decimal>.Some(100), cycle: false);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenStartAboveMaxValue_ProducesMessages()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequence = CreateSequence(200, 1, Option<decimal>.Some(1), Option<decimal>.Some(100), cycle: false);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenCycleWithoutBounds_ProducesMessages()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequence = CreateSequence(1, 1, Option<decimal>.None, Option<decimal>.None, cycle: true);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
    }
}
