using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
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
            var table = TableProvider.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Checks.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_14").UnwrapSomeAsync().ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
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

        [Test]
        public void Checks_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = TableProvider.GetTable("table_test_table_32").UnwrapSome();
            var check = table.Checks.Single();

            Assert.IsFalse(check.IsEnabled);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_32").UnwrapSomeAsync().ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.IsFalse(check.IsEnabled);
        }
    }
}