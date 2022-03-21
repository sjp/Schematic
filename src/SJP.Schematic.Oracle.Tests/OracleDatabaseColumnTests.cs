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