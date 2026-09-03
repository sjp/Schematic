using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
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
    public async Task Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
    {
        const string tableName = "table_test_table_33";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        const string defaultValue = "1";
        var comparer = new SqlServerExpressionComparer();
        var equals = comparer.Equals(defaultValue, column.DefaultValue.UnwrapSome());

        Assert.That(equals, Is.True);
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
    public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
    {
        const string tableName = "table_test_table_34";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "test_column_3", StringComparison.Ordinal));

        Assert.That(column.IsComputed, Is.True);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsCorrectDefinition()
    {
        const string tableName = "table_test_table_34";
        const string expectedDefinition = "([test_column_1]+[test_column_2])";

        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "test_column_3", StringComparison.Ordinal));

        Assert.That(column.ComputedDefinition.UnwrapSome(), Is.EqualTo(expectedDefinition));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNonPersistedComputedColumn_ReturnsVirtualStorage()
    {
        const string tableName = "table_test_table_34";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "test_column_3", StringComparison.Ordinal));

        Assert.That(column.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Virtual));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithPersistedComputedColumn_ReturnsStoredStorage()
    {
        const string tableName = "table_test_table_34";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "test_column_4", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.IsComputed, Is.True);
            Assert.That(column.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Stored));
        }
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
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsSomeAutoincrement()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement, OptionIs.Some);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(10));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(5));
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithIdentity_ReturnsByDefaultGenerationWithoutSequence()
    {
        const string tableName = "table_test_table_35";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];
        var autoIncrement = column.AutoIncrement.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(autoIncrement.Generation, Is.EqualTo(IdentityGeneration.ByDefault));
            Assert.That(autoIncrement.MinValue, OptionIs.None);
            Assert.That(autoIncrement.MaxValue, OptionIs.None);
            Assert.That(autoIncrement.SequenceName, OptionIs.None);
            Assert.That(autoIncrement.Cycle, Is.False);
        }
    }

    [Test]
    public async Task Columns_WhenGivenTableWithJsonColumn_ReturnsColumnWithJsonDataType()
    {
        if (!await DatabaseProvider.SupportsJsonDataType())
        {
            Assert.Pass();
            return;
        }

        const string tableName = "table_test_table_37";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "json_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Json));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithXmlColumn_ReturnsColumnWithXmlDataType()
    {
        const string tableName = "table_test_table_38";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "xml_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Xml));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeometryColumn_ReturnsColumnWithGeometryDataType()
    {
        const string tableName = "table_test_table_39";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "geometry_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Geometry));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeographyColumn_ReturnsColumnWithGeometryDataType()
    {
        const string tableName = "table_test_table_40";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "geography_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Geometry));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithUniqueIdentifierColumn_ReturnsColumnWithUniqueIdentifierDataType()
    {
        const string tableName = "table_test_table_41";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => string.Equals(c.Name.LocalName, "uniqueidentifier_column", StringComparison.Ordinal));

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.UniqueIdentifier));
    }
}