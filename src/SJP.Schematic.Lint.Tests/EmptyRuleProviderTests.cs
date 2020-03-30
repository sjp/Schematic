using System.Data;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class EmptyRuleProviderTests
    {
        private static IRuleProvider RuleProvider => new EmptyRuleProvider();

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
        public static void GetRules_GivenValidInput_ReturnsEmptySet()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var rules = RuleProvider.GetRules(connection, RuleLevel.Error);

            Assert.That(rules, Is.Empty);
        }
    }
}
