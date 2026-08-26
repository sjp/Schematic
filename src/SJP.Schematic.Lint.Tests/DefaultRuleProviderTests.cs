using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests;

[TestFixture]
internal static class DefaultRuleProviderTests
{
    private static IRuleProvider RuleProvider => new DefaultRuleProvider();

    [Test]
    public static void GetRules_GivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => RuleProvider.GetRules(null, RuleLevel.Error), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRules_GivenInvalidRuleLevel_ThrowsArgumentException()
    {
        Assert.That(() => RuleProvider.GetRules(Mock.Of<ISchematicConnection>(), (RuleLevel)555), Throws.ArgumentException);
    }

    [Test]
    public static void GetRules_GivenValidInput_ReturnsNonEmptySet()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();
        var connection = new SchematicConnection(dbConnection, dialect);

        var rules = RuleProvider.GetRules(connection, RuleLevel.Error);

        Assert.That(rules, Is.Not.Empty);
    }

    [Test]
    public static void GetRules_GivenNullConnectionAndNoLevel_ThrowsArgumentNullException()
    {
        Assert.That(() => RuleProvider.GetRules(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRules_GivenNoLevel_ReturnsTheSameRulesAsTheLevelOverload()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());

        var defaulted = RuleProvider.GetRules(connection).Select(static r => r.Id).ToList();
        var levelled = RuleProvider.GetRules(connection, RuleLevel.Error).Select(static r => r.Id).ToList();

        Assert.That(defaulted, Is.EqualTo(levelled));
    }

    [Test]
    public static void GetRules_GivenNoLevel_ReturnsRulesAtMoreThanOneLevel()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());

        var levels = RuleProvider.GetRules(connection)
            .Select(static r => r.Level)
            .Distinct()
            .ToList();

        // The point of per-rule defaults: severity is a signal, not a constant.
        Assert.That(levels, Has.Count.GreaterThan(1));
    }

    [Test]
    public static void GetRules_GivenExplicitLevel_ReturnsEveryRuleAtThatLevel()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());

        var rules = RuleProvider.GetRules(connection, RuleLevel.Error);

        Assert.That(rules.Select(static r => r.Level), Is.All.EqualTo(RuleLevel.Error));
    }

    [Test]
    public static void GetRules_GivenValidInput_ReturnsEveryRuleDefinedInTheRulesNamespace()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());

        var provided = RuleProvider.GetRules(connection).Select(static r => r.GetType()).ToList();
        var defined = typeof(WhitespaceNameRule).Assembly
            .GetTypes()
            .Where(static t => t.Namespace == typeof(WhitespaceNameRule).Namespace
                && !t.IsAbstract
                && t.IsAssignableTo(typeof(IRule)))
            .ToList();

        // A rule that exists but is never handed out is a rule that silently does nothing.
        Assert.That(provided, Is.EquivalentTo(defined));
    }
}
