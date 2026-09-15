using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests;

internal static class MySqlDatabasePrimaryKeyTests
{
    [Test]
    public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new MySqlDatabasePrimaryKey(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentException()
    {
        var columns = Array.Empty<IDatabaseColumn>();
        Assert.That(
            () => new MySqlDatabasePrimaryKey(columns),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        var columns = new IDatabaseColumn[] { null };
        Assert.That(
            () => new MySqlDatabasePrimaryKey(columns),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void Name_PropertyGet_EqualsPrimary()
    {
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabasePrimaryKey(columns);

        Assert.That(key.Name.UnwrapSome().LocalName, Is.EqualTo("PRIMARY"));
    }

    [Test]
    public static void KeyType_PropertyGet_EqualsPrimary()
    {
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabasePrimaryKey(columns);

        Assert.That(key.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
    }

    [Test]
    public static void Columns_PropertyGet_EqualsCtorArg()
    {
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabasePrimaryKey(columns);

        Assert.That(key.Columns, Is.EqualTo(columns));
    }

    [TestCase("Primary Key: PRIMARY")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string expectedResult)
    {
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabasePrimaryKey(columns);
        var result = key.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}