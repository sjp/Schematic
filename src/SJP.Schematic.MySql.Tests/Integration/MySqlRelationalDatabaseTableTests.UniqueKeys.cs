using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests.Integration
{
    [TestFixture]
    internal partial class MySqlRelationalDatabaseTableTests : MySqlTest
    {
        [Test]
        public void UniqueKey_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.UniqueKey.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKey.Values.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public void UniqueKey_WhenQueriedByName_ReturnsCorrectKey()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKey["uk_test_table_6"];

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKey.Values.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKey["uk_test_table_6"];
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            const string uniqueKeyName = "uk_test_table_6";
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKey[uniqueKeyName];

            Assert.AreEqual(uniqueKeyName, uk.Name.LocalName);
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_7");
            var uk = table.UniqueKey["uk_test_table_7"];
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_7");
            var uk = table.UniqueKey["uk_test_table_7"];

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.UniqueKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_7");
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
            var table = Database.GetTable("table_test_table_7");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var count = ukLookup.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup.Values.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenQueriedByName_ReturnsCorrectKey()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_6"];

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup.Values.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_6"];
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            const string uniqueKeyName = "uk_test_table_6";
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup[uniqueKeyName];

            Assert.AreEqual(uniqueKeyName, uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_7"];
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_7"];

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var count = uniqueKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
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
            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }
    }
}