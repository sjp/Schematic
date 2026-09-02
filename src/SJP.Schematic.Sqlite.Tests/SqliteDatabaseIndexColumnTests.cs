using System;
using System.Collections.Generic;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteDatabaseIndexColumnTests
{
    [Test]
    public static void Ctor_GivenNullExpression_ThrowsArgumentNullException()
    {
        var column = Mock.Of<IDatabaseColumn>();

        Assert.That(
            () => new SqliteDatabaseIndexColumn(null, [column], IndexColumnOrder.Ascending, Option<Identifier>.None),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenEmptyExpression_ThrowsArgumentException()
    {
        var column = Mock.Of<IDatabaseColumn>();

        Assert.That(
            () => new SqliteDatabaseIndexColumn(string.Empty, [column], IndexColumnOrder.Ascending, Option<Identifier>.None),
            Throws.ArgumentException
        );
    }

    [Test]
    public static void Ctor_GivenNullDependentColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new SqliteDatabaseIndexColumn("\"test\"", null, IndexColumnOrder.Ascending, Option<Identifier>.None),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenInvalidOrder_ThrowsArgumentException()
    {
        var column = Mock.Of<IDatabaseColumn>();
        const IndexColumnOrder order = (IndexColumnOrder)55;

        Assert.That(
            () => new SqliteDatabaseIndexColumn("\"test\"", [column], order, Option<Identifier>.None),
            Throws.ArgumentException
        );
    }

    [Test]
    public static void Ctor_GivenNoDependentColumns_DoesNotThrow()
    {
        var indexColumn = new SqliteDatabaseIndexColumn("lower(test)", [], IndexColumnOrder.Ascending, Option<Identifier>.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.DependentColumns, Is.Empty);
            Assert.That(indexColumn.Expression, Is.EqualTo("lower(test)"));
        }
    }

    [Test]
    public static void Ctor_GivenCollation_SetsCollationToGivenValue()
    {
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new SqliteDatabaseIndexColumn("\"test\"", [column], IndexColumnOrder.Descending, Option<Identifier>.Some("NOCASE"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.Collation.UnwrapSome().LocalName, Is.EqualTo("NOCASE"));
            Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Descending));
            Assert.That(indexColumn.NullOrder, Is.EqualTo(IndexColumnNullOrder.Default));
            Assert.That(indexColumn.PrefixLength, OptionIs.None);
        }
    }

    [Test]
    public static void DependentColumns_WhenSourceCollectionMutatedAfterConstruction_RemainsUnchanged()
    {
        var dependentColumns = new List<IDatabaseColumn> { Mock.Of<IDatabaseColumn>() };

        var indexColumn = new SqliteDatabaseIndexColumn("\"test\"", dependentColumns, IndexColumnOrder.Ascending, Option<Identifier>.None);

        dependentColumns.Add(Mock.Of<IDatabaseColumn>());

        Assert.That(indexColumn.DependentColumns, Has.Count.EqualTo(1));
    }

    [TestCase("test_expression", "Index Column: test_expression")]
    [TestCase("test_expression_other", "Index Column: test_expression_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string expression, string expectedResult)
    {
        var column = Mock.Of<IDatabaseColumn>();

        var indexColumn = new SqliteDatabaseIndexColumn(expression, [column], IndexColumnOrder.Ascending, Option<Identifier>.None);
        var result = indexColumn.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
