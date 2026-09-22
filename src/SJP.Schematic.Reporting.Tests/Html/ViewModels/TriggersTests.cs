using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class TriggersTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
    private const string TableUrl = "#/tables/test_table";

    [Test]
    public static void Ctor_GivenNullTriggers_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Triggers(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggers")
        );
    }

    [Test]
    public static void Ctor_GivenTriggersContainingNull_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Triggers([null!]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggers")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenNullObjectName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Triggers.TriggerRow(null!, TableUrl, "test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("objectName")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenNullTriggerName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Triggers.TriggerRow(TableName, TableUrl, null!, "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerName")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenNullUpdateColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Triggers.TriggerRow(TableName, TableUrl, "test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("updateColumns")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenNullObjectUrl_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Triggers.TriggerRow(TableName, null!, "test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("objectUrl")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Triggers.TriggerRow(TableName, TableUrl, "test_trigger", null!, TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenInvalidTriggerEvent_ThrowsArgumentException()
    {
        Assert.That(
            () => new Triggers.TriggerRow(TableName, TableUrl, "test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, (TriggerEvent)55, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerEvent")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenNoTriggerEvents_ThrowsArgumentException()
    {
        Assert.That(
            () => new Triggers.TriggerRow(TableName, TableUrl, "test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.None, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerEvent")
        );
    }

    [Test]
    public static void TriggerRowCtor_GivenUndefinedTriggerEventFlag_DoesNotSilentlyDropIt()
    {
        Assert.That(
            () => new Triggers.TriggerRow(TableName, TableUrl, "test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert | (TriggerEvent)64, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerEvent")
        );
    }
}
