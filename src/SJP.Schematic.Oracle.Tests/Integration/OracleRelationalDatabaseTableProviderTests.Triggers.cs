using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    [Test]
    public async Task Triggers_GivenTableWithNoTriggers_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("trigger_test_table_2").ConfigureAwait(false);

        Assert.That(table.Triggers, Is.Empty);
    }

    [Test]
    public async Task Triggers_GivenTableWithTrigger_ReturnsNonEmptyCollection()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);

        Assert.That(table.Triggers, Is.Not.Empty);
    }

    [Test]
    public async Task Triggers_GivenTableWithTrigger_ReturnsCorrectName()
    {
        const string triggerName = "TRIGGER_TEST_TABLE_1_TRIGGER_1";

        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == triggerName);

        Assert.That(trigger.Name.LocalName, Is.EqualTo(triggerName));
    }

    [Test]
    public async Task Triggers_GivenTableWithTrigger_ReturnsCorrectDefinition()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "TRIGGER_TEST_TABLE_1_TRIGGER_1");

        const string expectedDefinition = @"begin
    null;
end;
";

        Assert.That(trigger.Definition, Is.EqualTo(expectedDefinition));
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "TRIGGER_TEST_TABLE_1_TRIGGER_1");

        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Insert;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerForUpdate_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "TRIGGER_TEST_TABLE_1_TRIGGER_2");

        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerForDelete_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "TRIGGER_TEST_TABLE_1_TRIGGER_3");

        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Delete;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerAfterInsert_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "TRIGGER_TEST_TABLE_1_TRIGGER_4");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
        const TriggerEvent events = TriggerEvent.Insert;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerAfterUpdate_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "TRIGGER_TEST_TABLE_1_TRIGGER_5");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
        const TriggerEvent events = TriggerEvent.Update;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerAfterDelete_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "TRIGGER_TEST_TABLE_1_TRIGGER_6");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
        const TriggerEvent events = TriggerEvent.Delete;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }
}