using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal partial class SqliteRelationalDatabaseTableTests : SqliteTest
    {
        [Test]
        public void Trigger_GivenTableWithNoTriggers_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("trigger_test_table_2");
            var triggerLookup = table.Trigger;

            Assert.Zero(triggerLookup.Count);
        }

        [Test]
        public void Trigger_GivenTableWithTrigger_ReturnsNonEmptyLookup()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;

            Assert.NotZero(triggerLookup.Count);
        }

        [Test]
        public void Trigger_GivenTableWithTrigger_ReturnsCorrectName()
        {
            Identifier triggerName = "trigger_test_table_1_trigger_1";

            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup[triggerName];

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public void Trigger_GivenTableWithTrigger_ReturnsCorrectDefinition()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup["trigger_test_table_1_trigger_1"];

            const string expectedDefinition = @"create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
begin
    select 1;
end";

            var comparer = new SqliteExpressionComparer(StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(comparer.Equals(expectedDefinition, trigger.Definition));
        }

        [Test]
        public void Trigger_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup["trigger_test_table_1_trigger_1"];

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Trigger_GivenTableWithTriggerForUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup["trigger_test_table_1_trigger_2"];

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Trigger_GivenTableWithTriggerForDelete_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup["trigger_test_table_1_trigger_3"];

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Trigger_GivenTableWithTriggerAfterInsert_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup["trigger_test_table_1_trigger_4"];

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Trigger_GivenTableWithTriggerAfterUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup["trigger_test_table_1_trigger_5"];

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Trigger_GivenTableWithTriggerAfterDelete_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var triggerLookup = table.Trigger;
            var trigger = triggerLookup["trigger_test_table_1_trigger_6"];

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Triggers_GivenTableWithNoTriggers_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("trigger_test_table_2");
            var count = table.Triggers.Count;

            Assert.Zero(count);
        }

        [Test]
        public void Triggers_GivenTableWithTrigger_ReturnsNonEmptyCollection()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var count = table.Triggers.Count;

            Assert.NotZero(count);
        }

        [Test]
        public void Triggers_GivenTableWithTrigger_ReturnsCorrectName()
        {
            Identifier triggerName = "trigger_test_table_1_trigger_1";

            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == triggerName);

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public void Triggers_GivenTableWithTrigger_ReturnsCorrectDefinition()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

            const string expectedDefinition = @"create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
begin
    select 1;
end";

            var comparer = new SqliteExpressionComparer(StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(comparer.Equals(expectedDefinition, trigger.Definition));
        }

        [Test]
        public void Triggers_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Triggers_GivenTableWithTriggerForUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_2");

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Triggers_GivenTableWithTriggerForDelete_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_3");

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Triggers_GivenTableWithTriggerAfterInsert_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_4");

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Triggers_GivenTableWithTriggerAfterUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_5");

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public void Triggers_GivenTableWithTriggerAfterDelete_ReturnsCorrectEventAndTiming()
        {
            var table = Database.GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_6");

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithNoTriggers_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("trigger_test_table_2").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);

            Assert.Zero(triggerLookup.Count);
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTrigger_ReturnsNonEmptyLookup()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);

            Assert.NotZero(triggerLookup.Count);
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTrigger_ReturnsCorrectName()
        {
            Identifier triggerName = "trigger_test_table_1_trigger_1";

            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup[triggerName];

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTrigger_ReturnsCorrectDefinition()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup["trigger_test_table_1_trigger_1"];

            const string expectedDefinition = @"create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
begin
    select 1;
end";

            var comparer = new SqliteExpressionComparer(StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(comparer.Equals(expectedDefinition, trigger.Definition));
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup["trigger_test_table_1_trigger_1"];

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTriggerForUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup["trigger_test_table_1_trigger_2"];

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTriggerForDelete_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup["trigger_test_table_1_trigger_3"];

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTriggerAfterInsert_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup["trigger_test_table_1_trigger_4"];

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTriggerAfterUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup["trigger_test_table_1_trigger_5"];

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggerAsync_GivenTableWithTriggerAfterDelete_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggerLookup = await table.TriggerAsync().ConfigureAwait(false);
            var trigger = triggerLookup["trigger_test_table_1_trigger_6"];

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithNoTriggers_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("trigger_test_table_2").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);

            var count = triggers.Count;

            Assert.Zero(count);
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTrigger_ReturnsNonEmptyCollection()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var count = table.Triggers.Count;

            Assert.NotZero(count);
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTrigger_ReturnsCorrectName()
        {
            Identifier triggerName = "trigger_test_table_1_trigger_1";

            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == triggerName);

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTrigger_ReturnsCorrectDefinition()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

            const string expectedDefinition = @"create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
begin
    select 1;
end";

            var comparer = new SqliteExpressionComparer(StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(comparer.Equals(expectedDefinition, trigger.Definition));
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTriggerForInsert_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == "trigger_test_table_1_trigger_1");

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTriggerForUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == "trigger_test_table_1_trigger_2");

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTriggerForDelete_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == "trigger_test_table_1_trigger_3");

            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTriggerAfterInsert_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == "trigger_test_table_1_trigger_4");

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Insert;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTriggerAfterUpdate_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == "trigger_test_table_1_trigger_5");

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }

        [Test]
        public async Task TriggersAsync_GivenTableWithTriggerAfterDelete_ReturnsCorrectEventAndTiming()
        {
            var table = await Database.GetTableAsync("trigger_test_table_1").ConfigureAwait(false);
            var triggers = await table.TriggersAsync().ConfigureAwait(false);
            var trigger = triggers.First(t => t.Name == "trigger_test_table_1_trigger_6");

            const TriggerQueryTiming timing = TriggerQueryTiming.After;
            const TriggerEvent events = TriggerEvent.Delete;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(timing, trigger.QueryTiming);
                Assert.AreEqual(events, trigger.TriggerEvent);
            });
        }
    }
}