using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlDatabaseKeyTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        Assert.That(() => new MySqlDatabaseKey(null, keyType, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = (DatabaseKeyType)55;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        Assert.That(() => new MySqlDatabaseKey(keyName, keyType, columns), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;

        Assert.That(() => new MySqlDatabaseKey(keyName, keyType, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var columns = Array.Empty<IDatabaseColumn>();

        Assert.That(() => new MySqlDatabaseKey(keyName, keyType, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var columns = new IDatabaseColumn[] { null };

        Assert.That(() => new MySqlDatabaseKey(keyName, keyType, columns), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabaseKey(keyName, keyType, columns);

        Assert.That(key.Name.UnwrapSome(), Is.EqualTo(keyName));
    }

    [Test]
    public static void KeyType_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabaseKey(keyName, keyType, columns);

        Assert.That(key.KeyType, Is.EqualTo(keyType));
    }

    [Test]
    public static void Columns_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabaseKey(keyName, keyType, columns);

        Assert.That(key.Columns, Is.EqualTo(columns));
    }

    [TestCase(DatabaseKeyType.Foreign, "test_foreign_key", "Foreign Key: test_foreign_key")]
    [TestCase(DatabaseKeyType.Primary, "test_primary_key", "Primary Key: test_primary_key")]
    [TestCase(DatabaseKeyType.Unique, "test_unique_key", "Unique Key: test_unique_key")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(DatabaseKeyType keyType, string name, string expectedResult)
    {
        var keyName = Identifier.CreateQualifiedIdentifier(name);
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new MySqlDatabaseKey(keyName, keyType, columns);
        var result = key.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
