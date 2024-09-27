﻿using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseTriggerTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        Assert.That(() => new DatabaseTrigger(null, definition, timing, events, true), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Identifier triggerName = "test_trigger";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        Assert.That(() => new DatabaseTrigger(triggerName, null!, timing, events, true), Throws.ArgumentNullException);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Identifier triggerName = "test_trigger";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        Assert.That(() => new DatabaseTrigger(triggerName, definition, timing, events, true), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = (TriggerQueryTiming)55;
        const TriggerEvent events = TriggerEvent.Update;

        Assert.That(() => new DatabaseTrigger(triggerName, definition, timing, events, true), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = (TriggerEvent)55;

        Assert.That(() => new DatabaseTrigger(triggerName, definition, timing, events, true), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.None;

        Assert.That(() => new DatabaseTrigger(triggerName, definition, timing, events, true), Throws.ArgumentException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.That(trigger.Name, Is.EqualTo(triggerName));
    }

    [Test]
    public static void Definition_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.That(trigger.Definition, Is.EqualTo(definition));
    }

    [Test]
    public static void QueryTiming_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.That(trigger.QueryTiming, Is.EqualTo(timing));
    }

    [Test]
    public static void TriggerEvent_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
    }

    [Test]
    public static void IsEnabled_WhenTrueProvidedInCtor_ReturnsTrue()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.That(trigger.IsEnabled, Is.True);
    }

    [Test]
    public static void IsEnabled_WhenFalseProvidedInCtor_ReturnsFalse()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, false);

        Assert.That(trigger.IsEnabled, Is.False);
    }

    [TestCase("test_trigger", "Trigger: test_trigger")]
    [TestCase("test_trigger_other", "Trigger: test_trigger_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var triggerName = Identifier.CreateQualifiedIdentifier(name);
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, false);
        var result = trigger.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}