using System;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
    {
        [Test]
        public void Triggers_GivenTableWithNoTriggers_ReturnsEmptyCollection()
        {
            var table = GetTable("trigger_test_table_2");
            var count = table.Triggers.Count;

            Assert.Zero(count);
        }

        [Test]
        public void Triggers_GivenTableWithTrigger_ReturnsNonEmptyCollection()
        {
            var table = GetTable("trigger_test_table_1");
            var count = table.Triggers.Count;

            Assert.NotZero(count);
        }

        [Test]
        public void Triggers_GivenTableWithTrigger_ReturnsCorrectName()
        {
            Identifier triggerName = "trigger_test_table_1_trigger_1";

            var table = GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == triggerName);

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public void Triggers_GivenTableWithTrigger_ReturnsCorrectDefinition()
        {
            var table = GetTable("trigger_test_table_1");
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
            var table = GetTable("trigger_test_table_1");
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
            var table = GetTable("trigger_test_table_1");
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
            var table = GetTable("trigger_test_table_1");
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
            var table = GetTable("trigger_test_table_1");
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
            var table = GetTable("trigger_test_table_1");
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
            var table = GetTable("trigger_test_table_1");
            var trigger = table.Triggers.First(t => t.Name == "trigger_test_table_1_trigger_6");

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