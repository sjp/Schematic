using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseComputedColumnTests
{
    [Test]
    public static void Ctor_GivenNullColumnName_ThrowsArgumentNullException()
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var definition = Option<string>.Some("1");

        Assert.That(() => new DatabaseComputedColumn(null, dbType, isNullable, defaultValue, definition), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDbType_ThrowsArgumentNullException()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var definition = Option<string>.Some("1");

        Assert.That(() => new DatabaseComputedColumn(columnName, null, isNullable, defaultValue, definition), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var definition = Option<string>.Some("1");

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.Name, Is.EqualTo(columnName));
    }

    [Test]
    public static void Type_PropertyGet_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var definition = Option<string>.Some("1");

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.Type, Is.EqualTo(dbType));
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void IsNullable_PropertyGet_EqualsCtorArg(bool isNullable)
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        var defaultValue = Option<string>.Some("test_default_value");
        var definition = Option<string>.Some("1");

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.IsNullable, Is.EqualTo(isNullable));
    }

    [Test]
    public static void DefaultValue_GivenNoneDefaultValue_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.None;
        var definition = Option<string>.Some("1");

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.DefaultValue, OptionIs.None);
    }

    [Test]
    public static void DefaultValue_GivenSomeDefaultValue_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        const string defaultExpression = "test_default_value";
        var defaultValue = Option<string>.Some(defaultExpression);
        var definition = Option<string>.Some("1");

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo(defaultExpression));
    }

    [Test]
    public static void Definition_GivenNoneDefinition_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.None;
        var definition = Option<string>.None;

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.Definition, OptionIs.None);
    }

    [Test]
    public static void Definition_GivenSomeDefinition_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        const string expression = "1";
        var definition = Option<string>.Some(expression);

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.Definition.UnwrapSome(), Is.EqualTo(expression));
    }

    [Test]
    public static void IsComputed_PropertyGet_EqualsTrue()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        const string defaultExpression = "test_default_value";
        var defaultValue = Option<string>.Some(defaultExpression);
        const string expression = "1";
        var definition = Option<string>.Some(expression);

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.IsComputed, Is.True);
    }

    [Test]
    public static void AutoIncrement_PropertyGet_IsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        const string defaultExpression = "test_default_value";
        var defaultValue = Option<string>.Some(defaultExpression);
        const string expression = "1";
        var definition = Option<string>.Some(expression);

        var column = new DatabaseComputedColumn(columnName, dbType, isNullable, defaultValue, definition);

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [TestCase("test_column_1", "Computed Column: test_column_1")]
    [TestCase("test_column_2", "Computed Column: test_column_2")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string columnName, string expectedResult)
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var definition = Option<string>.Some("1");

        var column = new DatabaseComputedColumn(Identifier.CreateQualifiedIdentifier(columnName), dbType, isNullable, defaultValue, definition);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}