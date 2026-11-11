using System;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

internal static class DatabaseColumnTests
{
    [Test]
    public static void Ctor_GivenNullColumnName_ThrowsArgumentNullException()
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        Assert.That(
            () => new DatabaseColumn(null, dbType, isNullable, defaultValue, valueSource),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    [Test]
    public static void Ctor_GivenNullDbType_ThrowsArgumentNullException()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        Assert.That(
            () => new DatabaseColumn(columnName, null, isNullable, defaultValue, valueSource),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("type")
        );
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        Assert.That(column.Name, Is.EqualTo(columnName));
    }

    [Test]
    public static void Name_GivenQualifiedCtorArg_PropertyGetReturnsLocalNameOnly()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_schema", "test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        Assert.That(column.Name, Is.EqualTo(Identifier.CreateQualifiedIdentifier("test_column_name")));
    }

    [Test]
    public static void Type_PropertyGet_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        Assert.That(column.Type, Is.EqualTo(dbType));
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void IsNullable_PropertyGet_EqualsCtorArg(bool isNullable)
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        Assert.That(column.IsNullable, Is.EqualTo(isNullable));
    }

    [Test]
    public static void DefaultValue_GivenNoneDefaultValue_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.None;
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        Assert.That(column.DefaultValue, OptionIs.None);
    }

    [Test]
    public static void DefaultValue_GivenSomeDefaultValue_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        const string defaultExpression = "test_default_value";
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown(defaultExpression));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo(defaultExpression));
    }

    [Test]
    public static void Default_GivenNoneDefaultValue_EqualsNone()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.None;
        var valueSource = new ColumnValueSource.Stored();

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        Assert.That(column.Default, OptionIs.None);
    }

    [Test]
    public static void Default_GivenSomeDefaultValue_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var columnDefault = new DatabaseDefaultValue.Literal("((0))", Option<Identifier>.Some("df_test"));
        var defaultValue = Option<DatabaseDefaultValue>.Some(columnDefault);
        var valueSource = new ColumnValueSource.Stored();

        var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, valueSource);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.Default.UnwrapSome().Value, Is.SameAs(columnDefault));
            // the string form stays available as a convenience over the default's definition
            Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo("((0))"));
        }
    }

    [Test]
    public static void Ctor_GivenSomeDefaultWithNoValue_ThrowsArgumentNullException()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        var defaultValue = Option<DatabaseDefaultValue>.Some(default);

        Assert.That(
            () => new DatabaseColumn(columnName, dbType, false, defaultValue, new ColumnValueSource.Stored()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("defaultValue")
        );
    }

    [Test]
    public static void Ctor_GivenDefaultValueSource_ThrowsArgumentNullException()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();

        Assert.That(
            () => new DatabaseColumn(columnName, dbType, false, Option<DatabaseDefaultValue>.None, default(ColumnValueSource)),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("valueSource")
        );
    }

    [Test]
    public static void ValueSource_GivenStoredCtorArg_EqualsStored()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();

        var column = new DatabaseColumn(columnName, dbType, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        Assert.That(column.ValueSource.Value, Is.TypeOf<ColumnValueSource.Stored>());
    }

    [Test]
    public static void ValueSource_GivenGeneratedCtorArg_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));

        const decimal initialValue = 123m;
        const decimal increment = 456m;
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(initialValue, increment));

        var column = new DatabaseColumn(columnName, dbType, false, defaultValue, valueSource);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.ValueSource.UnwrapIdentity().Increment, Is.EqualTo(increment));
            Assert.That(column.ValueSource.UnwrapIdentity().InitialValue, Is.EqualTo(initialValue));
        }
    }

    [Test]
    public static void ValueSource_GivenComputedCtorArg_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        var valueSource = new ColumnValueSource.Computed(Option<string>.Some("1"), ComputedColumnStorage.Stored);

        var column = new DatabaseColumn(columnName, dbType, false, Option<DatabaseDefaultValue>.None, valueSource);

        Assert.That(column.ValueSource.UnwrapComputed(), Is.SameAs(valueSource));
    }

    [Test]
    public static void Default_GivenStoredColumnWithDefault_EqualsCtorArg()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();
        var defaultValue = new DatabaseDefaultValue.Literal("1");

        var column = new DatabaseColumn(columnName, dbType, false, Option<DatabaseDefaultValue>.Some(defaultValue), new ColumnValueSource.Stored());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.ValueSource.Value, Is.TypeOf<ColumnValueSource.Stored>());
            Assert.That(column.Default.UnwrapSome().Value, Is.SameAs(defaultValue));
        }
    }

    [TestCase("test_column_1", "Computed Column: test_column_1")]
    [TestCase("test_column_2", "Computed Column: test_column_2")]
    public static void ToString_WhenGivenComputedColumn_ReturnsExpectedValues(string columnName, string expectedResult)
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var valueSource = new ColumnValueSource.Computed(Option<string>.Some("1"), ComputedColumnStorage.Virtual);

        var column = new DatabaseColumn(Identifier.CreateQualifiedIdentifier(columnName), dbType, isNullable, Option<DatabaseDefaultValue>.None, valueSource);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public static void IsHidden_WhenNotGiven_EqualsFalse()
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();

        var column = new DatabaseColumn(columnName, dbType, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        Assert.That(column.IsHidden, Is.False);
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void IsHidden_PropertyGet_EqualsCtorArg(bool isHidden)
    {
        var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
        var dbType = Mock.Of<IDbType>();

        var column = new DatabaseColumn(
            columnName,
            dbType,
            false,
            Option<DatabaseDefaultValue>.None,
            new ColumnValueSource.Stored(),
            isHidden);

        Assert.That(column.IsHidden, Is.EqualTo(isHidden));
    }

    [TestCase("test_column_1", "Column: test_column_1")]
    [TestCase("test_column_2", "Column: test_column_2")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string columnName, string expectedResult)
    {
        var dbType = Mock.Of<IDbType>();
        const bool isNullable = false;
        var defaultValue = Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown("test_default_value"));
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(123, 456));

        var column = new DatabaseColumn(Identifier.CreateQualifiedIdentifier(columnName), dbType, isNullable, defaultValue, valueSource);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}