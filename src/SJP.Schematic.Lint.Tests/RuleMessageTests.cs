using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests;

[TestFixture]
internal static class RuleMessageTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceId_ThrowsArgumentException(string ruleId)
    {
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";
        Assert.That(() => new RuleMessage(ruleId, title, level, message), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceTitle_ThrowsArgumentException(string title)
    {
        const string ruleId = "TEST_ID";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";
        Assert.That(() => new RuleMessage(ruleId, title, level, message), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void Ctor_GivenInvalidRuleLevel_ThrowsArgumentException()
    {
        const string ruleId = "TEST_ID";
        const string title = "title";
        const RuleLevel level = (RuleLevel)999;
        const string message = "message";
        Assert.That(() => new RuleMessage(ruleId, title, level, message), Throws.ArgumentException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceMessage_ThrowsArgumentException(string message)
    {
        const string ruleId = "TEST_ID";
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new RuleMessage(ruleId, title, level, message), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void RuleId_PropertyGet_MatchesCtorArg()
    {
        const string ruleId = "TEST_ID";
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";

        var ruleMessage = new RuleMessage(ruleId, title, level, message);

        Assert.That(ruleMessage.RuleId, Is.EqualTo(ruleId));
    }

    [Test]
    public static void Title_PropertyGet_MatchesCtorArg()
    {
        const string ruleId = "TEST_ID";
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";

        var ruleMessage = new RuleMessage(ruleId, title, level, message);

        Assert.That(ruleMessage.Title, Is.EqualTo(title));
    }

    [Test]
    public static void Level_PropertyGet_MatchesCtorArg()
    {
        const string ruleId = "TEST_ID";
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";

        var ruleMessage = new RuleMessage(ruleId, title, level, message);

        Assert.That(ruleMessage.Level, Is.EqualTo(level));
    }

    [Test]
    public static void Message_PropertyGet_MatchesCtorArg()
    {
        const string ruleId = "TEST_ID";
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";

        var ruleMessage = new RuleMessage(ruleId, title, level, message);

        Assert.That(ruleMessage.Message, Is.EqualTo(message));
    }

    [Test]
    public static void ObjectName_GivenNoObjectName_IsNone()
    {
        var message = new RuleMessage("TEST_ID", "title", RuleLevel.Error, "message");

        Assert.That(message.ObjectName.IsNone, Is.True);
    }

    [Test]
    public static void ObjectName_GivenObjectName_IsThatName()
    {
        var objectName = Identifier.CreateQualifiedIdentifier("main", "test_table");
        var message = new RuleMessage("TEST_ID", "title", RuleLevel.Error, "message", objectName);

        Assert.That(message.ObjectName.UnwrapSome(), Is.EqualTo(objectName));
    }
}
