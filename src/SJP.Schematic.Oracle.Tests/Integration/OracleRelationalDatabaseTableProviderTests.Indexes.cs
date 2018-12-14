using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableProviderTests : OracleTest
    {
        [Test]
        public async Task Indexes_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var count = table.Indexes.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = await GetTableAsync("table_test_table_8").ConfigureAwait(false);
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
        public async Task Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var index = table.Indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_8", index.Name.LocalName);
        }

        [Test]
        public async Task Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

            var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
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
        public async Task Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var index = table.Indexes.Single();

            Assert.AreEqual("IX_TEST_TABLE_9", index.Name.LocalName);
        }

        [Test]
        public async Task Indexes_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var index = table.Indexes.Single();

            Assert.AreEqual(0, index.IncludedColumns.Count);
        }

        [Test]
        public async Task Indexes_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public async Task Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var index = table.Indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public async Task Indexes_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = await GetTableAsync("table_test_table_13").ConfigureAwait(false);
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }
    }
}