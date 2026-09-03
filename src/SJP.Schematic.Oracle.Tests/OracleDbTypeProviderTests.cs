using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDbTypeProviderTests
{
    private static OracleDbTypeProvider Provider => new();

    [Test]
    public static void Ctor_GivenNoArguments_CreatesWithoutError()
    {
        Assert.That(() => new OracleDbTypeProvider(), Throws.Nothing);
    }

    [Test]
    public static void CreateColumnType_GivenNullTypeMetadata_ThrowsArgumentNullException()
    {
        Assert.That(() => Provider.CreateColumnType(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetComparableColumnType_GivenNullOtherType_ThrowsArgumentNullException()
    {
        Assert.That(() => Provider.GetComparableColumnType(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void CreateColumnType_GivenUnknownDataTypeWithoutTypeName_ThrowsArgumentOutOfRangeException()
    {
        var metadata = new ColumnTypeMetadata { DataType = DataType.Unknown };

        Assert.That(() => Provider.CreateColumnType(metadata), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    // Forward mapping: a generic data type resolves to its default Oracle type name.
    [TestCase(DataType.BigInteger, "NUMBER")]
    [TestCase(DataType.Boolean, "CHAR")]
    [TestCase(DataType.Date, "DATE")]
    [TestCase(DataType.DateTime, "TIMESTAMP WITH LOCAL TIME ZONE")]
    [TestCase(DataType.Float, "FLOAT")]
    [TestCase(DataType.Geometry, "SDO_GEOMETRY")]
    [TestCase(DataType.Integer, "NUMBER")]
    [TestCase(DataType.Interval, "INTERVAL DAY TO SECOND")]
    [TestCase(DataType.DateTimeOffset, "TIMESTAMP WITH TIME ZONE")]
    [TestCase(DataType.TinyInteger, "NUMBER")]
    [TestCase(DataType.Money, "NUMBER")]
    [TestCase(DataType.Variant, "ANYDATA")]
    [TestCase(DataType.Vector, "VECTOR")]
    [TestCase(DataType.Json, "JSON")]
    [TestCase(DataType.Numeric, "NUMBER")]
    [TestCase(DataType.SmallInteger, "NUMBER")]
    [TestCase(DataType.Time, "INTERVAL DAY TO SECOND")]
    [TestCase(DataType.UniqueIdentifier, "RAW")]
    [TestCase(DataType.Xml, "XMLTYPE")]
    public static void CreateColumnType_GivenDataTypeWithoutTypeName_ReturnsExpectedTypeName(DataType dataType, string expectedTypeName)
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = dataType });

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    // Forward mapping for variable-length character/binary types.
    [TestCase(DataType.Binary, "RAW")]
    [TestCase(DataType.LargeBinary, "BLOB")]
    [TestCase(DataType.String, "VARCHAR2")]
    [TestCase(DataType.Text, "CLOB")]
    [TestCase(DataType.Unicode, "NVARCHAR2")]
    [TestCase(DataType.UnicodeText, "NCLOB")]
    public static void CreateColumnType_GivenVariableLengthDataTypeWithoutTypeName_ReturnsExpectedTypeName(DataType dataType, string expectedTypeName)
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = dataType, IsFixedLength = false });

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    // Forward mapping for fixed-length character/binary types. A large object has no fixed-length
    // form, so it keeps the same name whether or not the column is declared as a fixed length.
    [TestCase(DataType.Binary, "RAW")]
    [TestCase(DataType.LargeBinary, "BLOB")]
    [TestCase(DataType.String, "CHAR")]
    [TestCase(DataType.Text, "CLOB")]
    [TestCase(DataType.Unicode, "NCHAR")]
    [TestCase(DataType.UnicodeText, "NCLOB")]
    public static void CreateColumnType_GivenFixedLengthDataTypeWithoutTypeName_ReturnsExpectedTypeName(DataType dataType, string expectedTypeName)
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = dataType, IsFixedLength = true });

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    // Reverse mapping: a native Oracle type name resolves to its generic data type.
    [TestCase("BFILE", DataType.LargeBinary)]
    [TestCase("BLOB", DataType.LargeBinary)]
    [TestCase("CHAR", DataType.String)]
    [TestCase("ANYDATA", DataType.Variant)]
    [TestCase("CLOB", DataType.Text)]
    [TestCase("NCLOB", DataType.UnicodeText)]
    [TestCase("DATE", DataType.Date)]
    [TestCase("FLOAT", DataType.Float)]
    [TestCase("INTEGER", DataType.BigInteger)]
    [TestCase("INTERVAL DAY TO SECOND", DataType.Interval)]
    [TestCase("INTERVAL YEAR TO MONTH", DataType.Interval)]
    [TestCase("JSON", DataType.Json)]
    [TestCase("NCHAR", DataType.Unicode)]
    [TestCase("NVARCHAR2", DataType.Unicode)]
    [TestCase("RAW", DataType.Binary)]
    [TestCase("LONG RAW", DataType.LargeBinary)]
    [TestCase("ROWID", DataType.Other)]
    [TestCase("UROWID", DataType.Other)]
    [TestCase("SDO_GEOMETRY", DataType.Geometry)]
    [TestCase("TIMESTAMP", DataType.DateTime)]
    [TestCase("TIMESTAMP WITH TIME ZONE", DataType.DateTimeOffset)]
    [TestCase("TIMESTAMP WITH LOCAL TIME ZONE", DataType.DateTime)]
    // Oracle reports the fractional seconds precision as part of the type name
    [TestCase("TIMESTAMP(6)", DataType.DateTime)]
    [TestCase("TIMESTAMP(9) WITH TIME ZONE", DataType.DateTimeOffset)]
    [TestCase("TIMESTAMP(3) WITH LOCAL TIME ZONE", DataType.DateTime)]
    [TestCase("INTERVAL DAY(3) TO SECOND(6)", DataType.Interval)]
    [TestCase("INTERVAL YEAR(4) TO MONTH", DataType.Interval)]
    [TestCase("VECTOR", DataType.Vector)]
    [TestCase("VARCHAR2", DataType.String)]
    [TestCase("XMLTYPE", DataType.Xml)]
    public static void CreateColumnType_GivenTypeNameWithUnknownDataType_ResolvesExpectedDataType(string typeName, DataType expectedDataType)
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { TypeName = typeName, DataType = DataType.Unknown });

        Assert.That(columnType.DataType, Is.EqualTo(expectedDataType));
    }

    // Reverse mapping: a native Oracle type name resolves to its CLR type.
    [TestCase("BLOB", typeof(byte[]))]
    [TestCase("CHAR", typeof(string))]
    [TestCase("DATE", typeof(DateTime))]
    [TestCase("JSON", typeof(string))]
    [TestCase("RAW", typeof(byte[]))]
    [TestCase("SDO_GEOMETRY", typeof(object))]
    [TestCase("XMLTYPE", typeof(string))]
    public static void CreateColumnType_GivenTypeName_ResolvesExpectedClrType(string typeName, Type expectedClrType)
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { TypeName = typeName, DataType = DataType.Unknown });

        Assert.That(columnType.ClrType, Is.EqualTo(expectedClrType));
    }

    [Test]
    public static void CreateColumnType_GivenTypeNameInAnotherSchema_ResolvesOtherDataType()
    {
        // a type outside SYS is user-defined -- an object type, a varray or a nested table -- which
        // the catalog does not distinguish from the column alone
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("APP", "ADDRESS_TYPE"), DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(DataType.Other));
    }

    [Test]
    public static void CreateColumnType_GivenUnqualifiedUnrecognisedTypeName_ResolvesUnknownDataType()
    {
        var metadata = new ColumnTypeMetadata { TypeName = "NOT_A_REAL_TYPE", DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(DataType.Unknown));
    }

    [Test]
    public static void CreateColumnType_GivenJsonDataType_ReturnsJsonColumnType()
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Json });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("JSON"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Json));
            Assert.That(columnType.ClrType, Is.EqualTo(typeof(string)));
        }
    }

    [Test]
    public static void CreateColumnType_GivenXmlDataType_ReturnsXmlTypeColumnType()
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Xml });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("XMLTYPE"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Xml));
            Assert.That(columnType.ClrType, Is.EqualTo(typeof(string)));
        }
    }

    [Test]
    public static void CreateColumnType_GivenGeometryDataType_ReturnsSdoGeometryColumnType()
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Geometry });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("SDO_GEOMETRY"));
            Assert.That(columnType.TypeName.Schema, Is.EqualTo("MDSYS"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Geometry));
            Assert.That(columnType.ClrType, Is.EqualTo(typeof(object)));
        }
    }

    [Test]
    public static void CreateColumnType_GivenUniqueIdentifierDataType_ReturnsRawColumnType()
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.UniqueIdentifier });

        // Oracle has no dedicated GUID type; UUIDs are conventionally stored as RAW(16).
        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("RAW"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.UniqueIdentifier));
        }
    }

    [Test]
    public static void GetComparableColumnType_GivenColumnType_ReturnsTypeWithMatchingDataType()
    {
        var sourceType = Provider.CreateColumnType(new ColumnTypeMetadata { TypeName = "XMLTYPE", DataType = DataType.Unknown });
        var comparableType = Provider.GetComparableColumnType(sourceType);

        Assert.That(comparableType.DataType, Is.EqualTo(sourceType.DataType));
    }
}
