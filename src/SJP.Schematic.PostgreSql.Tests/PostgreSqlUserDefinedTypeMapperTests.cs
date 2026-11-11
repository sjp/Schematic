using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Queries;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests;

internal static class PostgreSqlUserDefinedTypeMapperTests
{
    private static readonly Identifier TypeName = Identifier.CreateQualifiedIdentifier("public", "test_type");

    private static GetAllUserDefinedTypeDefinitions.Result CreateRow(string typeKind, string baseTypeName = null) => new()
    {
        SchemaName = "public",
        TypeName = "test_type",
        TypeKind = typeKind,
        IsNotNull = true,
        DefaultValue = "'abc'",
        BaseTypeName = baseTypeName,
        BaseTypeSchema = baseTypeName != null ? "pg_catalog" : null,
        CharacterMaximumLength = 0,
        NumericPrecision = 0,
        NumericPrecisionRadix = 0,
        NumericScale = 0,
        CollationName = null,
        EnumLabels = ["small", "large"],
    };

    private static T MapType<T>(GetAllUserDefinedTypeDefinitions.Result row, IDatabaseColumn[] attributes = null, IDatabaseCheckConstraint[] checks = null)
    {
        var result = PostgreSqlUserDefinedTypeMapper.MapType(TypeName, row, attributes ?? [], checks ?? [], new PostgreSqlDbTypeProvider());

        Assert.That(result.Value, Is.TypeOf<T>());
        return (T)result.Value!;
    }

    [Test]
    public static void MapType_GivenDomainRow_ReturnsDomainWithAliasedTypeNullabilityDefaultAndChecks()
    {
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "VALUE > 0", true);

        var domain = MapType<DatabaseUserDefinedType.Domain>(CreateRow("d", "int4"), checks: [check]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(domain.Name, Is.EqualTo(TypeName));
            Assert.That(domain.AliasedType.TypeName.LocalName, Is.EqualTo("int4"));
            Assert.That(domain.IsNullable, Is.False);
            Assert.That(domain.DefaultValue.UnwrapSome(), Is.EqualTo("'abc'"));
            Assert.That(domain.Checks, Is.EqualTo(new[] { check }));
        }
    }

    [Test]
    public static void MapType_GivenDomainRowWithoutBaseType_ReturnsUnknown()
    {
        var unknown = MapType<DatabaseUserDefinedType.Unknown>(CreateRow("d"));

        Assert.That(unknown.Definition, OptionIs.None);
    }

    [Test]
    public static void MapType_GivenEnumRow_ReturnsEnumWithLabels()
    {
        var enumType = MapType<DatabaseUserDefinedType.Enum>(CreateRow("e"));

        Assert.That(enumType.Values, Is.EqualTo(new[] { "small", "large" }));
    }

    [Test]
    public static void MapType_GivenEnumRowWithoutLabels_ReturnsEmptyEnum()
    {
        var enumType = MapType<DatabaseUserDefinedType.Enum>(CreateRow("e") with { EnumLabels = null });

        Assert.That(enumType.Values, Is.Empty);
    }

    [Test]
    public static void MapType_GivenCompositeRow_ReturnsCompositeWithAttributesAndNoDefinition()
    {
        var attribute = new DatabaseColumn("attr", TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        var composite = MapType<DatabaseUserDefinedType.Composite>(CreateRow("c"), attributes: [attribute]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(composite.Attributes.Single(), Is.SameAs(attribute));
            Assert.That(composite.Definition, OptionIs.None);
        }
    }

    [Test]
    public static void MapType_GivenRangeRow_ReturnsRangeWithSubtype()
    {
        var range = MapType<DatabaseUserDefinedType.Range>(CreateRow("r", "int8"));

        Assert.That(range.Subtype.TypeName.LocalName, Is.EqualTo("int8"));
    }

    [Test]
    public static void MapType_GivenRangeRowWithoutSubtype_ReturnsUnknown()
    {
        MapType<DatabaseUserDefinedType.Unknown>(CreateRow("r"));
    }

    [Test]
    public static void MapType_GivenRowOfUnrecognisedKind_ReturnsUnknown()
    {
        MapType<DatabaseUserDefinedType.Unknown>(CreateRow("m", "int4"));
    }
}
