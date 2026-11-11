using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DatabaseUserDefinedTypeMapperTests
{
    private static readonly Identifier TypeName = Identifier.CreateQualifiedIdentifier("test_schema", "test_type");

    private static readonly IDbType VarcharType = new ColumnDataType(
        Identifier.CreateQualifiedIdentifier("sys", "varchar"),
        DataType.String,
        "varchar(11)",
        typeof(string),
        false,
        11,
        Option<INumericPrecision>.None,
        Option<Identifier>.None);

    private static IDatabaseColumn CreateColumn(string columnName) => new DatabaseColumn(columnName, VarcharType, false, Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Literal("'x'")), new ColumnValueSource.Stored());

    private static IDatabaseCheckConstraint CreateCheck(string checkName) => new DatabaseCheckConstraint(Option<Identifier>.Some(checkName), "([value] > 0)", true);

    private static Dto.DatabaseUserDefinedType CreateTypeDto(Dto.UserDefinedTypeKind kind)
    {
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();
        var columnMapper = new DatabaseColumnMapper();
        var checkMapper = new DatabaseCheckMapper();

        // every field is filled in, whether or not the kind has it, so that reading the DTO shows
        // which fields each kind takes and which it ignores
        return new Dto.DatabaseUserDefinedType
        {
            TypeName = identifierMapper.Map(TypeName),
            Kind = kind,
            BaseType = dbTypeMapper.Map(VarcharType),
            EnumValues = ["one", "two"],
            Attributes = [columnMapper.Map(CreateColumn("attr"))],
            Checks = [checkMapper.Map(CreateCheck("ck_test"))],
            IsNullable = false,
            DefaultValue = "'abc'",
            Definition = "create type ...",
        };
    }

    private static IEnumerable<TestCaseData> TypesWithKinds()
    {
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Unknown(TypeName, "create type ..."), Dto.UserDefinedTypeKind.Unknown).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Unknown));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Alias(TypeName, VarcharType, false, "'abc'"), Dto.UserDefinedTypeKind.Alias).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Alias));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Domain(TypeName, VarcharType, false, "'abc'", [CreateCheck("ck_test")]), Dto.UserDefinedTypeKind.Domain).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Domain));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Enum(TypeName, ["one", "two"]), Dto.UserDefinedTypeKind.Enum).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Enum));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Composite(TypeName, [CreateColumn("attr")], "create type ..."), Dto.UserDefinedTypeKind.Composite).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Composite));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Range(TypeName, VarcharType), Dto.UserDefinedTypeKind.Range).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Range));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Table(TypeName, [CreateColumn("attr")], [CreateCheck("ck_test")]), Dto.UserDefinedTypeKind.Table).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Table));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Clr(TypeName, "TestAssembly.TestNamespace.TestType"), Dto.UserDefinedTypeKind.Clr).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Clr));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Collection(TypeName, VarcharType, "create type ..."), Dto.UserDefinedTypeKind.Collection).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Collection));
    }

    [TestCaseSource(nameof(TypesWithKinds))]
    public static void Map_GivenTypeOfEachCase_WritesKindAndName(DatabaseUserDefinedType userDefinedType, Dto.UserDefinedTypeKind expectedKind)
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(userDefinedType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Kind, Is.EqualTo(expectedKind));
            Assert.That(result.TypeName.Schema, Is.EqualTo("test_schema"));
            Assert.That(result.TypeName.LocalName, Is.EqualTo("test_type"));
        }
    }

    [TestCaseSource(nameof(TypesWithKinds))]
    public static void Map_GivenTypeOfEachCase_RoundTripsAsSameCase(DatabaseUserDefinedType userDefinedType, Dto.UserDefinedTypeKind _)
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(mapper.Map(userDefinedType));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.TypeOf(userDefinedType.Value!.GetType()));
            Assert.That(result.Name, Is.EqualTo(userDefinedType.Name));
        }
    }

    [TestCaseSource(nameof(TypesWithKinds))]
    public static void Map_GivenTypeOfEachCase_RoundTripsToEquivalentDto(DatabaseUserDefinedType userDefinedType, Dto.UserDefinedTypeKind _)
    {
        var mapper = new DatabaseUserDefinedTypeMapper();
        var dto = mapper.Map(userDefinedType);

        var result = mapper.Map(mapper.Map(dto));

        Assert.That(result, Is.EqualTo(dto).Using<Dto.DatabaseUserDefinedType>(DtoEquals));
    }

    [Test]
    public static void Map_GivenDefaultType_ThrowsArgumentNullException()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        Assert.That(
            () => mapper.Map(default(DatabaseUserDefinedType)),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void Map_GivenAlias_WritesAliasedTypeAsBaseTypeWithNullabilityAndDefault()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Alias(TypeName, VarcharType, false, "'abc'"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BaseType!.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(result.IsNullable, Is.False);
            Assert.That(result.DefaultValue, Is.EqualTo("'abc'"));
            Assert.That(result.Checks, Is.Empty);
            Assert.That(result.Definition, Is.Null);
        }
    }

    [Test]
    public static void Map_GivenDomain_WritesAliasedTypeAsBaseTypeWithNullabilityDefaultAndChecks()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Domain(TypeName, VarcharType, false, "'abc'", [CreateCheck("ck_test")]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BaseType!.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(result.IsNullable, Is.False);
            Assert.That(result.DefaultValue, Is.EqualTo("'abc'"));
            Assert.That(result.Checks.Select(static c => c.CheckName!.LocalName), Is.EqualTo(new[] { "ck_test" }));
        }
    }

    [Test]
    public static void Map_GivenEnum_WritesValuesAndNothingElse()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Enum(TypeName, ["one", "two"]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.EnumValues, Is.EqualTo(new[] { "one", "two" }));
            Assert.That(result.BaseType, Is.Null);
            Assert.That(result.Attributes, Is.Empty);
            Assert.That(result.Checks, Is.Empty);
            Assert.That(result.IsNullable, Is.True);
            Assert.That(result.DefaultValue, Is.Null);
            Assert.That(result.Definition, Is.Null);
        }
    }

    [Test]
    public static void Map_GivenComposite_WritesAttributesAndDefinition()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Composite(TypeName, [CreateColumn("attr")], "create type ..."));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Attributes.Select(static a => a.ColumnName.LocalName), Is.EqualTo(new[] { "attr" }));
            Assert.That(result.Definition, Is.EqualTo("create type ..."));
            Assert.That(result.BaseType, Is.Null);
            Assert.That(result.IsNullable, Is.True);
        }
    }

    [Test]
    public static void Map_GivenRange_WritesSubtypeAsBaseType()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Range(TypeName, VarcharType));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BaseType!.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(result.IsNullable, Is.True);
        }
    }

    [Test]
    public static void Map_GivenTable_WritesColumnsAsAttributesWithChecks()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Table(TypeName, [CreateColumn("col")], [CreateCheck("ck_test")]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Attributes.Select(static a => a.ColumnName.LocalName), Is.EqualTo(new[] { "col" }));
            Assert.That(result.Checks.Select(static c => c.CheckName!.LocalName), Is.EqualTo(new[] { "ck_test" }));
            Assert.That(result.BaseType, Is.Null);
            Assert.That(result.IsNullable, Is.True);
        }
    }

    [Test]
    public static void Map_GivenClr_WritesExternalNameAsDefinition()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Clr(TypeName, "TestAssembly.TestNamespace.TestType"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Definition, Is.EqualTo("TestAssembly.TestNamespace.TestType"));
            Assert.That(result.BaseType, Is.Null);
            Assert.That(result.IsNullable, Is.True);
        }
    }

    [Test]
    public static void Map_GivenCollection_WritesElementTypeAsBaseTypeWithDefinition()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(new DatabaseUserDefinedType.Collection(TypeName, VarcharType, "create type ..."));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BaseType!.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(result.Definition, Is.EqualTo("create type ..."));
            Assert.That(result.IsNullable, Is.True);
        }
    }

    [Test]
    public static void Map_GivenSerializedUnknown_ReadsDefinitionOnly()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Unknown));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Unknown>());
        Assert.That(((DatabaseUserDefinedType.Unknown)result.Value!).Definition.UnwrapSome(), Is.EqualTo("create type ..."));
    }

    [Test]
    public static void Map_GivenSerializedAlias_ReadsBaseTypeAsAliasedTypeWithNullabilityAndDefault()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Alias));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Alias>());
        var alias = (DatabaseUserDefinedType.Alias)result.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(alias.AliasedType.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(alias.IsNullable, Is.False);
            Assert.That(alias.DefaultValue.UnwrapSome(), Is.EqualTo("'abc'"));
        }
    }

    [Test]
    public static void Map_GivenSerializedDomain_ReadsBaseTypeAsAliasedTypeWithNullabilityDefaultAndChecks()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Domain));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Domain>());
        var domain = (DatabaseUserDefinedType.Domain)result.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(domain.AliasedType.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(domain.IsNullable, Is.False);
            Assert.That(domain.DefaultValue.UnwrapSome(), Is.EqualTo("'abc'"));
            Assert.That(domain.Checks.Select(static c => c.Definition), Is.EqualTo(new[] { "([value] > 0)" }));
        }
    }

    [Test]
    public static void Map_GivenSerializedEnum_ReadsValues()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Enum));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Enum>());
        Assert.That(((DatabaseUserDefinedType.Enum)result.Value!).Values, Is.EqualTo(new[] { "one", "two" }));
    }

    [Test]
    public static void Map_GivenSerializedComposite_ReadsAttributesAndDefinition()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Composite));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Composite>());
        var composite = (DatabaseUserDefinedType.Composite)result.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(composite.Attributes.Select(static a => a.Name.LocalName), Is.EqualTo(new[] { "attr" }));
            Assert.That(composite.Attributes.Single().Default.UnwrapSome().Value, Is.TypeOf<DatabaseDefaultValue.Literal>());
            Assert.That(composite.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
        }
    }

    [Test]
    public static void Map_GivenSerializedRange_ReadsBaseTypeAsSubtype()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Range));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Range>());
        Assert.That(((DatabaseUserDefinedType.Range)result.Value!).Subtype.TypeName.LocalName, Is.EqualTo("varchar"));
    }

    [Test]
    public static void Map_GivenSerializedTable_ReadsAttributesAsColumnsWithChecks()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Table));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Table>());
        var table = (DatabaseUserDefinedType.Table)result.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "attr" }));
            Assert.That(table.Checks.Select(static c => c.Definition), Is.EqualTo(new[] { "([value] > 0)" }));
        }
    }

    [Test]
    public static void Map_GivenSerializedClr_ReadsDefinitionAsExternalName()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Clr));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Clr>());
        Assert.That(((DatabaseUserDefinedType.Clr)result.Value!).ExternalName.UnwrapSome(), Is.EqualTo("create type ..."));
    }

    [Test]
    public static void Map_GivenSerializedCollection_ReadsBaseTypeAsElementTypeWithDefinition()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var result = mapper.Map(CreateTypeDto(Dto.UserDefinedTypeKind.Collection));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Collection>());
        var collection = (DatabaseUserDefinedType.Collection)result.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.ElementType.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(collection.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
        }
    }

    [TestCase(Dto.UserDefinedTypeKind.Alias)]
    [TestCase(Dto.UserDefinedTypeKind.Domain)]
    [TestCase(Dto.UserDefinedTypeKind.Range)]
    [TestCase(Dto.UserDefinedTypeKind.Collection)]
    public static void Map_GivenSerializedKindWithoutBaseType_ReadsBackAsUnknownKeepingDefinition(Dto.UserDefinedTypeKind kind)
    {
        var mapper = new DatabaseUserDefinedTypeMapper();
        var dto = CreateTypeDto(kind) with { BaseType = null };

        var result = mapper.Map(dto);

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Unknown>());
        var unknown = (DatabaseUserDefinedType.Unknown)result.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unknown.Name, Is.EqualTo(TypeName));
            Assert.That(unknown.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
        }
    }

    [Test]
    public static void Map_GivenSerializedTypeWithInvalidKind_ThrowsArgumentException()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();
        var dto = CreateTypeDto((Dto.UserDefinedTypeKind)55);

        Assert.That(
            () => mapper.Map(dto),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    private static bool DtoEquals(Dto.DatabaseUserDefinedType x, Dto.DatabaseUserDefinedType y)
    {
        return x.TypeName == y.TypeName
            && x.Kind == y.Kind
            && x.BaseType?.TypeName == y.BaseType?.TypeName
            && x.EnumValues.SequenceEqual(y.EnumValues)
            && x.Attributes.Select(static a => a.ColumnName).SequenceEqual(y.Attributes.Select(static a => a.ColumnName))
            && x.Checks.Select(static c => c.CheckName).SequenceEqual(y.Checks.Select(static c => c.CheckName))
            && x.IsNullable == y.IsNullable
            && x.DefaultValue == y.DefaultValue
            && x.Definition == y.Definition;
    }
}
