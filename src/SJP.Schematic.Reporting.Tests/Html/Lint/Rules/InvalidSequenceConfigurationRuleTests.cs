using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class InvalidSequenceConfigurationRuleTests
{
    private static IDatabaseSequence CreateSequence(Identifier name, decimal start, decimal increment, Option<decimal> minValue, Option<decimal> maxValue, bool cycle)
    {
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(s => s.Name).Returns(name);
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
        Assert.That(() => rule.AnalyseSequences(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseSequences_GivenZeroIncrement_ProducesMessageWithVisibleSequenceName()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_sequence");
        var sequence = CreateSequence(sequenceName, 1, 0, Option<decimal>.None, Option<decimal>.None, cycle: false);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_sequence"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseSequences_GivenValidSequence_ProducesNoMessages()
    {
        var rule = new InvalidSequenceConfigurationRule(RuleLevel.Error);
        var sequence = CreateSequence("test_sequence", 1, 1, Option<decimal>.Some(1), Option<decimal>.Some(100), cycle: true);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Empty);
    }
}
