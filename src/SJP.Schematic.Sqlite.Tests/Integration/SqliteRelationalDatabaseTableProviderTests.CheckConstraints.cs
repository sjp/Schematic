using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
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
            var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            const string expectedDefinition = "([test_column]>(1))";

            var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            var comparer = new SqliteExpressionComparer();
            var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

            Assert.IsTrue(checksEqual);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }
    }
}