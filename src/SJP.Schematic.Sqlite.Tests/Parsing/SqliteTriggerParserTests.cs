using System;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Parsing;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Parsing;

[TestFixture]
internal static class SqliteTriggerParserTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Parse_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        var parser = new SqliteTriggerParser();

        Assert.That(() => parser.Parse(definition), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase("create trigger trig before insert on foo begin select 1; end", TriggerQueryTiming.Before, TriggerEvent.Insert)]
    [TestCase("create trigger trig after update on foo begin select 1; end", TriggerQueryTiming.After, TriggerEvent.Update)]
    [TestCase("create trigger trig instead of delete on foo begin select 1; end", TriggerQueryTiming.InsteadOf, TriggerEvent.Delete)]
    [TestCase("create trigger trig delete on foo begin select 1; end", TriggerQueryTiming.After, TriggerEvent.Delete)]
    [TestCase("create trigger trig before update of a, b on foo begin select 1; end", TriggerQueryTiming.Before, TriggerEvent.Update)]
    [TestCase("create temp trigger if not exists main.trig after insert on foo when new.a > 1 begin select 1; end", TriggerQueryTiming.After, TriggerEvent.Insert)]
    public static void Parse_GivenTrigger_ReturnsExpectedTimingAndEvent(string definition, TriggerQueryTiming expectedTiming, TriggerEvent expectedEvent)
    {
        var parser = new SqliteTriggerParser();

        var result = parser.Parse(definition);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Timing, Is.EqualTo(expectedTiming));
            Assert.That(result.Event, Is.EqualTo(expectedEvent));
        }
    }

    [Test]
    public static void Parse_GivenTriggerWithWhenClause_ReturnsCondition()
    {
        var parser = new SqliteTriggerParser();

        var result = parser.Parse("create trigger trig after insert on foo when new.a > 1 begin select 1; end");

        Assert.That(result.Condition.UnwrapSome(), Is.EqualTo("new.a > 1"));
    }

    [Test]
    public static void Parse_GivenTriggerWithoutWhenClause_ReturnsNoCondition()
    {
        var parser = new SqliteTriggerParser();

        var result = parser.Parse("create trigger trig after insert on foo begin select 1; end");

        Assert.That(result.Condition, OptionIs.None);
    }

    [Test]
    public static void Parse_GivenTriggerWithUpdateOfColumns_ReturnsUpdateColumns()
    {
        var parser = new SqliteTriggerParser();

        var result = parser.Parse("create trigger trig before update of a, \"b c\" on foo begin select 1; end");

        Assert.That(result.UpdateColumns.Select(static c => c.LocalName), Is.EqualTo(new[] { "a", "b c" }));
    }

    [Test]
    public static void Parse_GivenTriggerWithoutUpdateOfColumns_ReturnsEmptyUpdateColumns()
    {
        var parser = new SqliteTriggerParser();

        var result = parser.Parse("create trigger trig before update on foo begin select 1; end");

        Assert.That(result.UpdateColumns, Is.Empty);
    }
}
