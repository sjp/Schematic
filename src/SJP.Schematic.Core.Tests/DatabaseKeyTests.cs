using System;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseKeyTests
{
    [Test]
    public static void Ctor_GivenNullName_DoesNotThrowArgumentNullException()
    {
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        Assert.That(() => new DatabaseKey(null, keyType, columns, true), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = (DatabaseKeyType)55;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        Assert.That(() => new DatabaseKey(keyName, keyType, columns, true), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;

        Assert.That(() => new DatabaseKey(keyName, keyType, null, true), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var columns = Array.Empty<IDatabaseColumn>();

        Assert.That(() => new DatabaseKey(keyName, keyType, columns, true), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var columns = new IDatabaseColumn[] { null };

        Assert.That(() => new DatabaseKey(keyName, keyType, columns, true), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new DatabaseKey(keyName, keyType, columns, true);

        Assert.That(key.Name.UnwrapSome(), Is.EqualTo(keyName));
    }

    [Test]
    public static void KeyType_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new DatabaseKey(keyName, keyType, columns, true);

        Assert.That(key.KeyType, Is.EqualTo(keyType));
    }

    [Test]
    public static void Columns_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };

        var key = new DatabaseKey(keyName, keyType, columns, true);

        Assert.That(key.Columns, Is.EqualTo(columns));
    }

    [Test]
    public static void IsEnabled_WhenGivenTrueInCtor_ReturnsTrue()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        var key = new DatabaseKey(keyName, keyType, columns, enabled);

        Assert.That(key.IsEnabled, Is.EqualTo(enabled));
    }

    [Test]
    public static void IsEnabled_WhenGivenFalseInCtor_ReturnsFalse()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = false;

        var key = new DatabaseKey(keyName, keyType, columns, enabled);

        Assert.That(key.IsEnabled, Is.EqualTo(enabled));
    }

    [TestCase(DatabaseKeyType.Foreign, null, "Foreign Key")]
    [TestCase(DatabaseKeyType.Foreign, "test_foreign_key", "Foreign Key: test_foreign_key")]
    [TestCase(DatabaseKeyType.Primary, null, "Primary Key")]
    [TestCase(DatabaseKeyType.Primary, "test_primary_key", "Primary Key: test_primary_key")]
    [TestCase(DatabaseKeyType.Unique, null, "Unique Key")]
    [TestCase(DatabaseKeyType.Unique, "test_unique_key", "Unique Key: test_unique_key")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(DatabaseKeyType keyType, string name, string expectedResult)
    {
        var keyName = !name.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(name))
            : Option<Identifier>.None;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        var key = new DatabaseKey(keyName, keyType, columns, enabled);
        var result = key.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}