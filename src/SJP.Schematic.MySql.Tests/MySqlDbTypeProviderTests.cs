using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlDbTypeProviderTests
{
    private static MySqlDbTypeProvider Provider => new();

    [Test]
    public static void Ctor_GivenNoArguments_CreatesWithoutError()
    {
        Assert.That(() => new MySqlDbTypeProvider(), Throws.Nothing);
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

    // Forward mapping: a generic data type resolves to its default MySQL type name.
    [TestCase(DataType.BigInteger, "bigint")]
    [TestCase(DataType.LargeBinary, "longblob")]
    [TestCase(DataType.Boolean, "bit")]
    [TestCase(DataType.Date, "date")]
    [TestCase(DataType.DateTime, "datetime")]
    [TestCase(DataType.Float, "double")]
    [TestCase(DataType.Geometry, "geometry")]
    [TestCase(DataType.Integer, "int")]
    [TestCase(DataType.Interval, "timestamp")]
    [TestCase(DataType.Json, "json")]
    [TestCase(DataType.Numeric, "numeric")]
    [TestCase(DataType.SmallInteger, "smallint")]
    [TestCase(DataType.Text, "longtext")]
    [TestCase(DataType.Time, "time")]
    [TestCase(DataType.UnicodeText, "longtext")]
    [TestCase(DataType.Xml, "longtext")]
    public static void CreateColumnType_GivenDataTypeWithoutTypeName_ReturnsExpectedTypeName(DataType dataType, string expectedTypeName)
    {
        var metadata = new ColumnTypeMetadata { DataType = dataType };
        var columnType = Provider.CreateColumnType(metadata);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
            Assert.That(columnType.DataType, Is.EqualTo(dataType));
        }
    }

    // Forward mapping: variable-length string/binary types resolve to their variable-length name.
    [TestCase(DataType.Binary, "varbinary")]
    [TestCase(DataType.String, "varchar")]
    [TestCase(DataType.Unicode, "varchar")]
    public static void CreateColumnType_GivenVariableLengthDataType_ReturnsVariableLengthTypeName(DataType dataType, string expectedTypeName)
    {
        var metadata = new ColumnTypeMetadata { DataType = dataType, IsFixedLength = false };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    // Forward mapping: fixed-length string/binary types resolve to their fixed-length name.
    [TestCase(DataType.Binary, "binary")]
    [TestCase(DataType.String, "char")]
    [TestCase(DataType.Unicode, "char")]
    public static void CreateColumnType_GivenFixedLengthDataType_ReturnsFixedLengthTypeName(DataType dataType, string expectedTypeName)
    {
        var metadata = new ColumnTypeMetadata { DataType = dataType, IsFixedLength = true };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    // Reverse mapping: a MySQL type name resolves to the matching generic data type.
    [TestCase("bit", DataType.Boolean)]
    [TestCase("tinyint", DataType.Integer)]
    [TestCase("smallint", DataType.Integer)]
    [TestCase("mediumint", DataType.Integer)]
    [TestCase("int", DataType.Integer)]
    [TestCase("bigint", DataType.Integer)]
    [TestCase("year", DataType.Integer)]
    [TestCase("numeric", DataType.Numeric)]
    [TestCase("decimal", DataType.Numeric)]
    [TestCase("float", DataType.Float)]
    [TestCase("double", DataType.Float)]
    [TestCase("date", DataType.Date)]
    [TestCase("datetime", DataType.DateTime)]
    [TestCase("timestamp", DataType.DateTime)]
    [TestCase("time", DataType.Time)]
    [TestCase("char", DataType.Unicode)]
    [TestCase("varchar", DataType.Unicode)]
    [TestCase("binary", DataType.Binary)]
    [TestCase("varbinary", DataType.Binary)]
    [TestCase("blob", DataType.LargeBinary)]
    [TestCase("longtext", DataType.UnicodeText)]
    [TestCase("json", DataType.Json)]
    [TestCase("geometry", DataType.Geometry)]
    [TestCase("point", DataType.Geometry)]
    [TestCase("linestring", DataType.Geometry)]
    [TestCase("polygon", DataType.Geometry)]
    [TestCase("multipoint", DataType.Geometry)]
    [TestCase("multilinestring", DataType.Geometry)]
    [TestCase("multipolygon", DataType.Geometry)]
    [TestCase("geometrycollection", DataType.Geometry)]
    public static void CreateColumnType_GivenTypeNameWithUnknownDataType_ResolvesExpectedDataType(string typeName, DataType expectedDataType)
    {
        var metadata = new ColumnTypeMetadata { TypeName = typeName, DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(expectedDataType));
    }

    [Test]
    public static void CreateColumnType_GivenUnrecognisedTypeName_ResolvesUnknownDataType()
    {
        var metadata = new ColumnTypeMetadata { TypeName = "not_a_real_type", DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(DataType.Unknown));
    }

    // Reverse mapping: a MySQL type name resolves to the matching CLR type.
    [TestCase("bit", typeof(bool))]
    [TestCase("tinyint", typeof(byte))]
    [TestCase("smallint", typeof(short))]
    [TestCase("mediumint", typeof(int))]
    [TestCase("int", typeof(int))]
    [TestCase("bigint", typeof(long))]
    [TestCase("decimal", typeof(decimal))]
    [TestCase("numeric", typeof(decimal))]
    [TestCase("double", typeof(double))]
    [TestCase("date", typeof(DateTime))]
    [TestCase("varchar", typeof(string))]
    [TestCase("longtext", typeof(string))]
    [TestCase("json", typeof(string))]
    [TestCase("blob", typeof(byte[]))]
    [TestCase("varbinary", typeof(byte[]))]
    [TestCase("geometry", typeof(object))]
    [TestCase("point", typeof(object))]
    public static void CreateColumnType_GivenTypeName_ResolvesExpectedClrType(string typeName, Type expectedClrType)
    {
        var metadata = new ColumnTypeMetadata { TypeName = typeName, DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.ClrType, Is.EqualTo(expectedClrType));
    }

    [Test]
    public static void CreateColumnType_GivenUnrecognisedTypeName_ResolvesObjectClrType()
    {
        var metadata = new ColumnTypeMetadata { TypeName = "not_a_real_type", DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.ClrType, Is.EqualTo(typeof(object)));
    }

    [Test]
    public static void CreateColumnType_GivenJsonDataType_ReturnsJsonColumnType()
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Json });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("json"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Json));
            Assert.That(columnType.ClrType, Is.EqualTo(typeof(string)));
        }
    }

    [Test]
    public static void CreateColumnType_GivenXmlDataType_ReturnsLongTextColumnType()
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Xml });

        // MySQL has no dedicated XML type; XML is stored as unbounded text.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("longtext"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Xml));
        }
    }

    [Test]
    public static void CreateColumnType_GivenGeometryDataType_ReturnsGeometryColumnType()
    {
        var columnType = Provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Geometry });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("geometry"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Geometry));
            Assert.That(columnType.ClrType, Is.EqualTo(typeof(object)));
        }
    }

    [Test]
    public static void GetComparableColumnType_GivenColumnType_ReturnsTypeWithMatchingDataType()
    {
        var sourceType = Provider.CreateColumnType(new ColumnTypeMetadata { TypeName = "json", DataType = DataType.Unknown });
        var comparableType = Provider.GetComparableColumnType(sourceType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(comparableType.DataType, Is.EqualTo(DataType.Json));
            Assert.That(comparableType.TypeName.LocalName, Is.EqualTo("json"));
        }
    }
}
