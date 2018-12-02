using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
    {
        [Test]
        public void Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = TableProvider.GetTable("table_test_table_1").UnwrapSome();
            var count = table.Checks.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_1").UnwrapSomeAsync().ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var count = checks.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = TableProvider.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_14").UnwrapSomeAsync().ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            const string expectedDefinition = "([test_column]>(1))";

            var table = TableProvider.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Checks.Single();

            var comparer = new SqliteExpressionComparer();
            var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

            Assert.IsTrue(checksEqual);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            const string expectedDefinition = "([test_column]>(1))";

            var table = await TableProvider.GetTableAsync("table_test_table_14").UnwrapSomeAsync().ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            var comparer = new SqliteExpressionComparer();
            var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

            Assert.IsTrue(checksEqual);
        }

        [Test]
        public void Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = TableProvider.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_14").UnwrapSomeAsync().ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }
    }
}