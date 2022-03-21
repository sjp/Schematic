using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDatabaseTriggerTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        Assert.That(() => new PostgreSqlDatabaseTrigger(null, definition, timing, events, enabled), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentNullException(string definition)
    {
        Identifier triggerName = "test_trigger";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        Assert.That(() => new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = (TriggerQueryTiming)55;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        Assert.That(() => new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = (TriggerEvent)55;
        const bool enabled = true;

        Assert.That(() => new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.None;
        const bool enabled = true;

        Assert.That(() => new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled), Throws.ArgumentException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled);

        Assert.That(trigger.Name, Is.EqualTo(triggerName));
    }

    [Test]
    public static void Definition_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled);

        Assert.That(trigger.Definition, Is.EqualTo(definition));
    }

    [Test]
    public static void QueryTiming_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled);

        Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
    }

    [Test]
    public static void TriggerEvent_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled);

        Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
    }

    [Test]
    public static void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = true;

        var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled);

        Assert.That(trigger.IsEnabled, Is.EqualTo(enabled));
    }

    [Test]
    public static void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.InsteadOf;
        const TriggerEvent events = TriggerEvent.Update;
        const bool enabled = false;

        var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, enabled);

        Assert.That(trigger.IsEnabled, Is.EqualTo(enabled));
    }

    [TestCase("test_trigger", "Trigger: test_trigger")]
    [TestCase("test_trigger_other", "Trigger: test_trigger_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var triggerName = Identifier.CreateQualifiedIdentifier(name);
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new PostgreSqlDatabaseTrigger(triggerName, definition, timing, events, false);
        var result = trigger.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}