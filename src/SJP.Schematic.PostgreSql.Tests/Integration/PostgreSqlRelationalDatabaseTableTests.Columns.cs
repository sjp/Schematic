using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    [TestFixture]
    internal partial class PostgreSqlRelationalDatabaseTableTests : PostgreSqlTest
    {
        [Test]
        public void Column_WhenGivenTableWithOneColumn_ReturnsColumnLookupWithOneValue()
        {
            var table = Database.GetTable("table_test_table_1");
            var columnLookup = table.Column;

            Assert.AreEqual(1, columnLookup.Count);
        }

        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.Columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithOneColumn_ReturnsColumnLookupWithOneValue()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);

            Assert.AreEqual(1, columnLookup.Count);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var count = columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Column_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_1");
            var column = table.Column.Values.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_1");
            var column = table.Columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public void Column_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = Database.GetTable("table_test_table_4");
            var columns = table.Column.Values;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public void Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = Database.GetTable("table_test_table_4");
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = await Database.GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var columns = columnLookup.Values;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = await Database.GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public void Column_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectTable()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Single();

            Assert.AreEqual(tableName, column.Table.Name.LocalName);
        }

        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectTable()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Single();

            Assert.AreEqual(tableName, column.Table.Name.LocalName);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectTable()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();

            Assert.AreEqual(tableName, column.Table.Name.LocalName);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectTable()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.AreEqual(tableName, column.Table.Name.LocalName);
        }

        [Test]
        public void Column_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public void Column_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public void Column_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public void Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public void Column_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public void Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public void Column_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public void Column_WhenGivenTableColumnWithIdentity_ReturnsNotNullAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Last();

            Assert.IsNotNull(column.AutoIncrement);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithIdentity_ReturnsNotNullAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Last();

            Assert.IsNotNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableColumnWithIdentity_ReturnsNotNullAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Last();

            Assert.IsNotNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithIdentity_ReturnsNotNullAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.IsNotNull(column.AutoIncrement);
        }

        [Test]
        public void Column_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Last();

            Assert.AreEqual(1, column.AutoIncrement.InitialValue);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.InitialValue);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Last();

            Assert.AreEqual(1, column.AutoIncrement.InitialValue);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.InitialValue);
        }

        [Test]
        public void Column_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName);
            var column = table.Column.Values.Last();

            Assert.AreEqual(1, column.AutoIncrement.Increment);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName);
            var column = table.Columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.Increment);
        }

        [Test]
        public async Task ColumnAsync_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Last();

            Assert.AreEqual(1, column.AutoIncrement.Increment);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.Increment);
        }
    }
}