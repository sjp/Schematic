using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class IdentifierTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceLocalName_ThrowsArgumentNullException(string localName)
    {
        Assert.That(() => new Identifier(localName), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void LocalName_PropertyGet_EqualsCtorArgument()
    {
        const string name = "test";
        var identifier = new Identifier(name);
        Assert.That(identifier.LocalName, Is.EqualTo(name));
    }

    [Test, Combinatorial]
    public static void Ctor_GivenNullWhiteSpaceSchemaAndLocalNames_ThrowsArgumentException(
        [Values(null, "", "    ")] string schemaName,
        [Values(null, "", "    ")] string localName
    )
    {
        Assert.That(() => new Identifier(schemaName, localName), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void SchemaAndLocalName_PropertyGets_MatchCtorArguments()
    {
        const string localName = "local";
        const string schema = "schema";
        var identifier = new Identifier(schema, localName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.LocalName, Is.EqualTo(localName));
            Assert.That(identifier.Schema, Is.EqualTo(schema));
        }
    }

    [Test, Combinatorial]
    public static void Ctor_GivenNullWhiteSpaceDatabaseAndSchemaAndLocalNames_ThrowsArgumentNullException(
        [Values(null, "", "    ")] string databaseName,
        [Values(null, "", "    ")] string schemaName,
        [Values(null, "", "    ")] string localName
    )
    {
        Assert.That(() => new Identifier(databaseName, schemaName, localName), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void DatabaseAndSchemaAndLocalName_PropertyGets_MatchCtorArguments()
    {
        const string localName = "local";
        const string schema = "schema";
        const string database = "database";
        var identifier = new Identifier(database, schema, localName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.LocalName, Is.EqualTo(localName));
            Assert.That(identifier.Schema, Is.EqualTo(schema));
            Assert.That(identifier.Database, Is.EqualTo(database));
        }
    }

    [Test]
    public static void Ctor_GivenNullWhiteSpaceServerAndDatabaseAndSchemaAndLocalNames_ThrowsArgumentNullException(
        [Values(null, "", "    ")] string serverName,
        [Values(null, "", "    ")] string databaseName,
        [Values(null, "", "    ")] string schemaName,
        [Values(null, "", "    ")] string localName
    )
    {
        Assert.That(() => new Identifier(serverName, databaseName, schemaName, localName), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void ServerAndDatabaseAndSchemaAndLocalName_PropertyGets_MatchCtorArguments()
    {
        const string localName = "local";
        const string schema = "schema";
        const string database = "database";
        const string server = "server";
        var identifier = new Identifier(server, database, schema, localName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.LocalName, Is.EqualTo(localName));
            Assert.That(identifier.Schema, Is.EqualTo(schema));
            Assert.That(identifier.Database, Is.EqualTo(database));
            Assert.That(identifier.Server, Is.EqualTo(server));
        }
    }

    [Test]
    public static void Equals_GivenEqualIdentifiers_ReturnsTrue()
    {
        const string name = "test";
        var identifier = new Identifier(name, name);
        var otherIdentifier = new Identifier(name, name);
        Assert.That(identifier, Is.EqualTo(otherIdentifier));
    }

    [Test]
    public static void Equals_GivenDifferentIdentifiers_ReturnsFalse()
    {
        const string name = "test";
        const string otherName = "another";
        var identifier = new Identifier(name, name);
        var otherIdentifier = new Identifier(otherName, name);
        Assert.That(identifier, Is.Not.EqualTo(otherIdentifier));
    }

    [Test]
    public static void Equals_GivenDifferentIdentifierAsObject_ReturnsFalse()
    {
        const string name = "test";
        const string otherName = "another";
        var identifier = new Identifier(name, name);
        object otherIdentifier = new Identifier(otherName, name);

        Assert.That(identifier, Is.Not.EqualTo(otherIdentifier));
    }

    [Test]
    public static void Equals_GivenDifferentIdentifierAsObject_ReturnsTrue()
    {
        const string name = "test";
        var identifier = new Identifier(name, name);
        object otherIdentifier = new Identifier(name, name);

        var areEqual = identifier.Equals(otherIdentifier);

        Assert.That(areEqual, Is.True);
    }

    [Test]
    public static void EqualsOp_GivenEqualIdentifiers_ReturnsTrue()
    {
        const string name = "test";
        var identifier = new Identifier(name, name);
        var otherIdentifier = new Identifier(name, name);
        var isEqual = identifier == otherIdentifier;
        Assert.That(isEqual, Is.True);
    }

    [Test]
    public static void EqualsOp_GivenDifferentIdentifiers_ReturnsFalse()
    {
        const string name = "test";
        const string otherName = "alternative";
        var identifier = new Identifier(name, name);
        var otherIdentifier = new Identifier(otherName, name);
        var isEqual = identifier == otherIdentifier;
        Assert.That(isEqual, Is.False);
    }

    [Test]
    public static void ObjectsEquals_GivenEqualIdentifiers_ReturnsTrue()
    {
        const string name = "test";
        object identifier = new Identifier(name, name);
        object otherIdentifier = new Identifier(name, name);

        Assert.That(identifier, Is.EqualTo(otherIdentifier));
    }

    [Test]
    public static void ObjectsEquals_GivenDifferentObjects_ReturnsFalse()
    {
        const string name = "test";
        const string otherName = "another";
        object identifier = new Identifier(name, name);
        object otherIdentifier = new Identifier(otherName, name);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier, Is.Not.Null);
            Assert.That(identifier, Is.Not.EqualTo(1));
            Assert.That(identifier, Is.Not.EqualTo(otherIdentifier));
        }
    }

    [Test]
    public static void Identifier_WhenOnlyLocalNameProvided_OnlyHasLocalNamePropertySet()
    {
        var identifier = new Identifier("test");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.Null);
            Assert.That(identifier.LocalName, Is.Not.Null);
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenFullyQualifiedArguments_CreatesFullyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier("a", "b", "c", "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.EqualTo("a"));
            Assert.That(identifier.Database, Is.EqualTo("b"));
            Assert.That(identifier.Schema, Is.EqualTo("c"));
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsWithoutServer_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier(null, "b", "c", "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.EqualTo("b"));
            Assert.That(identifier.Schema, Is.EqualTo("c"));
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabase_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier(null, null, "c", "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.EqualTo("c"));
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabaseAndSchema_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier(null, null, null, "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.Null);
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsMissingServer_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier("b", "c", "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.EqualTo("b"));
            Assert.That(identifier.Schema, Is.EqualTo("c"));
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsMissingServerAndWithoutDatabase_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier(null, "c", "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.EqualTo("c"));
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsMissingServerAndWithoutDatabaseAndSchema_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier(null, null, "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.Null);
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsWithOnlyDatabaseAndLocalName_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier("c", "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.EqualTo("c"));
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsWithoutSchemaAndWithLocalName_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier(null, "d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.Null);
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [Test]
    public static void CreateQualifiedIdentifier_GivenArgumentsWithOnlyLocalName_CreatesCorrectlyQualifiedIdentifier()
    {
        var identifier = Identifier.CreateQualifiedIdentifier("d");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(identifier.Server, Is.Null);
            Assert.That(identifier.Database, Is.Null);
            Assert.That(identifier.Schema, Is.Null);
            Assert.That(identifier.LocalName, Is.EqualTo("d"));
        }
    }

    [TestCase(null, null, null, null)]
    [TestCase("a", null, "c", "d")]
    [TestCase("a", "b", null, "d")]
    [TestCase("a", "b", "c", null)]
    [TestCase(null, "b", null, "d")]
    [TestCase(null, "b", null, null)]
    [TestCase(null, null, "c", null)]
    public static void CreateQualifiedIdentifier_GivenInvalidArguments_ThrowsArgumentNullException(string serverName, string databaseName, string schemaName, string localName)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName), Throws.ArgumentNullException);
            Assert.That(() => Identifier.CreateQualifiedIdentifier(null, null, null, null), Throws.ArgumentNullException);
        }
    }

    [Test]
    public static void CompareTo_GivenSameIdentifier_ReturnsZero()
    {
        var identifier = new Identifier("name", "name", "name", "test");

        var compareResult = identifier.CompareTo(identifier);

        Assert.That(compareResult, Is.Zero);
    }

    [Test]
    public static void CompareTo_GivenNullIdentifier_ReturnsNonZero()
    {
        var identifier = new Identifier("name", "name", "name", "test");

        var compareResult = identifier.CompareTo(null);

        Assert.That(compareResult, Is.Not.Zero);
    }

    [Test]
    public static void CompareTo_GivenEqualIdentifiers_ReturnsZero()
    {
        var identifier = new Identifier("name", "name", "name", "test");
        var otherIdentifier = new Identifier("name", "name", "name", "test");

        var compareResult = identifier.CompareTo(otherIdentifier);

        Assert.That(compareResult, Is.Zero);
    }

    [Test]
    public static void CompareTo_GivenDifferentIdentifiers_ReturnsNonZero()
    {
        var identifier = new Identifier("name", "name", "name", "test");
        var otherIdentifier = new Identifier("name", "name", "name", "name");

        var compareResult = identifier.CompareTo(otherIdentifier);

        Assert.That(compareResult, Is.Not.Zero);
    }

    [Test]
    public static void GtOp_GivenDifferentServer_ReturnsTrueWhenExpected()
    {
        var identifier = new Identifier("z", "name", "name", "name");
        var otherIdentifier = new Identifier("name", "name", "name", "name");

        var isGt = identifier > otherIdentifier;

        Assert.That(isGt, Is.True);
    }

    [Test]
    public static void GtOp_GivenDifferentServer_ReturnsFalseWhenExpected()
    {
        var identifier = new Identifier("name", "name", "name", "name");
        var otherIdentifier = new Identifier("z", "name", "name", "name");

        var isGt = identifier > otherIdentifier;

        Assert.That(isGt, Is.False);
    }

    [Test]
    public static void GteOp_GivenDifferentServer_ReturnsTrueWhenExpected()
    {
        var identifier = new Identifier("z", "name", "name", "name");
        var otherIdentifier = new Identifier("name", "name", "name", "name");

        var isGte = identifier >= otherIdentifier;

        Assert.That(isGte, Is.True);
    }

    [Test]
    public static void GteOp_GivenDifferentServer_ReturnsFalseWhenExpected()
    {
        var identifier = new Identifier("name", "name", "name", "name");
        var otherIdentifier = new Identifier("z", "name", "name", "name");

        var isGte = identifier >= otherIdentifier;

        Assert.That(isGte, Is.False);
    }

    [Test]
    public static void GteOp_GivenSameIdentifiers_ReturnsTrue()
    {
        var identifier = new Identifier("name", "name", "name", "name");
        var otherIdentifier = new Identifier("name", "name", "name", "name");

        var isGte = identifier >= otherIdentifier;

        Assert.That(isGte, Is.True);
    }

    [Test]
    public static void LtOp_GivenDifferentServer_ReturnsFalseWhenExpected()
    {
        var identifier = new Identifier("z", "name", "name", "name");
        var otherIdentifier = new Identifier("name", "name", "name", "name");

        var isLt = identifier < otherIdentifier;

        Assert.That(isLt, Is.False);
    }

    [Test]
    public static void LtOp_GivenDifferentServer_ReturnsTrueWhenExpected()
    {
        var identifier = new Identifier("name", "name", "name", "name");
        var otherIdentifier = new Identifier("z", "name", "name", "name");

        var isLt = identifier < otherIdentifier;

        Assert.That(isLt, Is.True);
    }

    [Test]
    public static void LteOp_GivenDifferentServer_ReturnsFalseWhenExpected()
    {
        var identifier = new Identifier("z", "name", "name", "name");
        var otherIdentifier = new Identifier("name", "name", "name", "name");

        var isLte = identifier <= otherIdentifier;

        Assert.That(isLte, Is.False);
    }

    [Test]
    public static void LteOp_GivenDifferentServer_ReturnsTrueWhenExpected()
    {
        var identifier = new Identifier("name", "name", "name", "name");
        var otherIdentifier = new Identifier("z", "name", "name", "name");

        var isLte = identifier <= otherIdentifier;

        Assert.That(isLte, Is.True);
    }

    [Test]
    public static void LteOp_GivenSameIdentifiers_ReturnsTrue()
    {
        var identifier = new Identifier("name", "name", "name", "name");
        var otherIdentifier = new Identifier("name", "name", "name", "name");

        var isLte = identifier <= otherIdentifier;

        Assert.That(isLte, Is.True);
    }

    [TestCase("", "", "", "localName", "LocalName = localName")]
    [TestCase("", "", "schemaName", "localName", "Schema = schemaName, LocalName = localName")]
    [TestCase("", "databaseName", "schemaName", "localName", "Database = databaseName, Schema = schemaName, LocalName = localName")]
    [TestCase("serverName", "databaseName", "schemaName", "localName", "Server = serverName, Database = databaseName, Schema = schemaName, LocalName = localName")]
    public static void ToString_WhenInvoked_ReturnsExpectedOutput(string server, string database, string schema, string localName, string expectedOutput)
    {
        var identifier = Identifier.CreateQualifiedIdentifier(server, database, schema, localName);
        var result = identifier.ToString();

        Assert.That(result, Is.EqualTo(expectedOutput));
    }
}