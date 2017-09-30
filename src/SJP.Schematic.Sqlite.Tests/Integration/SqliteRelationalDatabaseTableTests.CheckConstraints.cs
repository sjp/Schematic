using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    [TestFixture]
    internal partial class SqliteRelationalDatabaseTableTests : SqliteTest
    {
        [Test]
        public void CheckConstraint_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1");
            var checkLookup = table.CheckConstraint;

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.CheckConstraints.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var count = checks.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void CheckConstraint_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraint["ck_test_table_14"];

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraints.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        // TODO, need to unwrap definitions possibly?
        // This is because the input may not necessarily match default values we give it.
        // i.e. declare definition as '([test_column] > 1)' and end up with '([test_column]>(1))'

        [Test]
        public void CheckConstraint_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraint["ck_test_table_14"];

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraints.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public void CheckConstraint_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraint["ck_test_table_14"];

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraints.Single();

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }
    }
}