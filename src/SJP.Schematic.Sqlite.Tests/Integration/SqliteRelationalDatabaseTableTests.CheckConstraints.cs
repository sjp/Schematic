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

        [Test]
        public void CheckConstraint_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            const string expectedDefinition = "([test_column]>(1))";

            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraint["ck_test_table_14"];

            var comparer = new SqliteExpressionComparer();
            var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

            Assert.IsTrue(checksEqual);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            const string expectedDefinition = "([test_column]>(1))";

            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraints.Single();

            var comparer = new SqliteExpressionComparer();
            var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

            Assert.IsTrue(checksEqual);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            const string expectedDefinition = "([test_column]>(1))";

            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            var comparer = new SqliteExpressionComparer();
            var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

            Assert.IsTrue(checksEqual);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            const string expectedDefinition = "([test_column]>(1))";

            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var check = checks.Single();

            var comparer = new SqliteExpressionComparer();
            var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

            Assert.IsTrue(checksEqual);
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