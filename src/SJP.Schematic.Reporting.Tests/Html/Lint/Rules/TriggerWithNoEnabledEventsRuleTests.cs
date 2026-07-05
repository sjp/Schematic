using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class TriggerWithNoEnabledEventsRuleTests
{
    private static IRelationalDatabaseTable CreateTableWithTrigger(Identifier tableName, IDatabaseTrigger trigger)
    {
        return new RelationalDatabaseTable(
            tableName,
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
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTriggerWithNoEvents_ProducesMessageWithVisibleTableName()
    {
        var rule = new TriggerWithNoEnabledEventsRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
        var trigger = Mock.Of<IDatabaseTrigger>(t => t.Name == "test_trigger" && t.TriggerEvent == TriggerEvent.None);
        var table = CreateTableWithTrigger(tableName, trigger);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
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
        var table = CreateTableWithTrigger("test", trigger);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
