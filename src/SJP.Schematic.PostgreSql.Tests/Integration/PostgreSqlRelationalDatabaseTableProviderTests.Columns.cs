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
        var table = await GetTableAsync("table_test_table_1");

        Assert.That(table.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_1");
        var column = table.Columns.Single();
        const string columnName = "test_column";

        Assert.That(column.Name.LocalName, Is.EqualTo(columnName));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
    {
        var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
        var table = await GetTableAsync("table_test_table_4");
        var columns = table.Columns;
        var columnNames = columns.Select(c => c.Name.LocalName);

        Assert.That(columnNames, Is.EqualTo(expectedColumnNames));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.IsNullable, Is.True);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
    {
        const string tableName = "table_test_table_2";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNoneDefaultValue()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.DefaultValue, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.IsComputed, Is.False);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoincrement()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithSerialIdentity_ReturnsSomeAutoincrement()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement, OptionIs.Some);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithSerialIdentity_ReturnsCorrectInitialValue()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(1));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithSerialIdentity_ReturnsCorrectIncrement()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(1));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsSomeAutoincrement()
    {
        const string tableName = "table_test_table_36";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement, OptionIs.Some);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
    {
        const string tableName = "table_test_table_36";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(123));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
    {
        const string tableName = "table_test_table_36";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(456));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNoGeneratedColumns_ReturnsNoComputedColumns()
    {
        const string tableName = "table_test_table_1";
        var table = await GetTableAsync(tableName);
        var computedColumns = table.Columns.Where(c => c.IsComputed).ToList();

        Assert.That(computedColumns, Is.Empty);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnCount()
    {
        var dbVersion = await DatabaseProvider.GetDatabaseVersionAsync();
        if (dbVersion < new Version(12, 0))
        {
            Assert.Pass();
            return;
        }

        const string tableName = "table_test_table_37";
        var table = await GetTableAsync(tableName);
        var computedColumns = table.Columns.Where(c => c.IsComputed).ToList();

        Assert.That(computedColumns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnNames()
    {
        var dbVersion = await DatabaseProvider.GetDatabaseVersionAsync();
        if (dbVersion < new Version(12, 0))
        {
            Assert.Pass();
            return;
        }

        const string tableName = "table_test_table_37";
        const string expectedColumnName = "test_column_2";
        var table = await GetTableAsync(tableName);
        var computedColumn = table.Columns.Single(c => c.IsComputed);

        Assert.That(computedColumn.Name.LocalName, Is.EqualTo(expectedColumnName));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithJsonColumn_ReturnsColumnWithJsonDataType()
    {
        const string tableName = "table_test_table_39";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "json_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Json));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithJsonbColumn_ReturnsColumnWithJsonDataType()
    {
        const string tableName = "table_test_table_39";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "jsonb_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Json));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithXmlColumn_ReturnsColumnWithXmlDataType()
    {
        const string tableName = "table_test_table_40";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "xml_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Xml));
    }

    [TestCase("point_column")]
    [TestCase("line_column")]
    [TestCase("lseg_column")]
    [TestCase("box_column")]
    [TestCase("path_column")]
    [TestCase("polygon_column")]
    [TestCase("circle_column")]
    public async Task Columns_WhenGivenTableWithGeometricColumn_ReturnsColumnWithGeometryDataType(string columnName)
    {
        const string tableName = "table_test_table_41";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, columnName, StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Geometry));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithUuidColumn_ReturnsColumnWithUniqueIdentifierDataType()
    {
        const string tableName = "table_test_table_42";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "uuid_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.UniqueIdentifier));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeneratedColumns_ReturnsExpectedComputedColumnDefinition()
    {
        var dbVersion = await DatabaseProvider.GetDatabaseVersionAsync();
        if (dbVersion < new Version(12, 0))
        {
            Assert.Pass();
            return;
        }

        const string tableName = "table_test_table_37";
        const string expectedDefinition = "(test_column_1 * 2)";
        var table = await GetTableAsync(tableName);
        var computedColumn = table.Columns.Single(c => c.IsComputed) as IDatabaseComputedColumn;

        Assert.That(computedColumn.Definition.UnwrapSome(), Is.EqualTo(expectedDefinition));
    }
}