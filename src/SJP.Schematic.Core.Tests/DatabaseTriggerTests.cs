using System;
using NUnit.Framework;
using Moq;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal class DatabaseTriggerTests
    {
        [Test]
        public void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(table, null, definition, timing, events, true));
        }

        [Test]
        public void Ctor_GivenNameMissingLocalIdentifier_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var triggerName = new SchemaIdentifier("test_trigger");
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, null, timing, events, true));
        }

        [Test]
        public void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            var definition = string.Empty;
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "          ";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = (TriggerQueryTiming)55;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = (TriggerEvent)55;

            Assert.Throws<ArgumentException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.None;

            Assert.Throws<ArgumentException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(table, trigger.Table);
        }

        [Test]
        public void Name_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public void Definition_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(definition, trigger.Definition);
        }

        [Test]
        public void QueryTiming_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(timing, trigger.QueryTiming);
        }

        [Test]
        public void TriggerEvent_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(events, trigger.TriggerEvent);
        }

        [Test]
        public void IsEnabled_WhenTrueProvidedInCtor_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.IsTrue(trigger.IsEnabled);
        }

        [Test]
        public void IsEnabled_WhenFalseProvidedInCtor_ReturnsFalse()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, false);

            Assert.IsFalse(trigger.IsEnabled);
        }
    }
}
