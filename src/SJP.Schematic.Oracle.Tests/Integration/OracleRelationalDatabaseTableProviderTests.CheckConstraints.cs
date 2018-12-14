using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableProviderTests : OracleTest
    {
        [Test]
        public async Task Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var count = table.Checks.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            const string expectedCheckName = "CK_TEST_TABLE_14";

            var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.AreEqual(expectedCheckName, check.Name.LocalName);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.AreEqual("test_column > 1", check.Definition);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = await GetTableAsync("table_test_table_32").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.IsFalse(check.IsEnabled);
        }
    }
}