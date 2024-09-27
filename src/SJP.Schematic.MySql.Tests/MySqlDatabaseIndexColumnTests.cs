using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlDatabaseIndexColumnTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceExpression_ThrowsArgumentException(string expression)
    {
        var column = Mock.Of<IDatabaseColumn>();

        Assert.That(() => new MySqlDatabaseIndexColumn(expression, column), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
    {
        const string expression = "`test`";

        Assert.That(() => new MySqlDatabaseIndexColumn(expression, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void DependentColumns_PropertyGet_EqualsCtorArg()
    {
        const string expression = "`test`";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

        Assert.Multiple(() =>
        {
            Assert.That(indexColumn.DependentColumns, Has.Exactly(1).Items);
            Assert.That(indexColumn.DependentColumns[0], Is.EqualTo(column));
        });
    }

    [Test]
    public static void Expression_PropertyGet_EqualsCtorArg()
    {
        const string expression = "`test`";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

        Assert.That(indexColumn.Expression, Is.EqualTo(expression));
    }

    [Test]
    public static void Order_PropertyGet_EqualsAscending()
    {
        // MySQL doesn't support descending ordering so this is always true
        const string expression = "`test`";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

        Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Ascending));
    }

    [TestCase("test_expression", "Index Column: test_expression")]
    [TestCase("test_expression_other", "Index Column: test_expression_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string expression, string expectedResult)
    {
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new MySqlDatabaseIndexColumn(expression, column);
        var result = indexColumn.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}