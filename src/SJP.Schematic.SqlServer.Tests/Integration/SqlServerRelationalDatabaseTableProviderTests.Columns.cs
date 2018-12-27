using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
    {
        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var count = table.Columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var column = table.Columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = await GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        {
            const string tableName = "table_test_table_33";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            const string defaultValue = "1";
            var comparer = new SqlServerExpressionComparer();
            var equals = comparer.Equals(defaultValue, column.DefaultValue);

            Assert.IsTrue(equals);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
        {
            const string tableName = "table_test_table_34";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsNotNullObject()
        {
            const string tableName = "table_test_table_34";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.IsNotNull(computedColumn);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsCorrectDefinition()
        {
            const string tableName = "table_test_table_34";
            const string expectedDefinition = "([test_column_1]+[test_column_2])";

            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.AreEqual(expectedDefinition, computedColumn.Definition);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsTrue(column.AutoIncrement.IsNone);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsSomeAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.IsTrue(column.AutoIncrement.IsSome);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.AreEqual(10, column.AutoIncrement.UnwrapSome().InitialValue);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.AreEqual(5, column.AutoIncrement.UnwrapSome().Increment);
        }
    }
}