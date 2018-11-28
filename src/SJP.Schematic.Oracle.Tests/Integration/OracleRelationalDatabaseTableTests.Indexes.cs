using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableTests : OracleTest
    {
        [Test]
        public void Indexes_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var count = table.Indexes.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_8").UnwrapSome();
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
            var table = Database.GetTable("table_test_table_8").UnwrapSome();
            var index = table.Indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_8", index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = Database.GetTable("table_test_table_9").UnwrapSome();
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
            var table = Database.GetTable("table_test_table_9").UnwrapSome();
            var index = table.Indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_9", index.Name.LocalName);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var count = indexes.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_8").UnwrapSomeAsync().ConfigureAwait(false);
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
            var table = await Database.GetTableAsync("table_test_table_8").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_8", index.Name.LocalName);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = await Database.GetTableAsync("table_test_table_9").UnwrapSomeAsync().ConfigureAwait(false);
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
            var table = await Database.GetTableAsync("table_test_table_9").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_9", index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = Database.GetTable("table_test_table_9").UnwrapSome();
            var index = table.Indexes.Single();

            Assert.AreEqual(0, index.IncludedColumns.Count);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_9").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual(0, index.IncludedColumns.Count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_9").UnwrapSome();
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_9").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public void Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = Database.GetTable("table_test_table_9").UnwrapSome();
            var index = table.Indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_9").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public void Indexes_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = Database.GetTable("table_test_table_13").UnwrapSome();
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_13").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }
    }
}