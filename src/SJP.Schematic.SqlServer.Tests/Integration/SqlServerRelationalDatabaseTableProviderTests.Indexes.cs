using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
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
        public void Indexes_WhenGivenTableWithIndexContainingSingleIncludedColumn_ReturnsIndexWithIncludedColumn()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = GetTable("table_test_table_10");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(1, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithIndexContainingMultipleIncludedColumns_ReturnsIndexWithAllColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name" };
            var expectedIncludedColumnNames = new[] { "last_name", "middle_name" };

            var table = GetTable("table_test_table_11");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(2, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = GetTable("table_test_table_11");
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public void Indexes_WhenGivenTableWithDisabledIndex_ReturnsIndexWithIsEnabledFalse()
        {
            var table = GetTable("table_test_table_12");
            var index = table.Indexes.Single();

            Assert.IsFalse(index.IsEnabled);
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