using System;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class IdentifierExtensionsTests
{
    [Test]
    public static void ToQualifiedName_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => IdentifierExtensions.ToQualifiedName(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ToQualifiedName_GivenLocalNameOnly_ReturnsLocalName()
    {
        var identifier = new Identifier("test_table");

        Assert.That(identifier.ToQualifiedName(), Is.EqualTo("test_table"));
    }

    [Test]
    public static void ToQualifiedName_GivenSchemaQualifiedName_ReturnsDottedName()
    {
        var identifier = Identifier.CreateQualifiedIdentifier("main", "test_table");

        Assert.That(identifier.ToQualifiedName(), Is.EqualTo("main.test_table"));
    }

    [Test]
    public static void ToQualifiedName_GivenFullyQualifiedName_ReturnsEveryPart()
    {
        var identifier = Identifier.CreateQualifiedIdentifier("server", "database", "schema", "test_table");

        Assert.That(identifier.ToQualifiedName(), Is.EqualTo("server.database.schema.test_table"));
    }

    [Test]
    public static void ToQualifiedName_GivenAnyIdentifier_DoesNotReturnDebuggerRepresentation()
    {
        var identifier = Identifier.CreateQualifiedIdentifier("main", "test_table");

        // Identifier.ToString() is documented as debug-only; this method exists so callers that
        // need a real name never reach for it.
        Assert.That(identifier.ToQualifiedName(), Is.Not.EqualTo(identifier.ToString()));
    }
}
