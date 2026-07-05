using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class SelectStarInViewDefinitionRuleTests
{
    private static IDatabaseView CreateView(Identifier viewName, string definition)
    {
        return new DatabaseView(viewName, definition, new List<IDatabaseColumn>());
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
        Assert.That(() => rule.AnalyseViews(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewSelectingStar_ProducesMessageWithVisibleViewName()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var viewName = Identifier.CreateQualifiedIdentifier("test_schema", "test_view");
        var view = CreateView(viewName, "SELECT * FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_view"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithExplicitColumns_ProducesNoMessages()
    {
        var rule = new SelectStarInViewDefinitionRule(RuleLevel.Error);
        var view = CreateView("test_view", "SELECT id, name FROM source_table");
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }
}
