using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class SelectStarInViewDefinitionRuleTests
{
    private static IDatabaseView CreateView(string definition)
    {
        return new DatabaseView("test_view", definition, new List<IDatabaseColumn>());
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new SelectStarInViewDefinitionRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseViews(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithExplicitColumns_ProducesNoMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT id, name FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingStar_ProducesMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT * FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingQualifiedStar_ProducesMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT t.* FROM source_table t");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingQualifiedStarMidSelectList_ProducesMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT a.id, b.* FROM a INNER JOIN b ON a.id = b.a_id");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingMultiPartQualifiedStar_ProducesMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT [source_db].[dbo].[source_table].* FROM [source_db].[dbo].[source_table]");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingBareStarMidSelectList_ProducesMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT id, * FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingStarWithParenthesisedTop_ProducesMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT TOP (10) * FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingStarWithAllQuantifier_ProducesMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT ALL * FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewUsingCountStar_ProducesNoMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT COUNT(*) AS record_count FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewUsingCountStarAfterAnotherColumn_ProducesNoMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT id, COUNT(*) AS record_count FROM source_table GROUP BY id");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewUsingSumStar_ProducesNoMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT sum(*) AS total FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithExplicitColumnsFromMultipleTables_ProducesNoMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("SELECT a.id, b.name, a.value * b.multiplier FROM a INNER JOIN b ON a.id = b.a_id");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }
}
