using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
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
    public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        Assert.That(column.IsComputed, Is.False);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoincrement()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns.Single();

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithSerialIdentity_ReturnsSomeAutoincrement()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement, OptionIs.Some);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithSerialIdentity_ReturnsCorrectInitialValue()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(1));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithSerialIdentity_ReturnsCorrectIncrement()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(1));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsSomeAutoincrement()
    {
        const string tableName = "table_test_table_36";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement, OptionIs.Some);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
    {
        const string tableName = "table_test_table_36";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(123));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
    {
        const string tableName = "table_test_table_36";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(456));
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
        var dbVersion = await Dialect.GetDatabaseVersionAsync(Connection).ConfigureAwait(false);
        if (dbVersion < new Version(12, 0))
        {
            Assert.Pass();
            return;
        }

        const string tableName = "table_test_table_37";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var computedColumns = table.Columns.Where(c => c.IsComputed).ToList();

        Assert.That(computedColumns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnNames()
    {
        var dbVersion = await Dialect.GetDatabaseVersionAsync(Connection).ConfigureAwait(false);
        if (dbVersion < new Version(12, 0))
        {
            Assert.Pass();
            return;
        }

        const string tableName = "table_test_table_37";
        const string expectedColumnName = "test_column_2";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var computedColumn = table.Columns.Single(c => c.IsComputed);

        Assert.That(computedColumn.Name.LocalName, Is.EqualTo(expectedColumnName));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnDefinition()
    {
        var dbVersion = await Dialect.GetDatabaseVersionAsync(Connection).ConfigureAwait(false);
        if (dbVersion < new Version(12, 0))
        {
            Assert.Pass();
            return;
        }

        const string tableName = "table_test_table_37";
        const string expectedDefinition = "(test_column_1 * 2)";
        var table = await GetTableAsync(tableName).ConfigureAwait(false);
        var computedColumn = table.Columns.Single(c => c.IsComputed) as IDatabaseComputedColumn;

        Assert.That(computedColumn.Definition.UnwrapSome(), Is.EqualTo(expectedDefinition));
    }
}