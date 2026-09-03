using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DatabaseUserDefinedTypeMapperTests
{
    [Test]
    public static void Map_GivenTypeWithNoDetail_RoundTripsToAnEquivalentType()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();
        var userDefinedType = new DatabaseUserDefinedType(
            Identifier.CreateQualifiedIdentifier("test_schema", "test_type"),
            UserDefinedTypeKind.Enum,
            Option<IDbType>.None);

        var result = mapper.Map(mapper.Map(userDefinedType));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo(userDefinedType.Name));
            Assert.That(result.Kind, Is.EqualTo(UserDefinedTypeKind.Enum));
            Assert.That(result.BaseType, OptionIs.None);
            Assert.That(result.EnumValues, Is.Empty);
            Assert.That(result.Attributes, Is.Empty);
            Assert.That(result.Checks, Is.Empty);
            Assert.That(result.IsNullable, Is.True);
            Assert.That(result.DefaultValue, OptionIs.None);
            Assert.That(result.Definition, OptionIs.None);
        }
    }

    [Test]
    public static void Map_GivenFullyDescribedType_RoundTripsToAnEquivalentType()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();

        var baseType = new ColumnDataType(
            Identifier.CreateQualifiedIdentifier("sys", "varchar"),
            DataType.String,
            "varchar(11)",
            typeof(string),
            false,
            11,
            Option<INumericPrecision>.None,
            Option<Identifier>.None);
        var attribute = new DatabaseColumn("attr", baseType, false, Option<string>.Some("'x'"), Option<IAutoIncrement>.None);
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "([value] > 0)", true);

        var userDefinedType = new DatabaseUserDefinedType(
            Identifier.CreateQualifiedIdentifier("test_schema", "test_type"),
            UserDefinedTypeKind.Domain,
            Option<IDbType>.Some(baseType),
            ["one", "two"],
            [attribute],
            [check],
            false,
            Option<string>.Some("'abc'"),
            Option<string>.Some("create type ..."));

        var result = mapper.Map(mapper.Map(userDefinedType));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo(userDefinedType.Name));
            Assert.That(result.Kind, Is.EqualTo(UserDefinedTypeKind.Domain));
            Assert.That(result.BaseType.UnwrapSome().TypeName, Is.EqualTo(baseType.TypeName));
            Assert.That(result.EnumValues, Is.EqualTo(new[] { "one", "two" }));
            Assert.That(result.Attributes.Select(a => a.Name.LocalName), Is.EqualTo(new[] { "attr" }));
            Assert.That(result.Attributes.Single().DefaultValue.UnwrapSome(), Is.EqualTo("'x'"));
            Assert.That(result.Checks.Single().Definition, Is.EqualTo("([value] > 0)"));
            Assert.That(result.IsNullable, Is.False);
            Assert.That(result.DefaultValue.UnwrapSome(), Is.EqualTo("'abc'"));
            Assert.That(result.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
        }
    }
}
