using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal class PostgreSqlDatabaseTriggerTests
    {
        [Test]
        public void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTrigger(null, triggerName, definition, timing, events, enabled));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTrigger(table, null, definition, timing, events, enabled));
        }

        [Test]
        public void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTrigger(null, triggerName, null, timing, events, enabled));
        }

        [Test]
        public void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            var definition = string.Empty;
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTrigger(null, triggerName, definition, timing, events, enabled));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "          ";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTrigger(null, triggerName, definition, timing, events, enabled));
        }

        [Test]
        public void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = (TriggerQueryTiming)55;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new PostgreSqlDatabaseTrigger(null, triggerName, definition, timing, events, enabled));
        }

        [Test]
        public void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = (TriggerEvent)55;
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new PostgreSqlDatabaseTrigger(null, triggerName, definition, timing, events, enabled));
        }

        [Test]
        public void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.None;
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new PostgreSqlDatabaseTrigger(null, triggerName, definition, timing, events, enabled));
        }

        [Test]
        public void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new PostgreSqlDatabaseTrigger(table, triggerName, definition, timing, events, enabled);

            Assert.AreEqual(table, trigger.Table);
        }

        [Test]
        public void Name_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new PostgreSqlDatabaseTrigger(table, triggerName, definition, timing, events, enabled);

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public void Definition_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new PostgreSqlDatabaseTrigger(table, triggerName, definition, timing, events, enabled);

            Assert.AreEqual(definition, trigger.Definition);
        }

        [Test]
        public void QueryTiming_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new PostgreSqlDatabaseTrigger(table, triggerName, definition, timing, events, enabled);

            Assert.AreEqual(timing, trigger.QueryTiming);
        }

        [Test]
        public void TriggerEvent_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new PostgreSqlDatabaseTrigger(table, triggerName, definition, timing, events, enabled);

            Assert.AreEqual(events, trigger.TriggerEvent);
        }

        [Test]
        public void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new PostgreSqlDatabaseTrigger(table, triggerName, definition, timing, events, enabled);

            Assert.AreEqual(enabled, trigger.IsEnabled);
        }

        [Test]
        public void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = false;

            var trigger = new PostgreSqlDatabaseTrigger(table, triggerName, definition, timing, events, enabled);

            Assert.AreEqual(enabled, trigger.IsEnabled);
        }
    }
}
