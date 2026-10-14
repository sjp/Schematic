using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Oracle.Queries;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

internal static class OracleCatalogMapperTests
{
    [TestCase("NO ACTION", ReferentialAction.NoAction)]
    [TestCase("RESTRICT", ReferentialAction.Restrict)]
    [TestCase("CASCADE", ReferentialAction.Cascade)]
    [TestCase("SET NULL", ReferentialAction.SetNull)]
    [TestCase("SET DEFAULT", ReferentialAction.SetDefault)]
    [TestCase("set null", ReferentialAction.SetNull)]
    public static void GetReferentialAction_GivenKnownDeleteRule_ReturnsExpectedAction(string deleteRule, ReferentialAction expected)
    {
        Assert.That(OracleCatalogMapper.GetReferentialAction(deleteRule), Is.EqualTo(expected));
    }

    [Test]
    public static void GetReferentialAction_GivenNullDeleteRule_ReturnsNoAction()
    {
        Assert.That(OracleCatalogMapper.GetReferentialAction(null), Is.EqualTo(ReferentialAction.NoAction));
    }

    [TestCase("")]
    [TestCase("    ")]
    [TestCase("SOMETHING UNEXPECTED")]
    public static void GetReferentialAction_GivenUnknownDeleteRule_ReturnsNoAction(string deleteRule)
    {
        Assert.That(OracleCatalogMapper.GetReferentialAction(deleteRule), Is.EqualTo(ReferentialAction.NoAction));
    }

    [TestCase("NUMBER", 10, 2, 10, 2)]
    [TestCase("NUMBER", 5, 0, 5, 0)]
    [TestCase("FLOAT", 126, 0, 126, 0)]
    [TestCase("INTERVAL DAY(2) TO SECOND(6)", 2, 6, 2, 6)]
    public static void GetNumericPrecision_GivenPrecision_ReturnsPrecisionAndScale(string typeName, int precision, int scale, int expectedPrecision, int expectedScale)
    {
        var result = OracleCatalogMapper.GetNumericPrecision(typeName, precision, scale);

        Assert.That(result, OptionIs.Some);
        var numericPrecision = result.UnwrapSome();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(expectedPrecision));
            Assert.That(numericPrecision.Scale, Is.EqualTo(expectedScale));
        }
    }

    [Test]
    public static void GetNumericPrecision_GivenNegativeScale_ClampsScaleToZero()
    {
        var result = OracleCatalogMapper.GetNumericPrecision("NUMBER", 5, -2);

        Assert.That(result, OptionIs.Some);
        var numericPrecision = result.UnwrapSome();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(5));
            Assert.That(numericPrecision.Scale, Is.Zero);
        }
    }

    // NUMBER(*, s), where INTEGER is NUMBER(*, 0)
    [TestCase(2, 2)]
    [TestCase(0, 0)]
    [TestCase(-2, 0)]
    public static void GetNumericPrecision_GivenNumberWithScaleButNoPrecision_ReturnsMaximumPrecision(int scale, int expectedScale)
    {
        var result = OracleCatalogMapper.GetNumericPrecision("NUMBER", null, scale);

        Assert.That(result, OptionIs.Some);
        var numericPrecision = result.UnwrapSome();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(38));
            Assert.That(numericPrecision.Scale, Is.EqualTo(expectedScale));
        }
    }

    [TestCase("TIMESTAMP(6)", null, 6)]
    [TestCase("TIMESTAMP(3) WITH TIME ZONE", null, 3)]
    [TestCase("TIMESTAMP(9) WITH LOCAL TIME ZONE", null, 9)]
    [TestCase("INTERVAL DAY(0) TO SECOND(6)", 0, 6)]
    [TestCase("NUMBER", null, null)]
    [TestCase("VARCHAR2", null, null)]
    [TestCase(null, null, 4)]
    public static void GetNumericPrecision_GivenNoPrecisionForType_ReturnsNone(string typeName, int? precision, int? scale)
    {
        var result = OracleCatalogMapper.GetNumericPrecision(typeName, precision, scale);

        Assert.That(result, OptionIs.None);
    }

    [TestCase(200, 100, "C", 100)]
    [TestCase(40, 10, "C", 10)]
    [TestCase(100, 100, "B", 100)]
    [TestCase(22, 0, null, 22)]
    public static void GetMaxLength_GivenCatalogLengths_ReturnsDeclaredLength(int dataLength, int charLength, string charUsed, int expectedLength)
    {
        Assert.That(OracleCatalogMapper.GetMaxLength(dataLength, charLength, charUsed), Is.EqualTo(expectedLength));
    }

    [TestCase("C", true)]
    [TestCase("B", false)]
    [TestCase("0", false)]
    [TestCase(null, false)]
    public static void HasCharacterLengthSemantics_GivenCharUsed_ReturnsExpectedResult(string charUsed, bool expected)
    {
        Assert.That(OracleCatalogMapper.HasCharacterLengthSemantics(charUsed), Is.EqualTo(expected));
    }

    [TestCase("ASC", IndexColumnOrder.Ascending)]
    [TestCase("DESC", IndexColumnOrder.Descending)]
    public static void MapIndexes_GivenDescendValue_ReturnsExpectedOrder(string descend, IndexColumnOrder expected)
    {
        var rows = new[] { CreateIndexRow("TEST_COLUMN", descend, null) };

        var indexColumn = MapSingleIndexColumn(rows, "TEST_COLUMN");

        Assert.That(indexColumn.Order, Is.EqualTo(expected));
    }

    [Test]
    public static void MapIndexes_GivenPlainColumn_ResolvesColumn()
    {
        var rows = new[] { CreateIndexRow("TEST_COLUMN", "ASC", null) };

        var indexColumn = MapSingleIndexColumn(rows, "TEST_COLUMN");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.Expression, Is.EqualTo("\"TEST_COLUMN\""));
            Assert.That(indexColumn.DependentColumns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "TEST_COLUMN" }));
        }
    }

    [Test]
    public static void MapIndexes_GivenDescendingColumnOverHiddenColumn_ResolvesColumnFromExpression()
    {
        var rows = new[] { CreateIndexRow("SYS_NC00003$", "DESC", "\"TEST_COLUMN\"") };

        var indexColumn = MapSingleIndexColumn(rows, "TEST_COLUMN");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.Expression, Is.EqualTo("\"TEST_COLUMN\""));
            Assert.That(indexColumn.DependentColumns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "TEST_COLUMN" }));
            Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Descending));
        }
    }

    [TestCase("UPPER(\"TEST_COLUMN\")")]
    [TestCase("\"TEST_COLUMN\"+1")]
    [TestCase("\"A\"||\"B\"")]
    public static void MapIndexes_GivenFunctionBasedColumn_ReturnsExpressionWithoutDependentColumns(string expression)
    {
        var rows = new[] { CreateIndexRow("SYS_NC00004$", "ASC", expression) };

        var indexColumn = MapSingleIndexColumn(rows, "TEST_COLUMN");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.Expression, Is.EqualTo(expression));
            Assert.That(indexColumn.DependentColumns, Is.Empty);
        }
    }

    private static GetTableIndexes.Result CreateIndexRow(string columnName, string descend, string columnExpression) => new()
    {
        IndexName = "IX_TEST",
        Uniqueness = "NONUNIQUE",
        IndexType = "NORMAL",
        Status = "VALID",
        Visibility = "VISIBLE",
        Descend = descend,
        ColumnName = columnName,
        ColumnExpression = columnExpression,
        ColumnPosition = 1,
    };

    private static IDatabaseIndexColumn MapSingleIndexColumn(IEnumerable<GetTableIndexes.Result> rows, string tableColumnName)
    {
        var column = Mock.Of<IDatabaseColumn>(c => c.Name == Identifier.CreateQualifiedIdentifier(tableColumnName));
        var columnLookup = new Dictionary<Identifier, IDatabaseColumn> { [column.Name] = column };

        var index = OracleCatalogMapper.MapIndexes(rows, columnLookup, new OracleDialect()).Single();
        return index.Columns.Single();
    }
}
