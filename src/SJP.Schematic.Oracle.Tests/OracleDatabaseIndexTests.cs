using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDatabaseIndexTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        Assert.That(() => new OracleDatabaseIndex(null, isUnique, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;

        Assert.That(() => new OracleDatabaseIndex(indexName, isUnique, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var columns = Array.Empty<IDatabaseIndexColumn>();

        Assert.That(() => new OracleDatabaseIndex(indexName, isUnique, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var columns = new IDatabaseIndexColumn[] { null };

        Assert.That(() => new OracleDatabaseIndex(indexName, isUnique, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new OracleDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.Name, Is.EqualTo(indexName));
    }

    [Test]
    public static void IsUnique_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new OracleDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.IsUnique, Is.EqualTo(isUnique));
    }

    [Test]
    public static void IsUnique_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = false;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new OracleDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.IsUnique, Is.EqualTo(isUnique));
    }

    [Test]
    public static void Columns_PropertyGet_EqualsCtorArg()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new OracleDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.Columns, Is.EqualTo(columns));
    }

    [Test]
    public static void IsEnabled_PropertyGet_ReturnsTrue()
    {
        Identifier indexName = "test_index";
        const bool isUnique = true;
        var column = Mock.Of<IDatabaseIndexColumn>();
        var columns = new[] { column };

        var index = new OracleDatabaseIndex(indexName, isUnique, columns);

        Assert.That(index.IsEnabled, Is.True);
    }

    [TestCase("test_index", "Index: test_index")]
    [TestCase("test_index_other", "Index: test_index_other")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var indexName = Identifier.CreateQualifiedIdentifier(name);
        var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

        var index = new OracleDatabaseIndex(indexName, false, columns);
        var result = index.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}