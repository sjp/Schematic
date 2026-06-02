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
}
