using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint;

internal static class DefaultHtmlRuleProviderTests
{
    [Test]
    public static void GetRules_GivenNullConnection_ThrowsArgumentNullException()
    {
        var provider = new DefaultHtmlRuleProvider();
        Assert.That(
            () => provider.GetRules(null!, RuleLevel.Warning),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection")
        );
    }

    [Test]
    public static void GetRules_GivenInvalidRuleLevel_ThrowsArgumentException()
    {
        var provider = new DefaultHtmlRuleProvider();
        var mockConnection = CreateMockConnection();
        const RuleLevel invalidLevel = (RuleLevel)999;

        Assert.That(
            () => provider.GetRules(mockConnection.Object, invalidLevel),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
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

    [Test]
    public static void GetRules_GivenDatabaseQueriesExcluded_ReturnsNoRuleThatTakesAConnection()
    {
        var provider = new DefaultHtmlRuleProvider(tableStatistics: null, queryDatabase: false);
        var mockConnection = CreateMockConnection();

        var rules = provider.GetRules(mockConnection.Object).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rules, Is.Not.Empty);
            Assert.That(rules, Has.None.Matches<IRule>(static r => TakesConnection(r)));
        }
    }

    [Test]
    public static void GetRules_GivenDatabaseQueriesExcluded_LeavesOutOnlyTheRulesThatTakeAConnection()
    {
        var mockConnection = CreateMockConnection();

        var schemaOnly = new DefaultHtmlRuleProvider(tableStatistics: null, queryDatabase: false)
            .GetRules(mockConnection.Object)
            .Select(static r => r.Id)
            .ToList();
        var expected = new DefaultHtmlRuleProvider()
            .GetRules(mockConnection.Object)
            .Where(static r => !TakesConnection(r))
            .Select(static r => r.Id)
            .ToList();

        // Compared in order: results are written out in the order the rules are provided.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(schemaOnly, Is.EqualTo(expected));
            Assert.That(schemaOnly, Has.Count.EqualTo(36));
        }
    }

    [Test]
    public static void GetRules_GivenDatabaseQueriesIncluded_ReturnsTheSameRulesAsTheDefaultConstructor()
    {
        var mockConnection = CreateMockConnection();

        var included = new DefaultHtmlRuleProvider(tableStatistics: null, queryDatabase: true)
            .GetRules(mockConnection.Object)
            .Select(static r => r.Id)
            .ToList();
        var defaulted = new DefaultHtmlRuleProvider()
            .GetRules(mockConnection.Object)
            .Select(static r => r.Id)
            .ToList();

        Assert.That(included, Is.EqualTo(defaulted));
    }

    // A rule can only query the database if it is given a connection to query it with.
    private static bool TakesConnection(IRule rule) => rule.GetType()
        .GetConstructors()
        .Any(static c => c.GetParameters().Any(static p => p.ParameterType == typeof(ISchematicConnection)));

    private static Mock<ISchematicConnection> CreateMockConnection()
    {
        var mockDialect = new Mock<IDatabaseDialect>();
        var mockConnection = new Mock<ISchematicConnection>();
        mockConnection.SetupGet(static c => c.Dialect).Returns(mockDialect.Object);

        return mockConnection;
    }
}
