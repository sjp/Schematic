using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests;

[TestFixture]
internal static class PluginRuleProviderTests
{
    private static IRuleProvider RuleProvider => new PluginRuleProvider();

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
    public static void GetRules_WhenInvoked_RetrievesRulesFromTestRuleProvider()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();
        var connection = new SchematicConnection(dbConnection, dialect);

        var rules = RuleProvider.GetRules(connection, RuleLevel.Error);

        Assert.That(rules, Has.Exactly(TestRuleProvider.RuleCount).Items);
    }

    [Test]
    public static void GetRules_WhenInvokedWithMatchingDialect_RetrievesRulesFromTestRuleProviderAndTestDialectProvider()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = new Fakes.FakeDatabaseDialect();
        var connection = new SchematicConnection(dbConnection, dialect);

        var rules = RuleProvider.GetRules(connection, RuleLevel.Error);
        const int expectedCount = TestRuleProvider.RuleCount + TestDialectRuleProvider.RuleCount;

        Assert.That(rules, Has.Exactly(expectedCount).Items);
    }
}

/// <summary>
/// Not intended to be used directly, testing only.
/// </summary>
public class TestRuleProvider : IRuleProvider
{
    /// <summary>
    /// Ignore.
    /// </summary>
    public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
    {
        return new DefaultRuleProvider()
            .GetRules(connection, level)
            .Take(RuleCount)
            .ToList();
    }

    /// <summary>
    /// Ignore.
    /// </summary>
    public const int RuleCount = 3;
}

/// <summary>
/// Not intended to be used directly, testing only.
/// </summary>
public class TestDialectRuleProvider : IDialectRuleProvider<Fakes.FakeDatabaseDialect>
{
    /// <summary>
    /// Ignore.
    /// </summary>
    public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
    {
        return new DefaultRuleProvider()
            .GetRules(connection, level)
            .Take(RuleCount)
            .ToList();
    }

    /// <summary>
    /// Ignore.
    /// </summary>
    public const int RuleCount = 5;
}
