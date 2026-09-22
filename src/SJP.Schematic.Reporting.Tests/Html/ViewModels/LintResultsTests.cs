using System;
using NUnit.Framework;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class LintResultsTests
{
    private const string RuleId = "SCHEMATIC0001";
    private const string RuleTitle = "Test rule";
    private const string Message = "A test message.";

    [Test]
    public static void Ctor_GivenNullLintRules_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new LintResults(null!, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("lintRules")
        );
    }

    [Test]
    public static void Ctor_GivenNullMessages_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new LintResults([], null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("messages")
        );
    }

    [Test]
    public static void LintRuleCtor_GivenNullRuleId_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new LintResults.LintRule(null!, RuleTitle, RuleLevel.Warning, 1),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleId")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void LintRuleCtor_GivenEmptyOrWhiteSpaceRuleId_ThrowsArgumentException(string ruleId)
    {
        Assert.That(
            () => new LintResults.LintRule(ruleId, RuleTitle, RuleLevel.Warning, 1),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleId")
        );
    }

    [Test]
    public static void LintRuleCtor_GivenNullRuleTitle_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new LintResults.LintRule(RuleId, null!, RuleLevel.Warning, 1),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleTitle")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void LintRuleCtor_GivenEmptyOrWhiteSpaceRuleTitle_ThrowsArgumentException(string ruleTitle)
    {
        Assert.That(
            () => new LintResults.LintRule(RuleId, ruleTitle, RuleLevel.Warning, 1),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleTitle")
        );
    }

    [Test]
    public static void LintRuleCtor_GivenInvalidRuleLevel_ThrowsArgumentException()
    {
        Assert.That(
            () => new LintResults.LintRule(RuleId, RuleTitle, (RuleLevel)55, 1),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }

    [Test]
    public static void LintMessageCtor_GivenNullRuleId_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new LintResults.LintMessage(null!, RuleTitle, RuleLevel.Warning, Message, null, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleId")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void LintMessageCtor_GivenEmptyOrWhiteSpaceRuleId_ThrowsArgumentException(string ruleId)
    {
        Assert.That(
            () => new LintResults.LintMessage(ruleId, RuleTitle, RuleLevel.Warning, Message, null, null),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleId")
        );
    }

    [Test]
    public static void LintMessageCtor_GivenNullRuleTitle_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new LintResults.LintMessage(RuleId, null!, RuleLevel.Warning, Message, null, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleTitle")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void LintMessageCtor_GivenEmptyOrWhiteSpaceRuleTitle_ThrowsArgumentException(string ruleTitle)
    {
        Assert.That(
            () => new LintResults.LintMessage(RuleId, ruleTitle, RuleLevel.Warning, Message, null, null),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ruleTitle")
        );
    }

    [Test]
    public static void LintMessageCtor_GivenNullMessage_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new LintResults.LintMessage(RuleId, RuleTitle, RuleLevel.Warning, null!, null, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("message")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void LintMessageCtor_GivenEmptyOrWhiteSpaceMessage_ThrowsArgumentException(string message)
    {
        Assert.That(
            () => new LintResults.LintMessage(RuleId, RuleTitle, RuleLevel.Warning, message, null, null),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("message")
        );
    }

    [Test]
    public static void LintMessageCtor_GivenInvalidRuleLevel_ThrowsArgumentException()
    {
        Assert.That(
            () => new LintResults.LintMessage(RuleId, RuleTitle, (RuleLevel)55, Message, null, null),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }
}
