using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class EmptyRoutineDefinitionRuleTests
{
    // The concrete DatabaseRoutine type rejects an empty definition at construction,
    // so IDatabaseRoutine is mocked to exercise the rule against arbitrary providers.
    private static IDatabaseRoutine CreateRoutine(string definition)
    {
        return Mock.Of<IDatabaseRoutine>(r => r.Name == "test_routine" && r.Definition == definition);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new EmptyRoutineDefinitionRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
    {
        var rule = new EmptyRoutineDefinitionRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseRoutines(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithDefinition_ProducesNoMessages()
    {
        var rule = new EmptyRoutineDefinitionRule(RuleLevel.Error);
        var routine = CreateRoutine("BEGIN RETURN 1; END");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithEmptyDefinition_ProducesMessages()
    {
        var rule = new EmptyRoutineDefinitionRule(RuleLevel.Error);
        var routine = CreateRoutine("   ");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Not.Empty);
    }
}
