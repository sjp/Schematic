using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableTests : OracleTest
    {
        [Test]
        public void UniqueKeys_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var count = table.UniqueKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = Database.GetTable("table_test_table_5").UnwrapSome();
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_5").UnwrapSome();
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("TEST_COLUMN", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_6").UnwrapSome();
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("TEST_COLUMN", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_6").UnwrapSome();
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("UK_TEST_TABLE_6", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = Database.GetTable("table_test_table_7").UnwrapSome();
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_7").UnwrapSome();
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("UK_TEST_TABLE_7", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var uniqueKeys = await tableOption.UnwrapSome().UniqueKeysAsync().ConfigureAwait(false);
            var count = uniqueKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var uniqueKeys = await tableOption.UnwrapSome().UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var uniqueKeys = await tableOption.UnwrapSome().UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("TEST_COLUMN", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var uniqueKeys = await tableOption.UnwrapSome().UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("TEST_COLUMN", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var uniqueKeys = await tableOption.UnwrapSome().UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual("UK_TEST_TABLE_6", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var tableOption = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var uniqueKeys = await tableOption.UnwrapSome().UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var uniqueKeys = await tableOption.UnwrapSome().UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual("UK_TEST_TABLE_7", uk.Name.LocalName);
        }
    }
}