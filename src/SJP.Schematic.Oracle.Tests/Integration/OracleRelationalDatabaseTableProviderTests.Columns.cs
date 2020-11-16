using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
    {
        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);

            Assert.That(table.Columns, Has.Exactly(1).Items);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var column = table.Columns.Single();
            const string columnName = "TEST_COLUMN";

            Assert.That(column.Name.LocalName, Is.EqualTo(columnName));
        }

        [Test]
        public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "FIRST_NAME", "MIDDLE_NAME", "LAST_NAME" };
            var table = await GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.That(columnNames, Is.EqualTo(expectedColumnNames));
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.IsNullable, Is.True);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_2";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.IsNullable, Is.False);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNoneDefaultValue()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.DefaultValue, OptionIs.None);
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

            Assert.That(equals, Is.True);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.IsComputed, Is.False);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns[table.Columns.Count - 1];

            Assert.That(column.IsNullable, Is.True);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsNotNullObject()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns[table.Columns.Count - 1];

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.That(computedColumn, Is.Not.Null);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsCorrectDefinition()
        {
            const string tableName = "TABLE_TEST_TABLE_34";
            const string expectedDefinition = "\"TEST_COLUMN_1\"+\"TEST_COLUMN_2\"";

            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns[table.Columns.Count - 1];

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.That(computedColumn.Definition.UnwrapSome(), Is.EqualTo(expectedDefinition));
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoIncrement()
        {
            const string tableName = "TABLE_TEST_TABLE_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.AutoIncrement, OptionIs.None);
        }
    }
}