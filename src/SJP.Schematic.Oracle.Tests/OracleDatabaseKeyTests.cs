﻿using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDatabaseKeyTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        Assert.That(() => new OracleDatabaseKey(null, keyType, columns, enabled), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = (DatabaseKeyType)55;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        Assert.That(() => new OracleDatabaseKey(keyName, keyType, columns, enabled), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        const bool enabled = true;

        Assert.That(() => new OracleDatabaseKey(keyName, keyType, null, enabled), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var columns = Array.Empty<IDatabaseColumn>();
        const bool enabled = true;

        Assert.That(() => new OracleDatabaseKey(keyName, keyType, columns, enabled), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var columns = new IDatabaseColumn[] { null };
        const bool enabled = true;

        Assert.That(() => new OracleDatabaseKey(keyName, keyType, columns, enabled), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Primary;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

        Assert.That(key.Name.UnwrapSome(), Is.EqualTo(keyName));
    }

    [Test]
    public static void KeyType_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

        Assert.That(key.KeyType, Is.EqualTo(keyType));
    }

    [Test]
    public static void Columns_PropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

        Assert.That(key.Columns, Is.EqualTo(columns));
    }

    [Test]
    public static void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

        Assert.That(key.IsEnabled, Is.EqualTo(enabled));
    }

    [Test]
    public static void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
    {
        Identifier keyName = "test_key";
        const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = false;

        var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

        Assert.That(key.IsEnabled, Is.EqualTo(enabled));
    }

    [TestCase(DatabaseKeyType.Foreign, "test_foreign_key", "Foreign Key: test_foreign_key")]
    [TestCase(DatabaseKeyType.Primary, "test_primary_key", "Primary Key: test_primary_key")]
    [TestCase(DatabaseKeyType.Unique, "test_unique_key", "Unique Key: test_unique_key")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(DatabaseKeyType keyType, string name, string expectedResult)
    {
        var keyName = Identifier.CreateQualifiedIdentifier(name);
        var column = Mock.Of<IDatabaseColumn>();
        var columns = new[] { column };
        const bool enabled = true;

        var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);
        var result = key.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}