using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseTriggerTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTrigger(null, definition, timing, events, enabled));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTrigger(triggerName, null, timing, events, enabled));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            var definition = string.Empty;
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "          ";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled));
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = (TriggerQueryTiming)55;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled));
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = (TriggerEvent)55;
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled));
        }

        [Test]
        public static void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.None;
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled);

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled);

            Assert.AreEqual(definition, trigger.Definition);
        }

        [Test]
        public static void QueryTiming_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled);

            Assert.AreEqual(timing, trigger.QueryTiming);
        }

        [Test]
        public static void TriggerEvent_PropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled);

            Assert.AreEqual(events, trigger.TriggerEvent);
        }

        [Test]
        public static void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = true;

            var trigger = new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled);

            Assert.AreEqual(enabled, trigger.IsEnabled);
        }

        [Test]
        public static void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
            const TriggerEvent events = TriggerEvent.Update;
            const bool enabled = false;

            var trigger = new SqlServerDatabaseTrigger(triggerName, definition, timing, events, enabled);

            Assert.AreEqual(enabled, trigger.IsEnabled);
        }
    }
}
