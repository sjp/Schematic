using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Oracle.Queries;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

internal static class OracleUserDefinedTypeMapperTests
{
    private static readonly Identifier TypeName = Identifier.CreateQualifiedIdentifier("TEST_SCHEMA", "TEST_TYPE");

    private const string Specification = "TYPE test_type AS ...";

    private static GetAllUserDefinedTypes.Result CreateRow(string typeCode, string elementTypeName = null) => new()
    {
        SchemaName = "TEST_SCHEMA",
        TypeName = "TEST_TYPE",
        TypeCode = typeCode,
        ElementTypeSchema = null,
        ElementTypeName = elementTypeName,
        ElementLength = elementTypeName != null ? 30 : 0,
        ElementCharUsed = elementTypeName != null ? "B" : null,
        ElementPrecision = null,
        ElementScale = null,
        ElementCollation = null,
    };

    private static T MapType<T>(GetAllUserDefinedTypes.Result row, IDatabaseColumn[] attributes = null)
    {
        var result = OracleUserDefinedTypeMapper.MapType(TypeName, row, attributes ?? [], Option<string>.Some(Specification), new OracleDbTypeProvider());

        Assert.That(result.Value, Is.TypeOf<T>());
        return (T)result.Value!;
    }

    [Test]
    public static void MapType_GivenObjectTypeRow_ReturnsCompositeWithAttributesAndSpecification()
    {
        var attribute = new DatabaseColumn("ATTR", TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        var composite = MapType<DatabaseUserDefinedType.Composite>(CreateRow("OBJECT"), [attribute]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(composite.Name, Is.EqualTo(TypeName));
            Assert.That(composite.Attributes.Single(), Is.SameAs(attribute));
            Assert.That(composite.Definition.UnwrapSome(), Is.EqualTo(Specification));
        }
    }

    [Test]
    public static void MapType_GivenCollectionTypeRow_ReturnsCollectionWithElementTypeAndSpecification()
    {
        var collection = MapType<DatabaseUserDefinedType.Collection>(CreateRow("COLLECTION", "VARCHAR2"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.ElementType.TypeName.LocalName, Is.EqualTo("VARCHAR2"));
            Assert.That(collection.ElementType.MaxLength, Is.EqualTo(30));
            Assert.That(collection.Definition.UnwrapSome(), Is.EqualTo(Specification));
        }
    }

    [Test]
    public static void MapType_GivenCollectionTypeRowWithoutElementType_ReturnsUnknownWithSpecification()
    {
        var unknown = MapType<DatabaseUserDefinedType.Unknown>(CreateRow("COLLECTION"));

        Assert.That(unknown.Definition.UnwrapSome(), Is.EqualTo(Specification));
    }

    [Test]
    public static void MapType_GivenRowOfUnrecognisedTypeCode_ReturnsUnknownWithSpecification()
    {
        var unknown = MapType<DatabaseUserDefinedType.Unknown>(CreateRow("XMLTYPE"));

        Assert.That(unknown.Definition.UnwrapSome(), Is.EqualTo(Specification));
    }
}
