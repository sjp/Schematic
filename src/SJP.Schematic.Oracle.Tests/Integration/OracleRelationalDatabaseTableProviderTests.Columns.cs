using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
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
        const string columnName = "TEST_COLUMN";

        Assert.That(column.Name.LocalName, Is.EqualTo(columnName));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
    {
        var expectedColumnNames = new[] { "FIRST_NAME", "MIDDLE_NAME", "LAST_NAME" };
        var table = await GetTableAsync("table_test_table_4");
        var columns = table.Columns;
        var columnNames = columns.Select(c => c.Name.LocalName);

        Assert.That(columnNames, Is.EqualTo(expectedColumnNames));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
    {
        const string tableName = "TABLE_TEST_TABLE_1";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.IsNullable, Is.True);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
    {
        const string tableName = "TABLE_TEST_TABLE_2";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNoneDefaultValue()
    {
        const string tableName = "TABLE_TEST_TABLE_1";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.DefaultValue, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
    {
        const string tableName = "TABLE_TEST_TABLE_33";
        var table = await GetTableAsync(tableName);
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
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.IsComputed, Is.False);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
    {
        const string tableName = "TABLE_TEST_TABLE_34";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.IsComputed, Is.True);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsCorrectDefinition()
    {
        const string tableName = "TABLE_TEST_TABLE_34";
        const string expectedDefinition = "\"TEST_COLUMN_1\"+\"TEST_COLUMN_2\"";

        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.ComputedDefinition.UnwrapSome(), Is.EqualTo(expectedDefinition));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsVirtualStorage()
    {
        const string tableName = "TABLE_TEST_TABLE_34";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Virtual));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumn_ReturnsNoDefaultValue()
    {
        const string tableName = "TABLE_TEST_TABLE_34";
        var table = await GetTableAsync(tableName);
        var column = table.Columns[table.Columns.Count - 1];

        Assert.That(column.DefaultValue, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNoneAutoIncrement()
    {
        const string tableName = "TABLE_TEST_TABLE_1";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithAlwaysIdentity_ReturnsIdentityMetadata()
    {
        const string tableName = "TABLE_TEST_TABLE_39";
        var table = await GetTableAsync(tableName);
        var autoIncrement = table.Columns.Single().AutoIncrement.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(autoIncrement.Generation, Is.EqualTo(IdentityGeneration.Always));
            Assert.That(autoIncrement.InitialValue, Is.EqualTo(10));
            Assert.That(autoIncrement.Increment, Is.EqualTo(5));
            Assert.That(autoIncrement.MinValue.UnwrapSome(), Is.EqualTo(1));
            Assert.That(autoIncrement.MaxValue.UnwrapSome(), Is.EqualTo(900));
            Assert.That(autoIncrement.Cycle, Is.True);
            Assert.That(autoIncrement.SequenceName, OptionIs.Some);
        }
    }

    [Test]
    public async Task Columns_WhenGivenTableColumnWithDefaultOnNullIdentity_ReturnsByDefaultOnNullGeneration()
    {
        const string tableName = "TABLE_TEST_TABLE_40";
        var table = await GetTableAsync(tableName);
        var autoIncrement = table.Columns.Single().AutoIncrement.UnwrapSome();

        Assert.That(autoIncrement.Generation, Is.EqualTo(IdentityGeneration.ByDefaultOnNull));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithJsonColumn_ReturnsColumnWithJsonDataType()
    {
        if (!DatabaseProvider.SupportsJsonDataType())
            Assert.Ignore("Native json data type requires Oracle Database 21c or later.");

        const string tableName = "TABLE_TEST_TABLE_36";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Json));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithXmlColumn_ReturnsColumnWithXmlDataType()
    {
        const string tableName = "TABLE_TEST_TABLE_37";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Xml));
    }

    [Test]
    public async Task Columns_WhenGivenTableWithGeometryColumn_ReturnsColumnWithGeometryDataType()
    {
        const string tableName = "TABLE_TEST_TABLE_38";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single();

        Assert.That(column.Type.DataType, Is.EqualTo(DataType.Geometry));
    }

    [TestCase("TIMESTAMP_COLUMN", 6)]
    [TestCase("TIMESTAMP_TZ_COLUMN", 3)]
    [TestCase("INTERVAL_COLUMN", 6)]
    public async Task Columns_WhenGivenTableWithFractionalSecondsColumn_ReturnsFractionalSecondsPrecisionAndNoNumericPrecision(string columnName, int expectedFractionalSecondsPrecision)
    {
        const string tableName = "TABLE_TEST_TABLE_43";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => c.Name.LocalName == columnName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.Type.FractionalSecondsPrecision.UnwrapSome(), Is.EqualTo(expectedFractionalSecondsPrecision));
            Assert.That(column.Type.NumericPrecision, OptionIs.None);
        }
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNegativeScaleNumberColumn_ReturnsZeroScale()
    {
        const string tableName = "TABLE_TEST_TABLE_43";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => c.Name.LocalName == "NEGATIVE_SCALE_COLUMN");
        var numericPrecision = column.Type.NumericPrecision.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(5));
            Assert.That(numericPrecision.Scale, Is.Zero);
        }
    }

    [Test]
    public async Task Columns_WhenGivenTableWithNumberColumnWithScaleOnly_ReturnsMaximumPrecision()
    {
        const string tableName = "TABLE_TEST_TABLE_43";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => c.Name.LocalName == "UNBOUNDED_PRECISION_COLUMN");
        var numericPrecision = column.Type.NumericPrecision.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(38));
            Assert.That(numericPrecision.Scale, Is.EqualTo(2));
        }
    }

    // the catalog reports a NUMBER's storage size and the byte length of a length declared in
    // characters, neither of which is part of the column's declaration
    [TestCase("NUMBER_COLUMN", "\"NUMBER\"", 22, DataType.Numeric)]
    [TestCase("INTEGER_COLUMN", "\"NUMBER\"(38)", 22, DataType.BigInteger)]
    [TestCase("NVARCHAR_COLUMN", "\"NVARCHAR2\"(100)", 100, DataType.Unicode)]
    [TestCase("NCHAR_COLUMN", "\"NCHAR\"(3)", 3, DataType.Unicode)]
    [TestCase("CHAR_SEMANTICS_VARCHAR_COLUMN", "\"VARCHAR2\"(10 CHAR)", 10, DataType.String)]
    [TestCase("CHAR_SEMANTICS_CHAR_COLUMN", "\"CHAR\"(5 CHAR)", 5, DataType.String)]
    [TestCase("BYTE_SEMANTICS_VARCHAR_COLUMN", "\"VARCHAR2\"(100)", 100, DataType.String)]
    public async Task Columns_WhenGivenTableWithLengthOrPrecisionColumn_ReturnsDeclaredType(string columnName, string expectedDefinition, int expectedMaxLength, DataType expectedDataType)
    {
        const string tableName = "TABLE_TEST_TABLE_45";
        var table = await GetTableAsync(tableName);
        var column = table.Columns.Single(c => c.Name.LocalName == columnName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.Type.Definition, Is.EqualTo(expectedDefinition));
            Assert.That(column.Type.MaxLength, Is.EqualTo(expectedMaxLength));
            Assert.That(column.Type.DataType, Is.EqualTo(expectedDataType));
        }
    }
}
