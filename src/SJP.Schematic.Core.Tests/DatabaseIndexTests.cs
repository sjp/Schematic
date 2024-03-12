using System;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseIndexTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        Assert.That(() => new DatabaseIndex(null, false, columns, [], true, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";

        Assert.That(() => new DatabaseIndex(indexName, false, null, [], true, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        var columns = Array.Empty<IDatabaseIndexColumn>();

        Assert.That(() => new DatabaseIndex(indexName, false, columns, [], true, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        var columns = new IDatabaseIndexColumn[] { null };

        Assert.That(() => new DatabaseIndex(indexName, false, columns, [], true, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIncludedColumnSet_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        Assert.That(() => new DatabaseIndex(indexName, false, columns, null, true, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_DoesNotThrowArgumentNullException()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        Assert.That(() => new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenIncludedColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = new[] { (IDatabaseColumn)null };

        Assert.That(() => new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None);

        Assert.That(index.Name, Is.EqualTo(indexName));
    }

    [Test]
    public static void IsUnique_GivenFalseCtorArgPropertyGet_ReturnsFalse()
    {
        Identifier indexName = "test_index";
        const bool isUnique = false;
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, isUnique, columns, includedColumns, true, Option<string>.None);

        Assert.That(index.IsUnique, Is.EqualTo(isUnique));
    }

    [Test]
    public static void IsUnique_GivenTrueCtorArgPropertyGet_ReturnsTrue()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, isUnique, columns, includedColumns, true, Option<string>.None);

        Assert.That(index.IsUnique, Is.EqualTo(isUnique));
    }

    [Test]
    public static void IsEnabled_GivenFalseCtorArgPropertyGet_ReturnsFalse()
    {
        Identifier indexName = "test_index";
        const bool isEnabled = false;
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, isEnabled, Option<string>.None);

        Assert.That(index.IsEnabled, Is.EqualTo(isEnabled));
    }

    [Test]
    public static void IsEnabled_GivenTrueCtorArgPropertyGet_ReturnsTrue()
    {
        Identifier indexName = "test_index";
        const bool isEnabled = true;
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, isEnabled, Option<string>.None);

        Assert.That(index.IsEnabled, Is.EqualTo(isEnabled));
    }

    [Test]
    public static void Columns_PropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None);

        Assert.That(index.Columns, Is.EqualTo(columns));
    }

    [Test]
    public static void IncludedColumns_PropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = new[] { Mock.Of<IDatabaseColumn>() };

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None);

        Assert.That(index.IncludedColumns, Is.EqualTo(includedColumns));
    }

    [Test]
    public static void FilterDefinition_GivenNoneCtorArgPropertyGet_ReturnsNone()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None);

        Assert.That(index.FilterDefinition, OptionIs.None);
    }

    [Test]
    public static void FilterDefinition_GivenValueForCtorArgPropertyGet_ReturnsValue()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();
        const string filterDefinition = "WHERE a = 1";

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.Some(filterDefinition));

        Assert.Multiple(() =>
        {
            Assert.That(index.FilterDefinition, OptionIs.Some);
            Assert.That(index.FilterDefinition.UnwrapSome(), Is.EqualTo(filterDefinition));
        });
    }

    [TestCase("test_index", "Index: test_index")]
    [TestCase("test_index_other", "Index: test_index_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var indexName = Identifier.CreateQualifiedIdentifier(name);
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = new[] { Mock.Of<IDatabaseColumn>() };

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None);
        var result = index.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}