using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDatabaseComputedColumnTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";

        Assert.That(() => new OracleDatabaseComputedColumn(null, columnType, true, definition), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullType_ThrowsArgumentNullException()
    {
        const string definition = "test";

        Assert.That(() => new OracleDatabaseComputedColumn("test_column", null, true, definition), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";
        var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

        Assert.That(column.Name, Is.EqualTo(columnName));
    }

    [Test]
    public static void Type_PropertyGet_EqualsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";
        var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

        Assert.That(column.Type, Is.EqualTo(columnType));
    }

    [Test]
    public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";
        var column = new OracleDatabaseComputedColumn(columnName, columnType, false, definition);

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";
        var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

        Assert.That(column.IsNullable, Is.True);
    }

    [Test]
    public static void IsComputed_PropertyGet_ReturnsTrue()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";
        var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

        Assert.That(column.IsComputed, Is.True);
    }

    [Test]
    public static void Definition_PropertyGet_EqualsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";
        var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

        Assert.That(column.Definition.UnwrapSome(), Is.EqualTo(definition));
    }

    [Test]
    public static void AutoIncrement_PropertyGet_ReturnsNone()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";
        var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [TestCase("test_computed_column_1", "Computed Column: test_computed_column_1")]
    [TestCase("test_computed_column_2", "Computed Column: test_computed_column_2")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var columnName = Identifier.CreateQualifiedIdentifier(name);
        var columnType = Mock.Of<IDbType>();
        const string definition = "test";

        var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
