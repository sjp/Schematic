using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

internal static class OracleDatabaseIndexColumnTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceExpression_ThrowsArgumentException(string expression)
    {
        var column = Mock.Of<IDatabaseColumn>();

        Assert.That(
            () => new OracleDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("expression")
        );
    }

    [Test]
    public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
    {
        const string expression = "\"test\"";

        Assert.That(
            () => new OracleDatabaseIndexColumn(expression, null, IndexColumnOrder.Ascending),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("column")
        );
    }

    [Test]
    public static void DependentColumns_PropertyGet_EqualsCtorArg()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new OracleDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.DependentColumns, Has.Exactly(1).Items);
            Assert.That(indexColumn.DependentColumns[0], Is.EqualTo(column));
        }
    }

    [Test]
    public static void Expression_PropertyGet_EqualsCtorArg()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new OracleDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

        Assert.That(indexColumn.Expression, Is.EqualTo(expression));
    }

    [Test]
    public static void Order_WhenDescendingProvidedInCtor_ReturnsDescending()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new OracleDatabaseIndexColumn(expression, column, IndexColumnOrder.Descending);

        Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Descending));
    }

    [Test]
    public static void DependentColumns_WhenConstructedFromAnExpression_ReturnsEmptyCollection()
    {
        const string expression = "\"SYS_NC00004$\"";

        var indexColumn = new OracleDatabaseIndexColumn(expression, IndexColumnOrder.Ascending);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.DependentColumns, Is.Empty);
            Assert.That(indexColumn.Expression, Is.EqualTo(expression));
        }
    }

    [TestCase("test_expression", "Index Column: test_expression")]
    [TestCase("test_expression_other", "Index Column: test_expression_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string expression, string expectedResult)
    {
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new OracleDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);
        var result = indexColumn.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
