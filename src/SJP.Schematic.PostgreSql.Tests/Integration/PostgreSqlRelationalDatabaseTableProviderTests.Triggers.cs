using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
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
        var dbVersion = await Dialect.GetDatabaseVersionAsync(Connection).ConfigureAwait(false);

        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

        const string expectedOldDefinition = "EXECUTE PROCEDURE test_trigger_fn()";
        const string expectedNewDefinition = "EXECUTE FUNCTION test_trigger_fn()";
        var expectedDefinition = dbVersion < new Version(12, 0)
            ? expectedOldDefinition
            : expectedNewDefinition;

        Assert.That(trigger.Definition, Is.EqualTo(expectedDefinition));
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Insert;

        Assert.Multiple(() =>
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        });
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerForUpdate_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_2");

        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        Assert.Multiple(() =>
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        });
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerForDelete_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_3");

        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Delete;

        Assert.Multiple(() =>
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        });
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerAfterInsert_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_4");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
        const TriggerEvent events = TriggerEvent.Insert;

        Assert.Multiple(() =>
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        });
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerAfterUpdate_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_5");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
        const TriggerEvent events = TriggerEvent.Update;

        Assert.Multiple(() =>
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        });
    }

    [Test]
    public async Task Triggers_GivenTableWithTriggerAfterDelete_ReturnsCorrectEventAndTiming()
    {
        var table = await GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
        var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_6");

        const TriggerQueryTiming timing = TriggerQueryTiming.After;
        const TriggerEvent events = TriggerEvent.Delete;

        Assert.Multiple(() =>
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        });
    }
}