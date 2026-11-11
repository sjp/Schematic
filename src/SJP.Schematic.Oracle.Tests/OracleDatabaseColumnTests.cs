using System;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

internal static class OracleDatabaseColumnTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        var columnType = Mock.Of<IDbType>();
        Assert.That(
            () => new OracleDatabaseColumn(null, columnType, true, Option<DatabaseDefaultValue>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    [Test]
    public static void Ctor_GivenNullType_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new OracleDatabaseColumn("test_column", null, true, Option<DatabaseDefaultValue>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("type")
        );
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None);

        Assert.That(column.Name, Is.EqualTo(columnName));
    }

    [Test]
    public static void Type_PropertyGet_EqualsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None);

        Assert.That(column.Type, Is.EqualTo(columnType));
    }

    [Test]
    public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var column = new OracleDatabaseColumn(columnName, columnType, false, Option<DatabaseDefaultValue>.None);

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None);

        Assert.That(column.IsNullable, Is.True);
    }

    [Test]
    public static void DefaultValue_PropertyGet_ReturnsCtorArg()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        const string defaultValue = "1";
        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Unknown(defaultValue)));

        Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo(defaultValue));
    }

    [Test]
    public static void Ctor_GivenDefaultValueSource_ThrowsArgumentNullException()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        Assert.That(
            () => new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None, default(ColumnValueSource)),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("valueSource")
        );
    }

    [Test]
    public static void ValueSource_GivenNoValueSourceCtorArgument_ReturnsStored()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None);

        Assert.That(column.ValueSource, Is.EqualTo((ColumnValueSource)new ColumnValueSource.Stored()));
    }

    [Test]
    public static void ValueSource_GivenIdentityCtorArgument_ReturnsCtorArgument()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var autoIncrement = new AutoIncrement(1, 1, IdentityGeneration.Always, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.Some("ISEQ$$_12345"));
        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Generated(autoIncrement));

        Assert.That(column.ValueSource.UnwrapIdentity(), Is.EqualTo(autoIncrement));
    }

    [Test]
    public static void ValueSource_GivenComputedCtorArgument_ReturnsCtorArgument()
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();
        var valueSource = new ColumnValueSource.Computed(Option<string>.Some("1 + 1"), ComputedColumnStorage.Virtual);

        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None, valueSource);

        Assert.That(column.ValueSource.UnwrapComputed(), Is.SameAs(valueSource));
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void IsHidden_PropertyGet_EqualsCtorArg(bool isHidden)
    {
        Identifier columnName = "test_column";
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored(), isHidden);

        Assert.That(column.IsHidden, Is.EqualTo(isHidden));
    }

    [TestCase("test_column_1", "Computed Column: test_column_1")]
    [TestCase("test_column_2", "Computed Column: test_column_2")]
    public static void ToString_WhenGivenComputedColumn_ReturnsExpectedValues(string name, string expectedResult)
    {
        var columnName = Identifier.CreateQualifiedIdentifier(name);
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Computed(Option<string>.Some("1"), ComputedColumnStorage.Virtual));
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("test_column_1", "Column: test_column_1")]
    [TestCase("test_column_2", "Column: test_column_2")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var columnName = Identifier.CreateQualifiedIdentifier(name);
        var columnType = Mock.Of<IDbType>();

        var column = new OracleDatabaseColumn(columnName, columnType, true, Option<DatabaseDefaultValue>.None);
        var result = column.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}