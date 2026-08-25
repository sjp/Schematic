using System;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Serialization;

namespace SJP.Schematic.Lint.Tests.Serialization;

[TestFixture]
internal static class SarifLintReportTests
{
    [Test]
    public static void Create_GivenNullResults_ThrowsArgumentNullException()
    {
        Assert.That(() => SarifLintReport.Create(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Create_GivenNoResults_ReturnsRunWithNoResults()
    {
        var log = SarifLintReport.Create([]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(log.Version, Is.EqualTo("2.1.0"));
            Assert.That(log.Runs, Has.Count.EqualTo(1));
            Assert.That(log.Runs[0].Results, Is.Empty);
            Assert.That(log.Runs[0].Tool.Driver.Rules, Is.Empty);
        }
    }

    [Test]
    public static void Create_GivenResultsFromOneRule_CatalogsThatRuleOnce()
    {
        IRuleMessage[] messages =
        [
            new RuleMessage("TEST_ID", "title", RuleLevel.Warning, "first"),
            new RuleMessage("TEST_ID", "title", RuleLevel.Warning, "second"),
        ];

        var log = SarifLintReport.Create(messages);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(log.Runs[0].Tool.Driver.Rules, Has.Count.EqualTo(1));
            Assert.That(log.Runs[0].Results, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public static void Create_GivenUnorderedResults_OrdersThemDeterministically()
    {
        IRuleMessage[] messages =
        [
            new RuleMessage("TEST_B", "title", RuleLevel.Warning, "zebra"),
            new RuleMessage("TEST_A", "title", RuleLevel.Warning, "beta"),
            new RuleMessage("TEST_A", "title", RuleLevel.Warning, "alpha"),
        ];

        var log = SarifLintReport.Create(messages);
        var texts = log.Runs[0].Results.Select(static r => r.Message.Text).ToList();

        Assert.That(texts, Is.EqualTo(new[] { "alpha", "beta", "zebra" }));
    }

    [Test]
    public static void Create_GivenResultWithObjectName_EmitsALogicalLocation()
    {
        var objectName = Identifier.CreateQualifiedIdentifier("main", "test_table");
        IRuleMessage[] messages = [new RuleMessage("TEST_ID", "title", RuleLevel.Error, "a message", objectName)];

        var log = SarifLintReport.Create(messages);
        var location = log.Runs[0].Results[0].Locations[0].LogicalLocations[0];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(location.Name, Is.EqualTo("test_table"));
            Assert.That(location.FullyQualifiedName, Is.EqualTo("main.test_table"));
        }
    }

    [Test]
    public static void Create_GivenResultWithoutObjectName_EmitsNoLocation()
    {
        IRuleMessage[] messages = [new RuleMessage("TEST_ID", "title", RuleLevel.Error, "a schema-wide message")];

        var log = SarifLintReport.Create(messages);

        Assert.That(log.Runs[0].Results[0].Locations, Is.Null);
    }

    [TestCase(RuleLevel.Information, "note")]
    [TestCase(RuleLevel.Warning, "warning")]
    [TestCase(RuleLevel.Error, "error")]
    public static void ToSarifLevel_GivenKnownLevel_ReturnsSarifEquivalent(RuleLevel level, string expected)
    {
        Assert.That(SarifLintReport.ToSarifLevel(level), Is.EqualTo(expected));
    }

    [Test]
    public static void ToSarifLevel_GivenUnknownLevel_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(() => SarifLintReport.ToSarifLevel((RuleLevel)999), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }
}
