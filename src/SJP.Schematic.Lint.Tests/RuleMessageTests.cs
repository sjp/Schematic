using NUnit.Framework;

namespace SJP.Schematic.Lint.Tests;

[TestFixture]
internal static class RuleMessageTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceId_ThrowsArgumentNullException(string ruleId)
    {
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";
        Assert.That(() => new RuleMessage(ruleId, title, level, message), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceTitle_ThrowsArgumentNullException(string title)
    {
        const string ruleId = "TEST_ID";
        const RuleLevel level = RuleLevel.Error;
        const string message = "message";
        Assert.That(() => new RuleMessage(ruleId, title, level, message), Throws.ArgumentNullException);
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
    public static void Ctor_GivenNullOrWhiteSpaceMessage_ThrowsArgumentNullException(string message)
    {
        const string ruleId = "TEST_ID";
        const string title = "title";
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new RuleMessage(ruleId, title, level, message), Throws.ArgumentNullException);
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
}