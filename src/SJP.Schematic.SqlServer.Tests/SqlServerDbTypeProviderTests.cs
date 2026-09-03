using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests;

[TestFixture]
internal static class SqlServerDbTypeProviderTests
{
    private static SqlServerDbTypeProvider Provider => new();

    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new SqlServerDbTypeProvider(), Throws.Nothing);
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

    // Reverse mapping: a SQL Server type name resolves to the matching generic data type.
    [TestCase("bigint", DataType.BigInteger)]
    [TestCase("bit", DataType.Boolean)]
    [TestCase("tinyint", DataType.TinyInteger)]
    [TestCase("smallint", DataType.SmallInteger)]
    [TestCase("int", DataType.Integer)]
    [TestCase("datetimeoffset", DataType.DateTimeOffset)]
    [TestCase("datetime2", DataType.DateTime)]
    [TestCase("time", DataType.Time)]
    [TestCase("money", DataType.Money)]
    [TestCase("smallmoney", DataType.Money)]
    [TestCase("numeric", DataType.Numeric)]
    [TestCase("rowversion", DataType.RowVersion)]
    [TestCase("timestamp", DataType.RowVersion)]
    [TestCase("sql_variant", DataType.Variant)]
    [TestCase("hierarchyid", DataType.Other)]
    [TestCase("sysname", DataType.Unicode)]
    [TestCase("vector", DataType.Vector)]
    [TestCase("image", DataType.LargeBinary)]
    [TestCase("varchar", DataType.String)]
    [TestCase("nvarchar", DataType.Unicode)]
    [TestCase("xml", DataType.Xml)]
    [TestCase("json", DataType.Json)]
    public static void CreateColumnType_GivenTypeNameWithUnknownDataType_ResolvesExpectedDataType(string typeName, DataType expectedDataType)
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("sys", typeName), DataType = DataType.Unknown, MaxLength = 10 };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(expectedDataType));
    }

    [TestCase("datetimeoffset", typeof(DateTimeOffset))]
    [TestCase("rowversion", typeof(byte[]))]
    [TestCase("sql_variant", typeof(object))]
    [TestCase("sysname", typeof(string))]
    [TestCase("tinyint", typeof(byte))]
    public static void CreateColumnType_GivenTypeName_ResolvesExpectedClrType(string typeName, Type expectedClrType)
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("sys", typeName), DataType = DataType.Unknown, MaxLength = 10 };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.ClrType, Is.EqualTo(expectedClrType));
    }

    [Test]
    public static void CreateColumnType_GivenUnrecognisedTypeName_ResolvesUnknownDataType()
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("sys", "not_a_real_type"), DataType = DataType.Unknown, MaxLength = 10 };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(DataType.Unknown));
    }

    // varbinary(max) carries no declared length, and holds a large object rather than an inline value
    [Test]
    public static void CreateColumnType_GivenUnboundedBinaryType_ResolvesLargeBinaryDataType()
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("sys", "varbinary"), DataType = DataType.Unknown, MaxLength = -1 };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(DataType.LargeBinary));
    }

    [Test]
    public static void CreateColumnType_GivenBoundedBinaryType_ResolvesBinaryDataType()
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("sys", "varbinary"), DataType = DataType.Unknown, MaxLength = 50 };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(DataType.Binary));
    }

    // Forward mapping: a generic data type resolves to its default SQL Server type name.
    [TestCase(DataType.TinyInteger, "tinyint")]
    [TestCase(DataType.DateTimeOffset, "datetimeoffset")]
    [TestCase(DataType.TimeOffset, "datetimeoffset")]
    [TestCase(DataType.Money, "money")]
    [TestCase(DataType.RowVersion, "rowversion")]
    [TestCase(DataType.Vector, "vector")]
    [TestCase(DataType.Bit, "varbinary")]
    [TestCase(DataType.Enum, "nvarchar")]
    [TestCase(DataType.Set, "nvarchar")]
    [TestCase(DataType.Network, "nvarchar")]
    [TestCase(DataType.FullTextSearch, "nvarchar")]
    [TestCase(DataType.Array, "sql_variant")]
    [TestCase(DataType.Range, "sql_variant")]
    [TestCase(DataType.Composite, "sql_variant")]
    [TestCase(DataType.Variant, "sql_variant")]
    [TestCase(DataType.Other, "sql_variant")]
    public static void CreateColumnType_GivenDataTypeWithoutTypeName_ReturnsExpectedTypeName(DataType dataType, string expectedTypeName)
    {
        var metadata = new ColumnTypeMetadata { DataType = dataType, MaxLength = 10 };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public static void GetComparableColumnType_GivenColumnType_PreservesEnumValues()
    {
        var sourceType = new ColumnDataType(
            new Identifier("app", "size"),
            DataType.Enum,
            "size",
            typeof(string),
            false,
            10,
            LanguageExt.Option<INumericPrecision>.None,
            LanguageExt.Option<Identifier>.None,
            LanguageExt.Option<IDbType>.None,
            ["small", "large"],
            LanguageExt.Option<IDbType>.None,
            false);

        var comparableType = Provider.GetComparableColumnType(sourceType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(comparableType.DataType, Is.EqualTo(DataType.Enum));
            Assert.That(comparableType.EnumValues, Is.EqualTo(new[] { "small", "large" }));
        }
    }
}
