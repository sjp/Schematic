using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class ColumnDataTypeTests
{
    private static ColumnDataType CreateDataType(
        Option<IDbType> elementType = default,
        string[] enumValues = null,
        Option<IDbType> baseType = default,
        bool isUnsigned = false,
        DataType dataType = DataType.String,
        Option<int> fractionalSecondsPrecision = default
    )
    {
        return new ColumnDataType(
            "test_type",
            dataType,
            "test_type(10)",
            typeof(string),
            false,
            10,
            Option<INumericPrecision>.None,
            Option<Identifier>.None,
            elementType,
            enumValues ?? [],
            baseType,
            isUnsigned,
            fractionalSecondsPrecision: fractionalSecondsPrecision
        );
    }

    [Test]
    public static void Ctor_GivenNullEnumValues_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ColumnDataType(
                "test_type",
                DataType.Enum,
                "test_type",
                typeof(string),
                false,
                0,
                Option<INumericPrecision>.None,
                Option<Identifier>.None,
                Option<IDbType>.None,
                null!,
                Option<IDbType>.None,
                false),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullEnumValue_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ColumnDataType(
                "test_type",
                DataType.Enum,
                "test_type",
                typeof(string),
                false,
                0,
                Option<INumericPrecision>.None,
                Option<Identifier>.None,
                Option<IDbType>.None,
                ["first", null!],
                Option<IDbType>.None,
                false),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidDataType_ThrowsArgumentException()
    {
        const DataType dataType = (DataType)555;

        Assert.That(() => CreateDataType(dataType: dataType), Throws.ArgumentException);
    }

    // the members added to describe collection, enumerated and unsigned types are the ones under
    // test; a type constructed without them describes none of them
    [Test]
    public static void Ctor_GivenNoTypeDetail_DescribesNoneOfIt()
    {
        var dataType = CreateDataType();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataType.ElementType, Is.EqualTo(Option<IDbType>.None));
            Assert.That(dataType.EnumValues, Is.Empty);
            Assert.That(dataType.BaseType, Is.EqualTo(Option<IDbType>.None));
            Assert.That(dataType.IsUnsigned, Is.False);
        }
    }

    [Test]
    public static void ElementType_GivenElementType_ReturnsElementType()
    {
        var elementType = CreateDataType(dataType: DataType.Integer);
        var dataType = CreateDataType(elementType: Option<IDbType>.Some(elementType), dataType: DataType.Array);

        Assert.That(dataType.ElementType.UnwrapSome(), Is.SameAs(elementType));
    }

    [Test]
    public static void EnumValues_GivenValues_ReturnsValuesInOrder()
    {
        var dataType = CreateDataType(enumValues: ["small", "medium", "large"], dataType: DataType.Enum);

        Assert.That(dataType.EnumValues, Is.EqualTo(new[] { "small", "medium", "large" }));
    }

    [Test]
    public static void BaseType_GivenBaseType_ReturnsBaseType()
    {
        var baseType = CreateDataType();
        var dataType = CreateDataType(baseType: Option<IDbType>.Some(baseType));

        Assert.That(dataType.BaseType.UnwrapSome(), Is.SameAs(baseType));
    }

    [Test]
    public static void IsUnsigned_GivenUnsignedType_ReturnsTrue()
    {
        var dataType = CreateDataType(isUnsigned: true, dataType: DataType.Integer);

        Assert.That(dataType.IsUnsigned, Is.True);
    }

    [Test]
    public static void ClrTypeName_GivenNoClrTypeName_NamesClrType()
    {
        var dataType = CreateDataType();

        Assert.That(dataType.ClrTypeName, Is.EqualTo("System.String"));
    }

    // a name is given when it is known more precisely than the type standing in for it, e.g. by a
    // document naming a type that this process cannot resolve
    [Test]
    public static void ClrTypeName_GivenClrTypeName_ReturnsGivenName()
    {
        var dataType = new ColumnDataType(
            "test_type",
            DataType.String,
            "test_type(10)",
            typeof(object),
            false,
            10,
            Option<INumericPrecision>.None,
            Option<Identifier>.None,
            Option<IDbType>.None,
            [],
            Option<IDbType>.None,
            false,
            "Some.Unresolvable.Type"
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataType.ClrTypeName, Is.EqualTo("Some.Unresolvable.Type"));
            Assert.That(dataType.ClrType, Is.EqualTo(typeof(object)));
        }
    }

    [Test]
    public static void Ctor_GivenWhiteSpaceClrTypeName_ThrowsArgumentException()
    {
        Assert.That(
            () => new ColumnDataType(
                "test_type",
                DataType.String,
                "test_type(10)",
                typeof(string),
                false,
                10,
                Option<INumericPrecision>.None,
                Option<Identifier>.None,
                Option<IDbType>.None,
                [],
                Option<IDbType>.None,
                false,
                "   "),
            Throws.ArgumentException);
    }

    // the shorter constructor is the one every existing caller uses, so it must keep describing a
    // type that has none of the additional detail
    [Test]
    public static void Ctor_GivenNoTypeDetailArguments_DescribesNoneOfIt()
    {
        var dataType = new ColumnDataType(
            "test_type",
            DataType.String,
            "test_type(10)",
            typeof(string),
            false,
            10,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataType.ElementType, Is.EqualTo(Option<IDbType>.None));
            Assert.That(dataType.EnumValues, Is.Empty);
            Assert.That(dataType.BaseType, Is.EqualTo(Option<IDbType>.None));
            Assert.That(dataType.IsUnsigned, Is.False);
            Assert.That(dataType.FractionalSecondsPrecision, Is.EqualTo(Option<int>.None));
        }
    }

    [TestCase(0)]
    [TestCase(3)]
    [TestCase(9)]
    public static void Ctor_GivenFractionalSecondsPrecision_DescribesGivenPrecision(int precision)
    {
        var dataType = CreateDataType(dataType: DataType.DateTime, fractionalSecondsPrecision: precision);

        Assert.That(dataType.FractionalSecondsPrecision.UnwrapSome(), Is.EqualTo(precision));
    }

    [Test]
    public static void Ctor_GivenNegativeFractionalSecondsPrecision_ThrowsArgumentException()
    {
        Assert.That(
            () => CreateDataType(dataType: DataType.DateTime, fractionalSecondsPrecision: -1),
            Throws.ArgumentException);
    }
}
