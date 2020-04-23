using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class RuleProviderBuilderTests
    {
        [Test]
        public static void AddRuleProvider_GivenNullRuleProvider_ThrowsArgumentNullException()
        {
            Assert.That(() => new RuleProviderBuilder().AddRuleProvider(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Build_GivenNoProviders_ReturnsEmptyRuleSet()
        {
            var dbConnection = Mock.Of<IDbConnectionFactory>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var ruleProvider = new RuleProviderBuilder().Build();
            var rules = ruleProvider.GetRules(connection, RuleLevel.Error);

            Assert.That(rules, Is.Empty);
        }

        [Test]
        public static void Build_GivenDefaultProvider_ReturnsNonEmptyRuleSet()
        {
            var dbConnection = Mock.Of<IDbConnectionFactory>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var defaultProvider = new DefaultRuleProvider();
            var ruleProvider = new RuleProviderBuilder()
                .AddRuleProvider(defaultProvider)
                .Build();
            var rules = ruleProvider.GetRules(connection, RuleLevel.Error);

            Assert.That(rules, Is.Not.Empty);
        }

        [Test]
        public static void Build_GivenDefaultProviderViaGeneric_ReturnsNonEmptyRuleSet()
        {
            var dbConnection = Mock.Of<IDbConnectionFactory>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var ruleProvider = new RuleProviderBuilder()
                .AddRuleProvider<DefaultRuleProvider>()
                .Build();
            var rules = ruleProvider.GetRules(connection, RuleLevel.Error);

            Assert.That(rules, Is.Not.Empty);
        }

        [Test]
        public static void Build_GivenMultipleProviders_ReturnsExpectedRuleCount()
        {
            var dbConnection = Mock.Of<IDbConnectionFactory>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var defaultProvider = new DefaultRuleProvider();
            var defaultRules = defaultProvider.GetRules(connection, RuleLevel.Error).ToList();
            var expectedCount = defaultRules.Count * 2;

            var ruleProvider = new RuleProviderBuilder()
                .AddRuleProvider(defaultProvider)
                .AddRuleProvider(defaultProvider)
                .Build();

            var builderRules = ruleProvider.GetRules(connection, RuleLevel.Error);

            Assert.That(builderRules, Has.Exactly(expectedCount).Items);
        }
    }
}
