using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_4
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql94Test
    {
        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = await GetTableAsync("v94_table_test_table_1").ConfigureAwait(false);

            Assert.That(table.Columns, Has.Exactly(1).Items);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = await GetTableAsync("v94_table_test_table_1").ConfigureAwait(false);
            var column = table.Columns.Single();
            const string columnName = "test_column";

            Assert.That(column.Name.LocalName, Is.EqualTo(columnName));
        }

        [Test]
        public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = await GetTableAsync("v94_table_test_table_4").ConfigureAwait(false);
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.That(columnNames, Is.EqualTo(expectedColumnNames));
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "v94_table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.IsNullable, Is.True);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "v94_table_test_table_2";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.IsNullable, Is.False);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNoneDefaultValue()
        {
            const string tableName = "v94_table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.DefaultValue, OptionIs.None);
        }

        [Test]
        public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "v94_table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.IsComputed, Is.False);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoincrement()
        {
            const string tableName = "v94_table_test_table_1";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Single();

            Assert.That(column.AutoIncrement, OptionIs.None);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsSomeAutoincrement()
        {
            const string tableName = "v94_table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.That(column.AutoIncrement, OptionIs.Some);
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "v94_table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(1));
        }

        [Test]
        public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "v94_table_test_table_35";
            var table = await GetTableAsync(tableName).ConfigureAwait(false);
            var column = table.Columns.Last();

            Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(1));
        }
    }
}