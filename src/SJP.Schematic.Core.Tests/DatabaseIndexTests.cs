using System;
using System.Collections.Generic;
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
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentException()
    {
        Identifier indexName = "test_index";
        var columns = Array.Empty<IDatabaseIndexColumn>();

        Assert.That(() => new DatabaseIndex(indexName, false, columns, [], true, Option<string>.None), Throws.ArgumentException);
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
    public static void Name_GivenQualifiedCtorArg_PropertyGetReturnsLocalNameOnly()
    {
        var indexName = Identifier.CreateQualifiedIdentifier("test_schema", "test_index");
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = Array.Empty<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None);

        Assert.That(index.Name, Is.EqualTo(Identifier.CreateQualifiedIdentifier("test_index")));
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.FilterDefinition, OptionIs.Some);
            Assert.That(index.FilterDefinition.UnwrapSome(), Is.EqualTo(filterDefinition));
        }
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

    [Test]
    public static void Ctor_GivenInvalidIndexType_ThrowsArgumentException()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        const IndexType indexType = (IndexType)55;

        Assert.That(
            () => new DatabaseIndex(indexName, false, columns, [], true, Option<string>.None, indexType, Option<int>.None, true, true),
            Throws.ArgumentException
        );
    }

    [Test]
    public static void Ctor_GivenNoPhysicalProperties_SetsPropertiesToDefaults()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        var index = new DatabaseIndex(indexName, false, columns, [], true, Option<string>.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.IndexType, Is.EqualTo(IndexType.Unknown));
            Assert.That(index.FillFactor, OptionIs.None);
            Assert.That(index.IsValid, Is.True);
            Assert.That(index.IsVisible, Is.True);
        }
    }

    [Test]
    public static void Ctor_GivenPhysicalProperties_SetsPropertiesToGivenValues()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        var index = new DatabaseIndex(indexName, false, columns, [], true, Option<string>.None, IndexType.Clustered, Option<int>.Some(70), false, false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.IndexType, Is.EqualTo(IndexType.Clustered));
            Assert.That(index.FillFactor, OptionIs.Some);
            Assert.That(index.FillFactor.UnwrapSome(), Is.EqualTo(70));
            Assert.That(index.IsValid, Is.False);
            Assert.That(index.IsVisible, Is.False);
        }
    }

    [Test]
    public static void Columns_WhenSourceCollectionsMutatedAfterConstruction_RemainUnchanged()
    {
        Identifier indexName = "test_index";
        var columns = new List<IDatabaseIndexColumn> { Mock.Of<IDatabaseIndexColumn>() };
        var includedColumns = new List<IDatabaseColumn>();

        var index = new DatabaseIndex(indexName, false, columns, includedColumns, true, Option<string>.None);

        columns.Add(Mock.Of<IDatabaseIndexColumn>());
        includedColumns.Add(Mock.Of<IDatabaseColumn>());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.Columns, Has.Count.EqualTo(1));
            Assert.That(index.IncludedColumns, Is.Empty);
        }
    }
}