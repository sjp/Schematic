using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
    {
        [Test]
        public void Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = GetTable("table_test_table_1");
            var count = table.Checks.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = GetTable("table_test_table_14");
            var check = table.Checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = GetTable("table_test_table_14");
            var check = table.Checks.Single();

            Assert.AreEqual("(test_column > 1)", check.Definition);
        }

        [Test]
        public void Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = GetTable("table_test_table_14");
            var check = table.Checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }
    }
}