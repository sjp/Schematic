using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDbTypeProviderTests
{
    private static PostgreSqlDbTypeProvider Provider => new();

    [Test]
    public static void Ctor_GivenNoArguments_CreatesWithoutError()
    {
        Assert.That(() => new PostgreSqlDbTypeProvider(), Throws.Nothing);
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

    // Reverse mapping: a PostgreSQL type name resolves to the matching generic data type.
    [TestCase("int2", DataType.SmallInteger)]
    [TestCase("int4", DataType.Integer)]
    [TestCase("int8", DataType.BigInteger)]
    [TestCase("bool", DataType.Boolean)]
    [TestCase("bytea", DataType.LargeBinary)]
    [TestCase("varchar", DataType.String)]
    [TestCase("char", DataType.String)]
    [TestCase("text", DataType.Text)]
    [TestCase("name", DataType.String)]
    [TestCase("oid", DataType.Integer)]
    [TestCase("timestamp", DataType.DateTime)]
    [TestCase("timestamptz", DataType.DateTimeOffset)]
    [TestCase("time", DataType.Time)]
    [TestCase("timetz", DataType.TimeOffset)]
    [TestCase("interval", DataType.Interval)]
    [TestCase("money", DataType.Money)]
    [TestCase("bit", DataType.Bit)]
    [TestCase("varbit", DataType.Bit)]
    [TestCase("inet", DataType.Network)]
    [TestCase("cidr", DataType.Network)]
    [TestCase("macaddr", DataType.Network)]
    [TestCase("macaddr8", DataType.Network)]
    [TestCase("tsvector", DataType.FullTextSearch)]
    [TestCase("tsquery", DataType.FullTextSearch)]
    [TestCase("pg_lsn", DataType.Other)]
    [TestCase("txid_snapshot", DataType.Other)]
    [TestCase("uuid", DataType.UniqueIdentifier)]
    [TestCase("jsonb", DataType.Json)]
    [TestCase("xml", DataType.Xml)]
    public static void CreateColumnType_GivenTypeNameWithUnknownDataType_ResolvesExpectedDataType(string typeName, DataType expectedDataType)
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("pg_catalog", typeName), DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(expectedDataType));
    }

    [TestCase("timestamptz", typeof(DateTimeOffset))]
    [TestCase("timetz", typeof(DateTimeOffset))]
    [TestCase("money", typeof(decimal))]
    [TestCase("oid", typeof(long))]
    public static void CreateColumnType_GivenTypeName_ResolvesExpectedClrType(string typeName, Type expectedClrType)
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("pg_catalog", typeName), DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.ClrType, Is.EqualTo(expectedClrType));
    }

    [Test]
    public static void CreateColumnType_GivenUnrecognisedTypeName_ResolvesUnknownDataType()
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("pg_catalog", "not_a_real_type"), DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(DataType.Unknown));
    }

    // Forward mapping: a generic data type resolves to its default PostgreSQL type name.
    [TestCase(DataType.DateTimeOffset, "timestamptz")]
    [TestCase(DataType.TimeOffset, "timetz")]
    [TestCase(DataType.Money, "money")]
    [TestCase(DataType.Network, "inet")]
    [TestCase(DataType.FullTextSearch, "tsvector")]
    [TestCase(DataType.TinyInteger, "int2")]
    [TestCase(DataType.LargeBinary, "bytea")]
    [TestCase(DataType.Array, "text")]
    [TestCase(DataType.Enum, "text")]
    [TestCase(DataType.Composite, "text")]
    [TestCase(DataType.Range, "text")]
    public static void CreateColumnType_GivenDataTypeWithoutTypeName_ReturnsExpectedTypeName(DataType dataType, string expectedTypeName)
    {
        var metadata = new ColumnTypeMetadata { DataType = dataType };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    [TestCase(DataType.Bit, true, "bit")]
    [TestCase(DataType.Bit, false, "varbit")]
    public static void CreateColumnType_GivenBitDataType_ReturnsExpectedTypeName(DataType dataType, bool isFixedLength, string expectedTypeName)
    {
        var metadata = new ColumnTypeMetadata { DataType = dataType, IsFixedLength = isFixedLength };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
    }

    // an empty '()' is not valid syntax, so an unbounded type carries no length annotation at all
    [Test]
    public static void CreateColumnType_GivenUnboundedType_ReturnsDefinitionWithoutLength()
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("pg_catalog", "timestamp"), DataType = DataType.Unknown };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.Definition, Is.EqualTo("\"timestamp\""));
    }

    // a length is declared in characters rather than in bytes, so it is printed as it was given
    [Test]
    public static void CreateColumnType_GivenBoundedType_ReturnsDefinitionWithLength()
    {
        var metadata = new ColumnTypeMetadata { TypeName = new Identifier("pg_catalog", "varchar"), DataType = DataType.Unknown, MaxLength = 50 };
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.Definition, Is.EqualTo("\"varchar\"(50)"));
    }

    [Test]
    public static void CreateTypeMetadata_GivenArrayColumn_DescribesArrayWithElementType()
    {
        var typeInfo = new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
            "ARRAY", "pg_catalog", "_int4", null, null, "b", "pg_catalog", "int4", "b", null);

        var metadata = PostgreSqlColumnTypeMetadata.Create(Provider, typeInfo, Option<Identifier>.None, 0, Option<INumericPrecision>.None);
        var columnType = Provider.CreateColumnType(metadata);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Array));
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("_int4"));
            Assert.That(columnType.ElementType.UnwrapSome().DataType, Is.EqualTo(DataType.Integer));
        }
    }

    [Test]
    public static void CreateTypeMetadata_GivenEnumColumn_DescribesEnumWithValues()
    {
        var typeInfo = new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
            "USER-DEFINED", "app", "size", null, null, "e", null, null, null, ["small", "large"]);

        var metadata = PostgreSqlColumnTypeMetadata.Create(Provider, typeInfo, Option<Identifier>.None, 0, Option<INumericPrecision>.None);
        var columnType = Provider.CreateColumnType(metadata);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Enum));
            Assert.That(columnType.TypeName, Is.EqualTo(new Identifier("app", "size")));
            Assert.That(columnType.EnumValues, Is.EqualTo(new[] { "small", "large" }));
        }
    }

    [Test]
    public static void CreateTypeMetadata_GivenArrayOfEnumColumn_DescribesArrayWithEnumElement()
    {
        var typeInfo = new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
            "ARRAY", "app", "_size", null, null, "b", "app", "size", "e", ["small", "large"]);

        var metadata = PostgreSqlColumnTypeMetadata.Create(Provider, typeInfo, Option<Identifier>.None, 0, Option<INumericPrecision>.None);
        var columnType = Provider.CreateColumnType(metadata);
        var elementType = columnType.ElementType.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Array));
            Assert.That(elementType.DataType, Is.EqualTo(DataType.Enum));
            Assert.That(elementType.EnumValues, Is.EqualTo(new[] { "small", "large" }));
        }
    }

    [TestCase("c", DataType.Composite)]
    [TestCase("r", DataType.Range)]
    [TestCase("m", DataType.Range)]
    [TestCase("b", DataType.Other)]
    public static void CreateTypeMetadata_GivenUserDefinedColumn_DescribesExpectedDataType(string typeKind, DataType expectedDataType)
    {
        var typeInfo = new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
            "USER-DEFINED", "app", "test_type", null, null, typeKind, null, null, null, null);

        var metadata = PostgreSqlColumnTypeMetadata.Create(Provider, typeInfo, Option<Identifier>.None, 0, Option<INumericPrecision>.None);
        var columnType = Provider.CreateColumnType(metadata);

        Assert.That(columnType.DataType, Is.EqualTo(expectedDataType));
    }

    // a domain is reported by the type it is defined over, so the domain names the column's type
    // while the type reported for it becomes the base type
    [Test]
    public static void CreateTypeMetadata_GivenDomainColumn_DescribesDomainOverBaseType()
    {
        var typeInfo = new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
            "character varying", "pg_catalog", "varchar", "app", "email_address", "b", null, null, null, null);

        var metadata = PostgreSqlColumnTypeMetadata.Create(Provider, typeInfo, Option<Identifier>.None, 100, Option<INumericPrecision>.None);
        var columnType = Provider.CreateColumnType(metadata);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName, Is.EqualTo(new Identifier("app", "email_address")));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.String));
            Assert.That(columnType.BaseType.UnwrapSome().TypeName.LocalName, Is.EqualTo("character varying"));
        }
    }

    [Test]
    public static void CreateTypeMetadata_GivenBuiltInColumn_QualifiesTypeNameWithCatalogSchema()
    {
        var typeInfo = new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
            "integer", "pg_catalog", "int4", null, null, "b", null, null, null, null);

        var metadata = PostgreSqlColumnTypeMetadata.Create(Provider, typeInfo, Option<Identifier>.None, 0, Option<INumericPrecision>.None);
        var columnType = Provider.CreateColumnType(metadata);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName, Is.EqualTo(new Identifier("pg_catalog", "integer")));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.Integer));
        }
    }
}
