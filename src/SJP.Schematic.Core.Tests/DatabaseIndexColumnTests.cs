using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseIndexColumnTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceExpression_ThrowsArgumentNullException(string expression)
    {
        var column = Mock.Of<IDatabaseColumn>();

        Assert.That(() => new DatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
    {
        const string expression = "lower(test_column)";

        Assert.That(() => new DatabaseIndexColumn(expression, (IDatabaseColumn)null, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDependentColumns_ThrowsArgumentNullException()
    {
        const string expression = "lower(test_column)";

        Assert.That(() => new DatabaseIndexColumn(expression, (IEnumerable<IDatabaseColumn>)null, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenDependentColumnsWithNullValue_ThrowsArgumentNullException()
    {
        const string expression = "lower(test_column)";
        var columns = new IDatabaseColumn[] { null };

        Assert.That(() => new DatabaseIndexColumn(expression, columns, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidIndexColumnOrder_ThrowsArgumentException()
    {
        const string expression = "lower(test_column)";
        var column = Mock.Of<IDatabaseColumn>();
        const IndexColumnOrder order = (IndexColumnOrder)55;

        Assert.That(() => new DatabaseIndexColumn(expression, column, order), Throws.ArgumentException);
    }

    [Test]
    public static void Expression_PropertyGet_EqualsCtorArg()
    {
        const string expression = "lower(test_column)";
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new DatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

        Assert.That(indexColumn.Expression, Is.EqualTo(expression));
    }

    [Test]
    public static void DependentColumns_PropertyGet_EqualsCtorArg()
    {
        const string expression = "lower(test_column)";
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new DatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);
        var indexDependentColumn = indexColumn.DependentColumns.Single();

        Assert.That(indexDependentColumn, Is.EqualTo(column));
    }

    [Test]
    public static void Order_WhenAscendingProvidedInCtor_ReturnsAscending()
    {
        const string expression = "lower(test_column)";
        var column = Mock.Of<IDatabaseColumn>();
        const IndexColumnOrder order = IndexColumnOrder.Ascending;

        var indexColumn = new DatabaseIndexColumn(expression, column, order);

        Assert.That(indexColumn.Order, Is.EqualTo(order));
    }

    [Test]
    public static void Order_WhenDescendingProvidedInCtor_ReturnsDescending()
    {
        const string expression = "lower(test_column)";
        var column = Mock.Of<IDatabaseColumn>();
        const IndexColumnOrder order = IndexColumnOrder.Descending;

        var indexColumn = new DatabaseIndexColumn(expression, column, order);

        Assert.That(indexColumn.Order, Is.EqualTo(order));
    }

    [TestCase("test_expression", "Index Column: test_expression")]
    [TestCase("test_expression_other", "Index Column: test_expression_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string expression, string expectedResult)
    {
        var column = Mock.Of<IDatabaseColumn>();
        const IndexColumnOrder order = IndexColumnOrder.Ascending;

        var indexColumn = new DatabaseIndexColumn(expression, column, order);
        var result = indexColumn.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
