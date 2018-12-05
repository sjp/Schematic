using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
    {
        [Test]
        public void Indexes_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = GetTable("table_test_table_1");
            var count = table.Indexes.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = GetTable("table_test_table_8");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("test_column", indexColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = GetTable("table_test_table_8");
            var index = table.Indexes.Single();

            Assert.AreEqual("ix_test_table_8", index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = GetTable("table_test_table_9");
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
            var table = GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.AreEqual("ix_test_table_9", index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var table = GetTable("table_test_table_9");
            var index = table.Indexes.Single();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            Assert.AreEqual(0, includedColumns.Count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public void Indexes_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = GetTable("table_test_table_13");
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }
    }
}