using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

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
    public static void Name_GivenQualifiedCtorArg_PropertyGetReturnsLocalNameOnly()
    {
        var triggerName = Identifier.CreateQualifiedIdentifier("test_schema", "test_trigger");
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.That(trigger.Name, Is.EqualTo(Identifier.CreateQualifiedIdentifier("test_trigger")));
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

    [Test]
    public static void Ctor_GivenInvalidTriggerGranularity_ThrowsArgumentException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;
        const TriggerGranularity granularity = (TriggerGranularity)55;

        Assert.That(
            () => new DatabaseTrigger(triggerName, definition, timing, events, true, granularity, Option<string>.None, []),
            Throws.ArgumentException
        );
    }

    [Test]
    public static void Ctor_GivenNullUpdateColumns_ThrowsArgumentNullException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        Assert.That(
            () => new DatabaseTrigger(triggerName, definition, timing, events, true, TriggerGranularity.Row, Option<string>.None, null!),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenUpdateColumnsContainingNull_ThrowsArgumentNullException()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;
        var updateColumns = new Identifier[] { null! };

        Assert.That(
            () => new DatabaseTrigger(triggerName, definition, timing, events, true, TriggerGranularity.Row, Option<string>.None, updateColumns),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenOtherTriggerEvent_DoesNotThrow()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Insert | TriggerEvent.Truncate | TriggerEvent.Other;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.That(trigger.TriggerEvent, Is.EqualTo(events));
    }

    [Test]
    public static void Ctor_GivenShortCtor_DefaultsNewMembersToUnknownAndEmpty()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true);

        Assert.Multiple(() =>
        {
            Assert.That(trigger.Granularity, Is.EqualTo(TriggerGranularity.Unknown));
            Assert.That(trigger.Condition, OptionIs.None);
            Assert.That(trigger.UpdateColumns, Is.Empty);
        });
    }

    [Test]
    public static void Granularity_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;
        const TriggerGranularity granularity = TriggerGranularity.Statement;

        var trigger = new DatabaseTrigger(triggerName, definition, timing, events, true, granularity, Option<string>.None, []);

        Assert.That(trigger.Granularity, Is.EqualTo(granularity));
    }

    [Test]
    public static void Condition_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;
        const string condition = "new.value > 0";

        var trigger = new DatabaseTrigger(
            triggerName,
            definition,
            timing,
            events,
            true,
            TriggerGranularity.Row,
            Option<string>.Some(condition),
            []
        );

        Assert.That(trigger.Condition.UnwrapSome(), Is.EqualTo(condition));
    }

    [Test]
    public static void UpdateColumns_PropertyGet_EqualsCtorArg()
    {
        Identifier triggerName = "test_trigger";
        const string definition = "create trigger test_trigger...";
        const TriggerQueryTiming timing = TriggerQueryTiming.Before;
        const TriggerEvent events = TriggerEvent.Update;
        var updateColumns = new Identifier[] { "first_col", "second_col" };

        var trigger = new DatabaseTrigger(
            triggerName,
            definition,
            timing,
            events,
            true,
            TriggerGranularity.Row,
            Option<string>.None,
            updateColumns
        );

        Assert.That(trigger.UpdateColumns, Is.EqualTo(updateColumns));
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