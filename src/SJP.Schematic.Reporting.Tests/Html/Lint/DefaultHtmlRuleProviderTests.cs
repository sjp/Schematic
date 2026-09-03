using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint;

[TestFixture]
internal static class DefaultHtmlRuleProviderTests
{
    [Test]
    public static void GetRules_GivenNullConnection_ThrowsArgumentNullException()
    {
        var provider = new DefaultHtmlRuleProvider();
        Assert.That(() => provider.GetRules(null!, RuleLevel.Warning), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRules_GivenInvalidRuleLevel_ThrowsArgumentException()
    {
        var provider = new DefaultHtmlRuleProvider();
        var mockConnection = CreateMockConnection();
        const RuleLevel invalidLevel = (RuleLevel)999;

        Assert.That(() => provider.GetRules(mockConnection.Object, invalidLevel), Throws.ArgumentException);
    }

    [Test]
    public static void GetRules_GivenValidArguments_ReturnsExpectedNumberOfRules()
    {
        var provider = new DefaultHtmlRuleProvider();
        var mockConnection = CreateMockConnection();

        var rules = provider.GetRules(mockConnection.Object, RuleLevel.Warning).ToList();

        Assert.That(rules, Has.Count.EqualTo(40));
    }

    [Test]
    public static void GetRules_GivenValidArguments_ReturnsRulesAtRequestedLevel()
    {
        var provider = new DefaultHtmlRuleProvider();
        var mockConnection = CreateMockConnection();

        var rules = provider.GetRules(mockConnection.Object, RuleLevel.Error).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rules, Is.Not.Empty);
            Assert.That(rules, Has.All.Matches<IRule>(static r => r.Level == RuleLevel.Error));
        }
    }

    [Test]
    public static void GetRules_GivenValidArguments_ReturnsEveryRuleDefinedInTheRulesNamespace()
    {
        var provider = new DefaultHtmlRuleProvider();
        var mockConnection = CreateMockConnection();

        var provided = provider.GetRules(mockConnection.Object).Select(static r => r.GetType()).ToList();
        var defined = typeof(WhitespaceNameRule).Assembly
            .GetTypes()
            .Where(static t => t.Namespace == typeof(WhitespaceNameRule).Namespace
                && !t.IsAbstract
                && t.IsAssignableTo(typeof(IRule)))
            .ToList();

        // A rule that exists but is never handed out is a rule that silently does nothing.
        Assert.That(provided, Is.EquivalentTo(defined));
    }

    private static Mock<ISchematicConnection> CreateMockConnection()
    {
        var mockDialect = new Mock<IDatabaseDialect>();
        var mockConnection = new Mock<ISchematicConnection>();
        mockConnection.SetupGet(static c => c.Dialect).Returns(mockDialect.Object);

        return mockConnection;
    }
}
