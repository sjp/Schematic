using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseColumnTests
{
    [Test]
    public static void Ctor_GivenNullColumnName_ThrowsArgumentNullException()
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        Assert.That(() => new DatabaseColumn(null, dbType, isNullable, defaultValue, autoIncrement), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDbType_ThrowsArgumentNullException()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        Assert.That(() => new DatabaseColumn(columnName, null, isNullable, defaultValue, autoIncrement), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        Assert.That(column.Name, Is.EqualTo(columnName));
    }

    [Test]
    public static void Name_GivenQualifiedCtorArg_PropertyGetReturnsLocalNameOnly()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_schema", "test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        Assert.That(column.Name, Is.EqualTo(Identifier.CreateQualifiedIdentifier("test_column_name")));
    }

    [Test]
    public static void Type_PropertyGet_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        Assert.That(column.Type, Is.EqualTo(dbType));
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void IsNullable_PropertyGet_EqualsCtorArg(bool isNullable)
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        Assert.That(column.IsNullable, Is.EqualTo(isNullable));
    }

    [Test]
    public static void DefaultValue_GivenNoneDefaultValue_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.None;
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

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
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo(defaultExpression));
    }

    [Test]
    public static void AutoIncrement_GivenNoneAutoIncrement_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.None;
        var autoIncrement = Option<IAutoIncrement>.None;

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        Assert.That(column.AutoIncrement, OptionIs.None);
    }

    [Test]
    public static void AutoIncrement_GivenSomeAutoIncrement_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");

        const decimal initialValue = 123m;
        const decimal increment = 456m;
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(initialValue, increment));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(increment));
            Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(initialValue));
        }
    }

    [Test]
    public static void IsComputed_PropertyGet_EqualsFalse()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        const string defaultExpression = "test_default_value";
        var defaultValue = Option<string>.Some(defaultExpression);
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        Assert.That(column.IsComputed, Is.False);
    }

    [Test]
    public static void Ctor_GivenInvalidComputedStorage_ThrowsArgumentException()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.None;
        var autoIncrement = Option<IAutoIncrement>.None;
        var definition = Option<string>.Some("1");
        const ComputedColumnStorage storage = (ComputedColumnStorage)55;

        Assert.That(() => new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement, true, definition, storage), Throws.ArgumentException);
    }

    [Test]
    public static void IsComputed_GivenComputedCtorArg_EqualsTrue()
    {
        var column = CreateComputedColumn(Option<string>.Some("1"), ComputedColumnStorage.Stored);

        Assert.That(column.IsComputed, Is.True);
    }

    [Test]
    public static void ComputedDefinition_GivenSomeDefinition_EqualsCtorArg()
    {
        const string expression = "1";

        var column = CreateComputedColumn(Option<string>.Some(expression), ComputedColumnStorage.Stored);

        Assert.That(column.ComputedDefinition.UnwrapSome(), Is.EqualTo(expression));
    }

    [Test]
    public static void ComputedDefinition_GivenNoneDefinition_EqualsNone()
    {
        var column = CreateComputedColumn(Option<string>.None, ComputedColumnStorage.Virtual);

        Assert.That(column.ComputedDefinition, OptionIs.None);
    }

    [TestCase(ComputedColumnStorage.Unknown)]
    [TestCase(ComputedColumnStorage.Virtual)]
    [TestCase(ComputedColumnStorage.Stored)]
    public static void ComputedStorage_PropertyGet_EqualsCtorArg(ComputedColumnStorage storage)
    {
        var column = CreateComputedColumn(Option<string>.Some("1"), storage);

        Assert.That(column.ComputedStorage, Is.EqualTo(storage));
    }

    [Test]
    public static void ComputedDefinition_WhenColumnIsNotComputed_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.None;
        var autoIncrement = Option<IAutoIncrement>.None;

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement, false, Option<string>.Some("1"), ComputedColumnStorage.Stored);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.ComputedDefinition, OptionIs.None);
            Assert.That(column.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Unknown));
        }
    }

    [Test]
    public static void ComputedDefinition_WhenNotProvidedToCtor_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.None;

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.ComputedDefinition, OptionIs.None);
            Assert.That(column.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Unknown));
        }
    }

    [TestCase("test_column_1", "Computed Column: test_column_1")]
    [TestCase("test_column_2", "Computed Column: test_column_2")]
    public static void ToString_WhenGivenComputedColumn_ReturnsExpectedValues(string columnName, string expectedResult)
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;

        var column = new DatabaseColumn(Identifier.CreateQualifiedIdentifier(columnName), dbType, isNullable, Option<string>.None, Option<IAutoIncrement>.None, true, Option<string>.Some("1"), ComputedColumnStorage.Virtual);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private static DatabaseColumn CreateComputedColumn(Option<string> computedDefinition, ComputedColumnStorage storage)
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();

        return new DatabaseColumn(columnName, dbType, false, Option<string>.None, Option<IAutoIncrement>.None, true, computedDefinition, storage);
    }

    [TestCase("test_column_1", "Column: test_column_1")]
    [TestCase("test_column_2", "Column: test_column_2")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string columnName, string expectedResult)
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<string>.Some("test_default_value");
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(Identifier.CreateQualifiedIdentifier(columnName), dbType, isNullable, defaultValue, autoIncrement);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}