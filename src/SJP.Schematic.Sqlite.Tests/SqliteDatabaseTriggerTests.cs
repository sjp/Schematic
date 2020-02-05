using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDatabaseTriggerTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new SqliteDatabaseTrigger(null, definition, timing, events), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new SqliteDatabaseTrigger(triggerName, null, timing, events), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            var definition = string.Empty;
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new SqliteDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "          ";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new SqliteDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = (TriggerQueryTiming)55;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new SqliteDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = (TriggerEvent)55;

            Assert.That(() => new SqliteDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.None;

            Assert.That(() => new SqliteDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new SqliteDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.Name, Is.EqualTo(triggerName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new SqliteDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.Definition, Is.EqualTo(definition));
        }

        [Test]
        public static void QueryTiming_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new SqliteDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
        }

        [Test]
        public static void TriggerEvent_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new SqliteDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new SqliteDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.IsEnabled, Is.True);
        }
    }
}
