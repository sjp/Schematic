using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V12
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql12Test
    {
        [Test]
        public async Task Triggers_GivenTableWithNoTriggers_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("v12_trigger_test_table_2").ConfigureAwait(false);

            Assert.That(table.Triggers, Is.Empty);
        }

        [Test]
        public async Task Triggers_GivenTableWithTrigger_ReturnsNonEmptyCollection()
        {
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);

            Assert.That(table.Triggers, Is.Not.Empty);
        }

        [Test]
        public async Task Triggers_GivenTableWithTrigger_ReturnsCorrectName()
        {
            Identifier triggerName = "v12_trigger_test_table_1_trigger_1";

            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == triggerName);

            Assert.That(trigger.Name, Is.EqualTo(triggerName));
        }

        [Test]
        public async Task Triggers_GivenTableWithTrigger_ReturnsCorrectDefinition()
        {
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == "v12_trigger_test_table_1_trigger_1");

            const string expectedDefinition = "EXECUTE FUNCTION v12_test_trigger_fn()";

            Assert.That(trigger.Definition, Is.EqualTo(expectedDefinition));
        }

        [Test]
        public async Task Triggers_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
        {
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == "v12_trigger_test_table_1_trigger_1");

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
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == "v12_trigger_test_table_1_trigger_2");

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
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == "v12_trigger_test_table_1_trigger_3");

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
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == "v12_trigger_test_table_1_trigger_4");

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
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == "v12_trigger_test_table_1_trigger_5");

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
            var table = await GetTableAsync("v12_trigger_test_table_1").ConfigureAwait(false);
            var trigger = table.Triggers.First(t => t.Name == "v12_trigger_test_table_1_trigger_6");

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
                Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
            });
        }
    }
}