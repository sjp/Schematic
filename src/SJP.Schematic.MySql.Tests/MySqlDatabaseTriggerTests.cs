using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlDatabaseTriggerTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new MySqlDatabaseTrigger(null, definition, timing, events), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentNullException(string definition)
        {
            Identifier triggerName = "test_trigger";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new MySqlDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = (TriggerQueryTiming)55;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.That(() => new MySqlDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = (TriggerEvent)55;

            Assert.That(() => new MySqlDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.None;

            Assert.That(() => new MySqlDatabaseTrigger(triggerName, definition, timing, events), Throws.ArgumentException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new MySqlDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.Name, Is.EqualTo(triggerName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new MySqlDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.Definition, Is.EqualTo(definition));
        }

        [Test]
        public static void QueryTiming_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new MySqlDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
        }

        [Test]
        public static void TriggerEvent_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new MySqlDatabaseTrigger(triggerName, definition, timing, events);

            Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
        }
    }
}
