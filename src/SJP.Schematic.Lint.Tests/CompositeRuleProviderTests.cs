using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests;

[TestFixture]
internal static class CompositeRuleProviderTests
{
    [Test]
    public static void Ctor_GivenNullRuleProviders_ThrowsArgumentNullException()
    {
        Assert.That(() => new CompositeRuleProvider(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRules_GivenNullConnection_ThrowsArgumentNullException()
    {
        var ruleProvider = new CompositeRuleProvider(Array.Empty<IRuleProvider>());
        Assert.That(() => ruleProvider.GetRules(null, RuleLevel.Error), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRules_GivenInvalidRuleLevel_ThrowsArgumentException()
    {
        var ruleProvider = new CompositeRuleProvider(Array.Empty<IRuleProvider>());
        Assert.That(() => ruleProvider.GetRules(Mock.Of<ISchematicConnection>(), (RuleLevel)555), Throws.ArgumentException);
    }

    [Test]
    public static void GetRules_GivenNoProviders_ReturnsEmptySet()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();
        var connection = new SchematicConnection(dbConnection, dialect);

        var ruleProvider = new CompositeRuleProvider(Array.Empty<IRuleProvider>());
        var rules = ruleProvider.GetRules(connection, RuleLevel.Error);

        Assert.That(rules, Is.Empty);
    }

    [Test]
    public static void GetRules_GivenDefaultProvider_ReturnsNonEmptySet()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();
        var connection = new SchematicConnection(dbConnection, dialect);

        var ruleProvider = new CompositeRuleProvider(new[] { new DefaultRuleProvider() });
        var rules = ruleProvider.GetRules(connection, RuleLevel.Error);

        Assert.That(rules, Is.Not.Empty);
    }

    [Test]
    public static void GetRules_GivenMultipleProviders_ReturnsExpectedRuleCount()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();
        var connection = new SchematicConnection(dbConnection, dialect);

        var defaultProvider = new DefaultRuleProvider();

        var ruleProvider = new CompositeRuleProvider(new[] { defaultProvider, defaultProvider });
        var defaultRules = defaultProvider.GetRules(connection, RuleLevel.Error).ToList();
        var expectedCount = defaultRules.Count * 2;

        var compositeRules = ruleProvider.GetRules(connection, RuleLevel.Error);

        Assert.That(compositeRules, Has.Exactly(expectedCount).Items);
    }
}