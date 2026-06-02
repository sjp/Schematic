using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class TriggerWithNoEnabledEventsRuleTests
{
    private static IRelationalDatabaseTable CreateTableWithTrigger(IDatabaseTrigger trigger)
    {
        return new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            [trigger]
        );
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new TriggerWithNoEnabledEventsRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new TriggerWithNoEnabledEventsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTriggerBoundToAnEvent_ProducesNoMessages()
    {
        var rule = new TriggerWithNoEnabledEventsRule(RuleLevel.Error);
        var trigger = new DatabaseTrigger(
            "test_trigger",
            "test_definition",
            TriggerQueryTiming.After,
            TriggerEvent.Insert,
            true
        );
        var table = CreateTableWithTrigger(trigger);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTriggerWithNoEvents_ProducesMessages()
    {
        // The concrete DatabaseTrigger type rejects TriggerEvent.None at construction,
        // so IDatabaseTrigger is mocked to exercise the rule against arbitrary providers.
        var rule = new TriggerWithNoEnabledEventsRule(RuleLevel.Error);
        var trigger = Mock.Of<IDatabaseTrigger>(t => t.Name == "test_trigger" && t.TriggerEvent == TriggerEvent.None);
        var table = CreateTableWithTrigger(trigger);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
