using System;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.DependentColumns, Has.Exactly(1).Items);
            Assert.That(indexColumn.DependentColumns[0], Is.EqualTo(column));
        }
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
        const string expression = "`test`";
        var column = Mock.Of<IDatabaseColumn>();
        var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

        Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Ascending));
    }

    [Test]
    public static void Order_WhenDescendingProvidedInCtor_ReturnsDescending()
    {
        const string expression = "`test`";
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new MySqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Descending, Option<int>.None);

        Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Descending));
    }

    [Test]
    public static void PrefixLength_WhenProvidedInCtor_ReturnsGivenValue()
    {
        const string expression = "`test`";
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new MySqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending, Option<int>.Some(20));

        Assert.That(indexColumn.PrefixLength.UnwrapSome(), Is.EqualTo(20));
    }

    [Test]
    public static void DependentColumns_WhenConstructedFromAnExpression_ReturnsEmptyCollection()
    {
        const string expression = "(lower(`test`))";

        var indexColumn = new MySqlDatabaseIndexColumn(expression, IndexColumnOrder.Ascending);

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

        var indexColumn = new MySqlDatabaseIndexColumn(expression, column);
        var result = indexColumn.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}