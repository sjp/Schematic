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
    public static void Map_GivenEnumWithNoValues_RoundTripsToAnEmptyEnum()
    {
        var mapper = new DatabaseUserDefinedTypeMapper();
        var userDefinedType = new DatabaseUserDefinedType.Enum(
            Identifier.CreateQualifiedIdentifier("test_schema", "test_type"),
            []);

        var result = mapper.Map(mapper.Map(userDefinedType));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo(userDefinedType.Name));
            Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Enum>());
            Assert.That(((DatabaseUserDefinedType.Enum)result.Value!).Values, Is.Empty);
        }
    }

    [Test]
    public static void Map_GivenFullyDescribedDomain_RoundTripsToAnEquivalentDomain()
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
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "([value] > 0)", true);

        var userDefinedType = new DatabaseUserDefinedType.Domain(
            Identifier.CreateQualifiedIdentifier("test_schema", "test_type"),
            baseType,
            false,
            Option<string>.Some("'abc'"),
            [check]);

        var result = mapper.Map(mapper.Map(userDefinedType));

        Assert.That(result.Value, Is.TypeOf<DatabaseUserDefinedType.Domain>());
        var domain = (DatabaseUserDefinedType.Domain)result.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo(userDefinedType.Name));
            Assert.That(domain.AliasedType.TypeName, Is.EqualTo(baseType.TypeName));
            Assert.That(domain.Checks.Single().Definition, Is.EqualTo("([value] > 0)"));
            Assert.That(domain.IsNullable, Is.False);
            Assert.That(domain.DefaultValue.UnwrapSome(), Is.EqualTo("'abc'"));
        }
    }
}
