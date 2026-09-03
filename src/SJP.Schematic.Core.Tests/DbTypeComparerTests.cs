using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DbTypeComparerTests
{
    private static ColumnDataType CreateDataType(
        Identifier typeName = null,
        DataType dataType = DataType.String,
        string definition = null,
        int maxLength = 50,
        bool isFixedLength = false,
        Option<INumericPrecision> numericPrecision = default,
        Option<Identifier> collation = default,
        Option<IDbType> elementType = default,
        string[] enumValues = null,
        Option<IDbType> baseType = default,
        bool isUnsigned = false,
        Option<int> fractionalSecondsPrecision = default
    )
    {
        return new ColumnDataType(
            typeName ?? "varchar",
            dataType,
            definition ?? "varchar(50)",
            typeof(string),
            isFixedLength,
            maxLength,
            numericPrecision,
            collation,
            elementType,
            enumValues ?? [],
            baseType,
            isUnsigned,
            fractionalSecondsPrecision: fractionalSecondsPrecision
        );
    }

    [Test]
    public static void Equals_GivenTwoNullTypes_ReturnsTrue()
    {
        Assert.That(DbTypeComparer.Structural.Equals(null, null), Is.True);
    }

    [Test]
    public static void Equals_GivenOneNullType_ReturnsFalse()
    {
        Assert.That(DbTypeComparer.Structural.Equals(CreateDataType(), null), Is.False);
    }

    [Test]
    public static void Equals_GivenSameReference_ReturnsTrue()
    {
        var dataType = CreateDataType();

        Assert.That(DbTypeComparer.Structural.Equals(dataType, dataType), Is.True);
    }

    // the definition is formatted by whichever dialect described the type, so it says nothing about
    // what the type stores
    [Test]
    public static void Equals_GivenTypesDifferingOnlyInDefinition_ReturnsTrue()
    {
        var lower = CreateDataType(typeName: "varchar", definition: "varchar(50)");
        var upper = CreateDataType(typeName: "VARCHAR", definition: "VARCHAR(50)");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DbTypeComparer.Structural.Equals(lower, upper), Is.True);
            Assert.That(DbTypeComparer.Structural.GetHashCode(lower), Is.EqualTo(DbTypeComparer.Structural.GetHashCode(upper)));
        }
    }

    [Test]
    public static void Equals_GivenTypesDifferingOnlyInClrType()
    {
        var stringType = CreateDataType();
        var objectType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(50)",
            typeof(object),
            false,
            50,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        Assert.That(DbTypeComparer.Structural.Equals(stringType, objectType), Is.True);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentNames_ReturnsFalse()
    {
        var varchar = CreateDataType(typeName: "varchar");
        var nvarchar = CreateDataType(typeName: "nvarchar");

        Assert.That(DbTypeComparer.Structural.Equals(varchar, nvarchar), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentSchemas_ReturnsFalse()
    {
        var systemType = CreateDataType(typeName: new Identifier("sys", "varchar"));
        var userType = CreateDataType(typeName: new Identifier("dbo", "varchar"));

        Assert.That(DbTypeComparer.Structural.Equals(systemType, userType), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentLengths_ReturnsFalse()
    {
        var shortType = CreateDataType(maxLength: 50);
        var longType = CreateDataType(maxLength: 100);

        Assert.That(DbTypeComparer.Structural.Equals(shortType, longType), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentFixedLengths_ReturnsFalse()
    {
        var fixedType = CreateDataType(isFixedLength: true);
        var varyingType = CreateDataType(isFixedLength: false);

        Assert.That(DbTypeComparer.Structural.Equals(fixedType, varyingType), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentDataTypes_ReturnsFalse()
    {
        var stringType = CreateDataType(dataType: DataType.String);
        var unicodeType = CreateDataType(dataType: DataType.Unicode);

        Assert.That(DbTypeComparer.Structural.Equals(stringType, unicodeType), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithSamePrecision_ReturnsTrue()
    {
        var first = CreateDataType(typeName: "numeric", numericPrecision: Option<INumericPrecision>.Some(new NumericPrecision(10, 2)));
        var second = CreateDataType(typeName: "numeric", numericPrecision: Option<INumericPrecision>.Some(new NumericPrecision(10, 2)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.True);
            Assert.That(DbTypeComparer.Structural.GetHashCode(first), Is.EqualTo(DbTypeComparer.Structural.GetHashCode(second)));
        }
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentPrecisions_ReturnsFalse()
    {
        var first = CreateDataType(typeName: "numeric", numericPrecision: Option<INumericPrecision>.Some(new NumericPrecision(10, 2)));
        var second = CreateDataType(typeName: "numeric", numericPrecision: Option<INumericPrecision>.Some(new NumericPrecision(10, 4)));

        Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.False);
    }

    [Test]
    public static void Equals_GivenTypeWithPrecisionAndTypeWithout_ReturnsFalse()
    {
        var withPrecision = CreateDataType(typeName: "numeric", numericPrecision: Option<INumericPrecision>.Some(new NumericPrecision(10, 2)));
        var withoutPrecision = CreateDataType(typeName: "numeric");

        Assert.That(DbTypeComparer.Structural.Equals(withPrecision, withoutPrecision), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithSameFractionalSecondsPrecision_ReturnsTrue()
    {
        var first = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime, fractionalSecondsPrecision: 3);
        var second = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime, fractionalSecondsPrecision: 3);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.True);
            Assert.That(DbTypeComparer.Structural.GetHashCode(first), Is.EqualTo(DbTypeComparer.Structural.GetHashCode(second)));
        }
    }

    // a name that has had its arguments removed no longer distinguishes one precision from another,
    // so the precision has to do it on its own
    [Test]
    public static void Equals_GivenTypesWithDifferentFractionalSecondsPrecisions_ReturnsFalse()
    {
        var milliseconds = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime, fractionalSecondsPrecision: 3);
        var nanoseconds = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime, fractionalSecondsPrecision: 9);

        Assert.That(DbTypeComparer.Structural.Equals(milliseconds, nanoseconds), Is.False);
    }

    [Test]
    public static void Equals_GivenTypeWithFractionalSecondsPrecisionAndTypeWithout_ReturnsFalse()
    {
        var withPrecision = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime, fractionalSecondsPrecision: 0);
        var withoutPrecision = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime);

        Assert.That(DbTypeComparer.Structural.Equals(withPrecision, withoutPrecision), Is.False);
    }

    // the name-only comparer is deliberately blind to everything the name does not say
    [Test]
    public static void Equals_GivenNameOnlyComparerAndTypesWithDifferentFractionalSecondsPrecisions_ReturnsTrue()
    {
        var milliseconds = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime, fractionalSecondsPrecision: 3);
        var nanoseconds = CreateDataType(typeName: "timestamp", dataType: DataType.DateTime, fractionalSecondsPrecision: 9);

        Assert.That(DbTypeComparer.NameOnly.Equals(milliseconds, nanoseconds), Is.True);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentSignedness_ReturnsFalse()
    {
        var signed = CreateDataType(typeName: "int", dataType: DataType.Integer, isUnsigned: false);
        var unsigned = CreateDataType(typeName: "int", dataType: DataType.Integer, isUnsigned: true);

        Assert.That(DbTypeComparer.Structural.Equals(signed, unsigned), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentEnumValues_ReturnsFalse()
    {
        var first = CreateDataType(dataType: DataType.Enum, enumValues: ["small", "large"]);
        var second = CreateDataType(dataType: DataType.Enum, enumValues: ["small", "medium", "large"]);

        Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentElementTypes_ReturnsFalse()
    {
        var intElement = CreateDataType(typeName: "int", dataType: DataType.Integer);
        var bigintElement = CreateDataType(typeName: "bigint", dataType: DataType.BigInteger);
        var first = CreateDataType(typeName: "int[]", dataType: DataType.Array, elementType: Option<IDbType>.Some(intElement));
        var second = CreateDataType(typeName: "int[]", dataType: DataType.Array, elementType: Option<IDbType>.Some(bigintElement));

        Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.False);
    }

    [Test]
    public static void Equals_GivenTypesWithEquivalentElementTypes_ReturnsTrue()
    {
        var first = CreateDataType(typeName: "int[]", dataType: DataType.Array, elementType: Option<IDbType>.Some(CreateDataType(typeName: "int", dataType: DataType.Integer)));
        var second = CreateDataType(typeName: "int[]", dataType: DataType.Array, elementType: Option<IDbType>.Some(CreateDataType(typeName: "INT", dataType: DataType.Integer)));

        Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.True);
    }

    [Test]
    public static void Equals_GivenTypesWithDifferentBaseTypes_ReturnsFalse()
    {
        var first = CreateDataType(typeName: "email", baseType: Option<IDbType>.Some(CreateDataType(maxLength: 50)));
        var second = CreateDataType(typeName: "email", baseType: Option<IDbType>.Some(CreateDataType(maxLength: 100)));

        Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.False);
    }

    [Test]
    public static void Equals_GivenStructuralComparerAndTypesDifferingOnlyInCollation_ReturnsFalse()
    {
        var first = CreateDataType(collation: Option<Identifier>.Some("Latin1_General_CI_AS"));
        var second = CreateDataType(collation: Option<Identifier>.Some("SQL_Latin1_General_CP1_CS_AS"));

        Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.False);
    }

    [Test]
    public static void Equals_GivenStructuralComparerAndCollationsDifferingOnlyInCase_ReturnsTrue()
    {
        var first = CreateDataType(collation: Option<Identifier>.Some("Latin1_General_CI_AS"));
        var second = CreateDataType(collation: Option<Identifier>.Some("latin1_general_ci_as"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DbTypeComparer.Structural.Equals(first, second), Is.True);
            Assert.That(DbTypeComparer.Structural.GetHashCode(first), Is.EqualTo(DbTypeComparer.Structural.GetHashCode(second)));
        }
    }

    [Test]
    public static void Equals_GivenStructuralComparerAndOneCollatedType_ReturnsFalse()
    {
        var collated = CreateDataType(collation: Option<Identifier>.Some("Latin1_General_CI_AS"));
        var uncollated = CreateDataType();

        Assert.That(DbTypeComparer.Structural.Equals(collated, uncollated), Is.False);
    }

    [Test]
    public static void Equals_GivenCollationIgnoringComparerAndTypesDifferingOnlyInCollation_ReturnsTrue()
    {
        var first = CreateDataType(collation: Option<Identifier>.Some("Latin1_General_CI_AS"));
        var second = CreateDataType(collation: Option<Identifier>.Some("SQL_Latin1_General_CP1_CS_AS"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DbTypeComparer.StructuralIgnoringCollation.Equals(first, second), Is.True);
            Assert.That(DbTypeComparer.StructuralIgnoringCollation.GetHashCode(first), Is.EqualTo(DbTypeComparer.StructuralIgnoringCollation.GetHashCode(second)));
        }
    }

    [Test]
    public static void Equals_GivenCollationIgnoringComparerAndTypesWithDifferentLengths_ReturnsFalse()
    {
        var first = CreateDataType(maxLength: 50);
        var second = CreateDataType(maxLength: 100);

        Assert.That(DbTypeComparer.StructuralIgnoringCollation.Equals(first, second), Is.False);
    }

    [Test]
    public static void Equals_GivenNameOnlyComparerAndTypesWithSameName_ReturnsTrue()
    {
        var first = CreateDataType(typeName: "varchar", maxLength: 50, collation: Option<Identifier>.Some("Latin1_General_CI_AS"));
        var second = CreateDataType(typeName: "VARCHAR", maxLength: 100);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DbTypeComparer.NameOnly.Equals(first, second), Is.True);
            Assert.That(DbTypeComparer.NameOnly.GetHashCode(first), Is.EqualTo(DbTypeComparer.NameOnly.GetHashCode(second)));
        }
    }

    [Test]
    public static void Equals_GivenNameOnlyComparerAndTypesWithDifferentNames_ReturnsFalse()
    {
        var first = CreateDataType(typeName: "varchar");
        var second = CreateDataType(typeName: "nvarchar");

        Assert.That(DbTypeComparer.NameOnly.Equals(first, second), Is.False);
    }

    [Test]
    public static void GetHashCode_GivenNullType_ReturnsZero()
    {
        Assert.That(DbTypeComparer.Structural.GetHashCode(null), Is.Zero);
    }

    [Test]
    public static void Comparer_WhenUsedAsDictionaryKeyComparer_TreatsEquivalentTypesAsOneKey()
    {
        var types = new Dictionary<IDbType, int>(DbTypeComparer.Structural)
        {
            [CreateDataType(typeName: "varchar", definition: "varchar(50)")] = 1,
        };

        types[CreateDataType(typeName: "VARCHAR", definition: "VARCHAR(50)")] = 2;

        Assert.That(types, Has.Count.EqualTo(1));
    }
}
