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
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var count = table.Columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = await Database.GetTableAsync("table_test_table_1").UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var count = columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var column = table.Columns.Single();
            const string columnName = "TEST_COLUMN";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_1").UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();
            const string columnName = "TEST_COLUMN";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public void Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "MIDDLE_NAME", "LAST_NAME" };
            var table = Database.GetTable("table_test_table_4").UnwrapSome();
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "MIDDLE_NAME", "LAST_NAME" };
            var table = await Database.GetTableAsync("table_test_table_4").UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public void Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_2";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_2";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public void Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        {
            const string tableName = "TABLE_TEST_TABLE_33";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            const string defaultValue = "1";
            var comparer = new OracleExpressionComparer();
            var equals = comparer.Equals(defaultValue, column.DefaultValue);

            Assert.IsTrue(equals);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        {
            const string tableName = "TABLE_TEST_TABLE_33";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            const string defaultValue = "1";
            var comparer = new OracleExpressionComparer();
            var equals = comparer.Equals(defaultValue, column.DefaultValue);

            Assert.IsTrue(equals);
        }

        [Test]
        public void Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public void Columns_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public void Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsNotNullObject()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.IsNotNull(computedColumn);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsNotNullObject()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.IsNotNull(computedColumn);
        }

        [Test]
        public void Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsCorrectDefinition()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            const string expectedDefinition = "\"TEST_COLUMN_1\"+\"TEST_COLUMN_2\"";

            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.AreEqual(expectedDefinition, computedColumn.Definition);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsCorrectDefinition()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            const string expectedDefinition = "\"TEST_COLUMN_1\"+\"TEST_COLUMN_2\"";

            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.AreEqual(expectedDefinition, computedColumn.Definition);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }
    }
}