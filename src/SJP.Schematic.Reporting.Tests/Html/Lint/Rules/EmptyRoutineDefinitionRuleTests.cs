using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class EmptyRoutineDefinitionRuleTests
{
    private static IDatabaseRoutine CreateRoutine(Identifier name, string definition)
    {
        return Mock.Of<IDatabaseRoutine>(r => r.Name == name && r.Definition == definition);
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
        Assert.That(() => rule.AnalyseRoutines(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithEmptyDefinition_ProducesMessageWithVisibleRoutineName()
    {
        var rule = new EmptyRoutineDefinitionRule(RuleLevel.Error);
        var routineName = Identifier.CreateQualifiedIdentifier("test_schema", "test_routine");
        var routine = CreateRoutine(routineName, "   ");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_routine"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithDefinition_ProducesNoMessages()
    {
        var rule = new EmptyRoutineDefinitionRule(RuleLevel.Error);
        var routine = CreateRoutine("test_routine", "BEGIN RETURN 1; END");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Empty);
    }
}
