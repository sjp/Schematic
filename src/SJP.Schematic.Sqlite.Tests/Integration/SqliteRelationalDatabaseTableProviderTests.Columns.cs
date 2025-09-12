using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
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
        const string columnName = "test_column";

        Assert.That(column.Name.LocalName, Is.EqualTo(columnName));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
    {
        var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
        var table = await GetTableAsync("table_test_table_4").ConfigureAwait(false);
        var columns = table.Columns;
        var columnNames = columns.Select(c => c.Name.LocalName);

        Assert.That(columnNames, Is.EqualTo(expectedColumnNames));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        Assert.That(column.IsNullable, Is.True);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
    {
        const string tableName = "table_test_table_2";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNoneDefaultValue()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        Assert.That(column.DefaultValue, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
    {
        const string tableName = "table_test_table_33";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        const string defaultValue = "1";
        Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo(defaultValue));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        Assert.That(column.IsComputed, Is.False);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoIncrement()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNoGeneratedColumns_ReturnsNoComputedColumns()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var computedColumns = table.Columns.Where(c => c.IsComputed).ToList();

        Assert.That(computedColumns, Is.Empty);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnCount()
    {
        const string tableName = "table_test_table_37";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var computedColumns = table.Columns.Where(c => c.IsComputed).ToList();

        Assert.That(computedColumns, Has.Exactly(3).Items);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnNames()
    {
        const string tableName = "table_test_table_37";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var expectedColumnNames = new[] { "test_column_2", "test_column_3", "test_column_4" };
        var computedColumnNames = table.Columns.Where(c => c.IsComputed).Select(c => c.Name.LocalName).ToList();

        Assert.That(computedColumnNames, Is.EqualTo(expectedColumnNames));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnDefinitions()
    {
        const string tableName = "table_test_table_37";
        var expectedDefinitions = new[]
        {
            "(test_column_1 * test_column_1)",
            "(test_column_1 * test_column_1 * test_column_1)",
            "(test_column_1 * test_column_1 * test_column_1 * test_column_1)",
        };
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var computedColumnDefinitions = table.Columns
            .Where(c => c.IsComputed)
            .Select(c => c as IDatabaseComputedColumn)
            .Select(c => c.Definition.Match(x => x, () => string.Empty))
            .ToList();

        Assert.That(computedColumnDefinitions, Is.EqualTo(expectedDefinitions));
    }
}