using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDatabaseIndexColumnTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceExpression_ThrowsArgumentException(string expression)
    {
        Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, IndexColumnOrder.Ascending), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceExpressionWithColumn_ThrowsArgumentException(string expression)
    {
        var column = Mock.Of<IDatabaseColumn>();

        Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
    {
        Assert.That(() => new PostgreSqlDatabaseIndexColumn("test", null, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidColumnOrder_ThrowsArgumentException()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();
        const IndexColumnOrder order = (IndexColumnOrder)55;

        Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, column, order), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenInvalidColumnOrderWithoutColumn_ThrowsArgumentException()
    {
        const string expression = "\"test\"";
        const IndexColumnOrder order = (IndexColumnOrder)55;

        Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, order), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_WhenGivenValidInputWithoutColumn_DoesNotThrow()
    {
        const string expression = "\"test\"";

        Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, IndexColumnOrder.Ascending), Throws.Nothing);
    }

    [Test]
    public static void Expression_PropertyGet_EqualsCtorArg()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

        Assert.That(indexColumn.Expression, Is.EqualTo(expression));
    }

    [Test]
    public static void DependentColumns_PropertyGet_EqualsCtorArg()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.DependentColumns, Has.Exactly(1).Items);
            Assert.That(indexColumn.DependentColumns[0], Is.EqualTo(column));
        }
    }

    [Test]
    public static void Order_WithAscendingCtorArgPropertyGet_EqualsCtorArg()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

        Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Ascending));
    }

    [Test]
    public static void Order_WithDescendingCtorArgPropertyGet_EqualsCtorArg()
    {
        const string expression = "\"test\"";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Descending);

        Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Descending));
    }

    [TestCase("test_expression", "Index Column: test_expression")]
    [TestCase("test_expression_other", "Index Column: test_expression_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string expression, string expectedResult)
    {
        var column = Mock.Of<IDatabaseColumn>();
        const IndexColumnOrder order = IndexColumnOrder.Ascending;

        var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, order);
        var result = indexColumn.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}