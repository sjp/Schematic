using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableTests : OracleTest
    {
        [Test]
        public void Index_WhenGivenTableWithNoIndexes_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.Index.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Index_WhenQueriedByName_ReturnsCorrectIndex()
        {
            const string indexName = "IX_TEST_TABLE_8";

            var table = Database.GetTable("table_test_table_8");
            var index = table.Index[indexName];

            Assert.AreEqual(indexName, index.Name.LocalName);
        }

        [Test]
        public void Index_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Index["ix_test_table_8"];
            var indexColumns = index.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("TEST_COLUMN", indexColumns.Single().DependentColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void Index_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            const string indexName = "IX_TEST_TABLE_8";
            var table = Database.GetTable("table_test_table_8");
            var index = table.Index[indexName];

            Assert.AreEqual(indexName, index.Name.LocalName);
        }

        [Test]
        public void Index_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = Database.GetTable("table_test_table_9");
            var index = table.Index["ix_test_table_9"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void Index_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            const string indexName = "IX_TEST_TABLE_9";

            var table = Database.GetTable("table_test_table_9");
            var index = table.Index[indexName];

            Assert.AreEqual(indexName, index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.Indexes.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("TEST_COLUMN", indexColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_8", index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_9", index.Name.LocalName);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithNoIndexes_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var count = indexLookup.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task IndexAsync_WhenQueriedByName_ReturnsCorrectIndex()
        {
            const string indexName = "IX_TEST_TABLE_8";

            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup[indexName];

            Assert.AreEqual(indexName, index.Name.LocalName);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_8"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("TEST_COLUMN", indexColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            const string indexName = "IX_TEST_TABLE_8";
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup[indexName];

            Assert.AreEqual(indexName, index.Name.LocalName);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_9"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            const string indexName = "IX_TEST_TABLE_9";

            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup[indexName];

            Assert.AreEqual(indexName, index.Name.LocalName);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var count = indexes.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("TEST_COLUMN", indexColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_8", index.Name.LocalName);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_9", index.Name.LocalName);
        }

        [Test]
        public void Index_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = Database.GetTable("table_test_table_9");
            var indexLookup = table.Index;
            var index = indexLookup["ix_test_table_9"];

            Assert.AreEqual(0, index.IncludedColumns.Count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.AreEqual(0, index.IncludedColumns.Count);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_9"];

            Assert.AreEqual(0, index.IncludedColumns.Count);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual(0, index.IncludedColumns.Count);
        }

        [Test]
        public void Index_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Index.Values.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public void Indexes_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup.Values.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public void Index_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Index.Values.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public void Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup.Values.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public void Index_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = Database.GetTable("table_test_table_13");
            var index = table.Index.Values.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public void Indexes_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = Database.GetTable("table_test_table_13");
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_13").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup.Values.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_13").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }
    }
}