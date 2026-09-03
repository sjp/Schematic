using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDatabaseColumnTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        var columnType = Mock.Of<IDbType>();
        Assert.That(() => new OracleDatabaseColumn(null, columnType, true, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullType_ThrowsArgumentNullException()
    {
        Assert.That(() => new OracleDatabaseColumn("test_column", null, true, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, null);

        Assert.That(column.Name, Is.EqualTo(columnName));
    }

    [Test]
    public static void Type_PropertyGet_EqualsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, null);

        Assert.That(column.Type, Is.EqualTo(columnType));
    }

    [Test]
    public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var column = new OracleDatabaseColumn(columnName, columnType, false, null);

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var column = new OracleDatabaseColumn(columnName, columnType, true, null);

        Assert.That(column.IsNullable, Is.True);
    }

    [Test]
    public static void DefaultValue_PropertyGet_ReturnsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string defaultValue = "1";
        var column = new OracleDatabaseColumn(columnName, columnType, true, defaultValue);

        Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo(defaultValue));
    }

    [Test]
    public static void IsComputed_PropertyGet_ReturnsFalse()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var column = new OracleDatabaseColumn(columnName, columnType, true, null);

        Assert.That(column.IsComputed, Is.False);
    }

    [Test]
    public static void AutoIncrement_PropertyGet_ReturnsNone()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var column = new OracleDatabaseColumn(columnName, columnType, true, null);

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [Test]
    public static void AutoIncrement_GivenIdentityCtorArgument_ReturnsCtorArgument()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var autoIncrement = new AutoIncrement(1, 1, IdentityGeneration.Always, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.Some("ISEQ$$_12345"));
        var column = new OracleDatabaseColumn(columnName, columnType, true, null, Option<IAutoIncrement>.Some(autoIncrement));

        Assert.That(column.AutoIncrement.UnwrapSome(), Is.EqualTo(autoIncrement));
    }

    [Test]
    public static void Ctor_GivenInvalidComputedStorage_ThrowsArgumentException()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        Assert.That(() => new OracleDatabaseColumn(columnName, columnType, true, null, Option<IAutoIncrement>.None, true, Option<string>.Some("1"), (ComputedColumnStorage)55), Throws.ArgumentException);
    }

    [Test]
    public static void ComputedDefinition_GivenComputedCtorArgs_ReturnsCtorArgs()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "1 + 1";

        var column = new OracleDatabaseColumn(columnName, columnType, true, null, Option<IAutoIncrement>.None, true, Option<string>.Some(definition), ComputedColumnStorage.Virtual);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.IsComputed, Is.True);
            Assert.That(column.ComputedDefinition.UnwrapSome(), Is.EqualTo(definition));
            Assert.That(column.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Virtual));
        }
    }

    [Test]
    public static void ComputedDefinition_WhenColumnIsNotComputed_ReturnsNone()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.ComputedDefinition, OptionIs.None);
            Assert.That(column.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Unknown));
        }
    }

    [TestCase("test_column_1", "Computed Column: test_column_1")]
    [TestCase("test_column_2", "Computed Column: test_column_2")]
    public static void ToString_WhenGivenComputedColumn_ReturnsExpectedValues(string name, string expectedResult)
    {
        var columnName = Identifier.CreateQualifiedIdentifier(name);
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, null, Option<IAutoIncrement>.None, true, Option<string>.Some("1"), ComputedColumnStorage.Virtual);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("test_column_1", "Column: test_column_1")]
    [TestCase("test_column_2", "Column: test_column_2")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var columnName = Identifier.CreateQualifiedIdentifier(name);
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, null);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}