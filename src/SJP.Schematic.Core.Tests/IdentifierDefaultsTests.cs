using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class IdentifierDefaultsTests
{
    [Test]
    public static void Ctor_GivenValidNames_PropertyGetsMatchCtorArguments()
    {
        var identifierDefaults = new IdentifierDefaults("server", "database", "schema");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifierDefaults.Server, Is.EqualTo("server"));
            Assert.That(identifierDefaults.Database, Is.EqualTo("database"));
            Assert.That(identifierDefaults.Schema, Is.EqualTo("schema"));
        }
    }

    [Test, Combinatorial]
    public static void Ctor_GivenNullEmptyOrWhiteSpaceNames_PropertyGetsAreNull(
        [Values(null, "", "    ")] string server,
        [Values(null, "", "    ")] string database,
        [Values(null, "", "    ")] string schema
    )
    {
        var identifierDefaults = new IdentifierDefaults(server, database, schema);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifierDefaults.Server, Is.Null);
            Assert.That(identifierDefaults.Database, Is.Null);
            Assert.That(identifierDefaults.Schema, Is.Null);
        }
    }

    [Test]
    public static void Empty_PropertyGet_HasNoNamesSet()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(IdentifierDefaults.Empty.Server, Is.Null);
            Assert.That(IdentifierDefaults.Empty.Database, Is.Null);
            Assert.That(IdentifierDefaults.Empty.Schema, Is.Null);
        }
    }
}
