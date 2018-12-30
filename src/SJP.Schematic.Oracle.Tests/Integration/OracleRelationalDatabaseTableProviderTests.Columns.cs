using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableProviderTests : OracleTest
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
            const string columnName = "TEST_COLUMN";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "MIDDLE_NAME", "LAST_NAME" };
            var table = await GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_2";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNoneDefaultValue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsTrue(column.DefaultValue.IsNone);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        {
            const string tableName = "TABLE_TEST_TABLE_33";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            const string defaultValue = "1";
            var comparer = new OracleExpressionComparer();
            var equals = comparer.Equals(defaultValue, column.DefaultValue.UnwrapSome());

            Assert.IsTrue(equals);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsNotNullObject()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.IsNotNull(computedColumn);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsCorrectDefinition()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            const string expectedDefinition = "\"TEST_COLUMN_1\"+\"TEST_COLUMN_2\"";

            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.AreEqual(expectedDefinition, computedColumn.Definition.UnwrapSome());
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoIncrement()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.IsTrue(column.AutoIncrement.IsNone);
        }
    }
}