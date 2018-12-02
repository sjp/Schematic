using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
    {
        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = TableProvider.GetTable("table_test_table_1").UnwrapSome();
            var count = table.Columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_1").UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var count = columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = TableProvider.GetTable("table_test_table_1").UnwrapSome();
            var column = table.Columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_1").UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public void Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = TableProvider.GetTable("table_test_table_4").UnwrapSome();
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = await TableProvider.GetTableAsync("table_test_table_4").UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public void Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = TableProvider.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = TableProvider.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = TableProvider.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public void Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        {
            const string tableName = "table_test_table_33";
            var table = TableProvider.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            const string defaultValue = "1";
            Assert.AreEqual(defaultValue, column.DefaultValue);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        {
            const string tableName = "table_test_table_33";
            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            const string defaultValue = "1";
            Assert.AreEqual(defaultValue, column.DefaultValue);
        }

        [Test]
        public void Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = TableProvider.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = TableProvider.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = await TableProvider.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }
    }
}