using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests
{
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
        public static void GetRules_WhenInvoked_RetrievesRulesFromTestClass()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var rules = RuleProvider.GetRules(connection, RuleLevel.Error);

            Assert.That(rules, Is.Not.Empty);
        }

        public class TestRuleProvider : IRuleProvider
        {
            public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
            {
                return new DefaultRuleProvider()
                    .GetRules(connection, level)
                    .Take(3)
                    .ToList();
            }
        }
    }
}
