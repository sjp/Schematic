using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableTests : OracleTest
    {
        [Test]
        public void Check_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var checkLookup = table.Check;

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public void Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var count = table.Checks.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checkLookup = await tableOption.UnwrapSome().CheckAsync().ConfigureAwait(false);

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checks = await tableOption.UnwrapSome().ChecksAsync().ConfigureAwait(false);
            var count = checks.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Check_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            const string expectedCheckName = "CK_TEST_TABLE_14";

            var table = Database.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Check[expectedCheckName];

            Assert.AreEqual(expectedCheckName, check.Name.LocalName);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            const string expectedCheckName = "CK_TEST_TABLE_14";

            var table = Database.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Checks.Single();

            Assert.AreEqual(expectedCheckName, check.Name.LocalName);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            const string expectedCheckName = "CK_TEST_TABLE_14";

            var tableOption = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await tableOption.UnwrapSome().CheckAsync().ConfigureAwait(false);
            var check = checkLookup[expectedCheckName];

            Assert.AreEqual(expectedCheckName, check.Name.LocalName);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            const string expectedCheckName = "CK_TEST_TABLE_14";

            var tableOption = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await tableOption.UnwrapSome().ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual(expectedCheckName, check.Name.LocalName);
        }

        [Test]
        public void Check_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Check["ck_test_table_14"];

            Assert.AreEqual("test_column > 1", check.Definition);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Checks.Single();

            Assert.AreEqual("test_column > 1", check.Definition);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await tableOption.UnwrapSome().CheckAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.AreEqual("test_column > 1", check.Definition);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await tableOption.UnwrapSome().ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("test_column > 1", check.Definition);
        }

        [Test]
        public void Check_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Check["ck_test_table_14"];

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public void Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_14").UnwrapSome();
            var check = table.Checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await tableOption.UnwrapSome().CheckAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await tableOption.UnwrapSome().ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public void Check_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_32").UnwrapSome();
            var check = table.Check["ck_test_table_32"];

            Assert.IsFalse(check.IsEnabled);
        }

        [Test]
        public void Checks_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_32").UnwrapSome();
            var check = table.Checks.Single();

            Assert.IsFalse(check.IsEnabled);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_32").ConfigureAwait(false);
            var checkLookup = await tableOption.UnwrapSome().CheckAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_32"];

            Assert.IsFalse(check.IsEnabled);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_32").ConfigureAwait(false);
            var checks = await tableOption.UnwrapSome().ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.IsFalse(check.IsEnabled);
        }
    }
}