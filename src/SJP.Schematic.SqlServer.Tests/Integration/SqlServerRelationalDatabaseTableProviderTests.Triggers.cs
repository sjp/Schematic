using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
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
        Identifier triggerName = "trigger_test_table_1_trigger_1";

        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == triggerName);

        Assert.That(trigger.Name, Is.EqualTo(triggerName));
    }

    [Test]
    public async Task Triggers_GivenTableWithTrigger_ReturnsCorrectDefinition()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

        const string expectedDefinition = @"
create trigger trigger_test_table_1_trigger_1
on trigger_test_table_1
for insert
as
begin
    declare @test int
end
";

        Assert.That(trigger.Definition, Is.EqualTo(expectedDefinition));
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
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
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_2");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
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
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_3");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
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
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_4");

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
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_5");

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
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_6");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
        const TriggerEvent events = TriggerEvent.Delete;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerInsteadOfInsert_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_7");

        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Insert;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerInsteadOfUpdate_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_8");

        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerInsteadOfDelete_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_9");

        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Delete;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }
}