using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlDatabaseIndexTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        Assert.That(() => new MySqlDatabaseIndex(null, isUnique, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;

        Assert.That(() => new MySqlDatabaseIndex(indexName, isUnique, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentException()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var columns = Array.Empty<IDatabaseIndexColumn>();

        Assert.That(() => new MySqlDatabaseIndex(indexName, isUnique, columns), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var columns = new IDatabaseIndexColumn[] { null };

        Assert.That(() => new MySqlDatabaseIndex(indexName, isUnique, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new MySqlDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.Name, Is.EqualTo(indexName));
    }

    [Test]
    public static void IsUnique_WithTrueCtorArgPropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new MySqlDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.IsUnique, Is.True);
    }

    [Test]
    public static void IsUnique_WithFalseCtorArgPropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = false;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new MySqlDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.IsUnique, Is.False);
    }

    [Test]
    public static void Columns_PropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new MySqlDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.Columns, Is.EqualTo(columns));
    }

    [Test]
    public static void IncludedColumns_PropertyGet_AreEmpty()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new MySqlDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.IncludedColumns, Is.Empty);
    }

    [Test]
    public static void IsEnabled_PropertyGet_ReturnsTrue()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new MySqlDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.IsEnabled, Is.True);
    }

    [Test]
    public static void FilterDefinition_PropertyGet_ReturnsNone()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        var index = new MySqlDatabaseIndex(indexName, false, columns);

        Assert.That(index.FilterDefinition, OptionIs.None);
    }

    [Test]
    public static void Ctor_GivenInvalidIndexType_ThrowsArgumentException()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
        const IndexType indexType = (IndexType)55;

        Assert.That(() => new MySqlDatabaseIndex(indexName, false, columns, indexType, true), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNoPhysicalProperties_DefaultsToAnUnknownVisibleIndex()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        var index = new MySqlDatabaseIndex(indexName, false, columns);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.IndexType, Is.EqualTo(IndexType.Unknown));
            Assert.That(index.IsVisible, Is.True);
            Assert.That(index.IsValid, Is.True);
            Assert.That(index.FillFactor, OptionIs.None);
        }
    }

    [Test]
    public static void Ctor_GivenPhysicalProperties_SetsPropertiesToGivenValues()
    {
        Identifier indexName = "test_index";
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        var index = new MySqlDatabaseIndex(indexName, false, columns, IndexType.FullText, false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.IndexType, Is.EqualTo(IndexType.FullText));
            Assert.That(index.IsVisible, Is.False);
        }
    }

    [TestCase("test_index", "Index: test_index")]
    [TestCase("test_index_other", "Index: test_index_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var indexName = Identifier.CreateQualifiedIdentifier(name);
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        var index = new MySqlDatabaseIndex(indexName, false, columns);
        var result = index.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}