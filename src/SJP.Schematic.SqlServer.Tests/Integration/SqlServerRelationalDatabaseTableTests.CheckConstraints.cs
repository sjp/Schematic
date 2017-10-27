using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    [TestFixture]
    internal partial class SqlServerRelationalDatabaseTableTests : SqlServerTest
    {
        [Test]
        public void Check_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1");
            var checkLookup = table.Check;

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public void Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.Checks.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checkLookup = await table.CheckAsync().ConfigureAwait(false);

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var count = checks.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Check_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.Check["ck_test_table_14"];

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.Checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public void Check_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.Check["ck_test_table_14"];

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public void Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.Checks.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public void Check_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.Check["ck_test_table_14"];

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public void Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.Checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public void Check_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_32");
            var check = table.Check["ck_test_table_32"];

            Assert.IsFalse(check.IsEnabled);
        }

        [Test]
        public void Checks_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_32");
            var check = table.Checks.Single();

            Assert.IsFalse(check.IsEnabled);
        }

        [Test]
        public async Task CheckAsync_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_32").ConfigureAwait(false);
            var checkLookup = await table.CheckAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_32"];

            Assert.IsFalse(check.IsEnabled);
        }

        [Test]
        public async Task ChecksAsync_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_32").ConfigureAwait(false);
            var checks = await table.ChecksAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.IsFalse(check.IsEnabled);
        }
    }
}