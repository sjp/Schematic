using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteDbTypeProviderTests
{
    // TODO
    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new SqliteDbTypeProvider(), Throws.Nothing);
    }

    [Test]
    public static void CreateColumnType_GivenJsonDataType_ReturnsTextAffinityColumnType()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Json });

        // SQLite has no dedicated JSON type; JSON is stored using text affinity.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("TEXT"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.UnicodeText));
        }
    }

    [Test]
    public static void CreateColumnType_GivenXmlDataType_ReturnsTextAffinityColumnType()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Xml });

        // SQLite has no dedicated XML type; XML is stored using text affinity.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("TEXT"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.UnicodeText));
        }
    }

    [Test]
    public static void CreateColumnType_GivenUniqueIdentifierDataType_ReturnsTextAffinityColumnType()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.UniqueIdentifier });

        // SQLite has no dedicated GUID type; UUIDs are stored using text affinity.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("TEXT"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.UnicodeText));
        }
    }

    // SQLite stores values by affinity, but the declared type is what the table definition says, so
    // it is reported unchanged rather than replaced by the name of the affinity
    [TestCase("VARCHAR(50)", "VARCHAR", DataType.UnicodeText)]
    [TestCase("DATETIME", "DATETIME", DataType.Numeric)]
    [TestCase("BOOLEAN", "BOOLEAN", DataType.Numeric)]
    [TestCase("DECIMAL(10, 2)", "DECIMAL", DataType.Numeric)]
    [TestCase("BIGINT", "BIGINT", DataType.BigInteger)]
    public static void CreateColumnType_GivenDeclaredTypeName_PreservesDeclaredType(string declaredTypeName, string expectedTypeName, DataType expectedDataType)
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { TypeName = declaredTypeName, DataType = DataType.Unknown });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo(expectedTypeName));
            Assert.That(columnType.Definition, Is.EqualTo(declaredTypeName));
            Assert.That(columnType.DataType, Is.EqualTo(expectedDataType));
        }
    }

    [Test]
    public static void CreateColumnType_GivenCollationOnTextColumn_AppliesCollation()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata
        {
            TypeName = "VARCHAR(50)",
            DataType = DataType.Unknown,
            Collation = LanguageExt.Option<Identifier>.Some("NOCASE"),
        });

        Assert.That(columnType.Collation.UnwrapSome().LocalName, Is.EqualTo("NOCASE"));
    }

    // a collation only applies to a text column, so one given for any other affinity is dropped
    [Test]
    public static void CreateColumnType_GivenCollationOnNonTextColumn_DropsCollation()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata
        {
            TypeName = "INTEGER",
            DataType = DataType.Unknown,
            Collation = LanguageExt.Option<Identifier>.Some("NOCASE"),
        });

        Assert.That(columnType.Collation.IsNone, Is.True);
    }

    [Test]
    public static void CreateColumnType_GivenNoDeclaredTypeName_NamesTypeByAffinity()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.BigInteger });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("INTEGER"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.BigInteger));
        }
    }

    // SQLite has no collection, enumerated, domain or unsigned types
    [Test]
    public static void CreateColumnType_GivenAnyColumn_DescribesNoTypeDetail()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { TypeName = "VARCHAR(50)", DataType = DataType.Unknown });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.ElementType.IsNone, Is.True);
            Assert.That(columnType.EnumValues, Is.Empty);
            Assert.That(columnType.BaseType.IsNone, Is.True);
            Assert.That(columnType.IsUnsigned, Is.False);
        }
    }

    [Test]
    public static void CreateColumnType_GivenGeometryDataType_ReturnsBlobAffinityColumnType()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Geometry });

        // SQLite has no dedicated spatial type; geometry is stored using blob affinity.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("BLOB"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.LargeBinary));
        }
    }
}