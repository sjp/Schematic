using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DatabaseSchemaMapperTests
{
    [Test]
    public static void Map_GivenOwnedSchema_RoundTripsToEquivalentSchema()
    {
        var mapper = new DatabaseSchemaMapper();
        var schema = new DatabaseSchema("test_schema", Option<string>.Some("test_owner"), true, false);

        var result = mapper.Map(mapper.Map(schema));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo(schema.Name));
            Assert.That(result.Owner.UnwrapSome(), Is.EqualTo("test_owner"));
            Assert.That(result.IsDefault, Is.True);
            Assert.That(result.IsSystem, Is.False);
        }
    }

    [Test]
    public static void Map_GivenUnownedSystemSchema_RoundTripsToEquivalentSchema()
    {
        var mapper = new DatabaseSchemaMapper();
        var schema = new DatabaseSchema("sys", Option<string>.None, false, true);

        var result = mapper.Map(mapper.Map(schema));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo(schema.Name));
            Assert.That(result.Owner, OptionIs.None);
            Assert.That(result.IsDefault, Is.False);
            Assert.That(result.IsSystem, Is.True);
        }
    }
}
