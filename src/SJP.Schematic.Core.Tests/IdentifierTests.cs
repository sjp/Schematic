using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class IdentifierTests
    {
        [Test]
        public static void Ctor_GivenNullOrWhiteSpaceLocalName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => new Identifier(null), Throws.ArgumentNullException);
                Assert.That(() => new Identifier(string.Empty), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("   "), Throws.ArgumentNullException);
            });
        }

        [Test]
        public static void LocalName_PropertyGet_EqualsCtorArgument()
        {
            const string name = "test";
            var identifier = new Identifier(name);
            Assert.That(identifier.LocalName, Is.EqualTo(name));
        }

        [Test]
        public static void Ctor_GivenNullWhiteSpaceSchemaAndLocalNames_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => new Identifier("a", null), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", string.Empty), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "   "), Throws.ArgumentNullException);

                Assert.That(() => new Identifier(null, "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier(string.Empty, "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("   ", "a"), Throws.ArgumentNullException);
            });
        }

        [Test]
        public static void SchemaAndLocalName_PropertyGets_MatchCtorArguments()
        {
            const string localName = "local";
            const string schema = "schema";
            var identifier = new Identifier(schema, localName);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.LocalName, Is.EqualTo(localName));
                Assert.That(identifier.Schema, Is.EqualTo(schema));
            });
        }

        [Test]
        public static void Ctor_GivenNullWhiteSpaceDatabaseAndSchemaAndLocalNames_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => new Identifier("a", "a", null), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "a", string.Empty), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "a", "   "), Throws.ArgumentNullException);

                Assert.That(() => new Identifier("a", null, "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", string.Empty, "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "   ", "a"), Throws.ArgumentNullException);

                Assert.That(() => new Identifier(null, "a", "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier(string.Empty, "a", "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("   ", "a", "a"), Throws.ArgumentNullException);
            });
        }

        [Test]
        public static void DatabaseAndSchemaAndLocalName_PropertyGets_MatchCtorArguments()
        {
            const string localName = "local";
            const string schema = "schema";
            const string database = "database";
            var identifier = new Identifier(database, schema, localName);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.LocalName, Is.EqualTo(localName));
                Assert.That(identifier.Schema, Is.EqualTo(schema));
                Assert.That(identifier.Database, Is.EqualTo(database));
            });
        }

        [Test]
        public static void Ctor_GivenNullWhiteSpaceServerAndDatabaseAndSchemaAndLocalNames_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => new Identifier("a", "a", "a", null), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "a", "a", string.Empty), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "a", "a", "   "), Throws.ArgumentNullException);

                Assert.That(() => new Identifier("a", "a", null, "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "a", string.Empty, "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "a", "   ", "a"), Throws.ArgumentNullException);

                Assert.That(() => new Identifier("a", null, "a", "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", string.Empty, "a", "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("a", "   ", "a", "a"), Throws.ArgumentNullException);

                Assert.That(() => new Identifier(null, "a", "a", "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier(string.Empty, "a", "a", "a"), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("   ", "a", "a", "a"), Throws.ArgumentNullException);
            });
        }

        [Test]
        public static void ServerAndDatabaseAndSchemaAndLocalName_PropertyGets_MatchCtorArguments()
        {
            const string localName = "local";
            const string schema = "schema";
            const string database = "database";
            const string server = "server";
            var identifier = new Identifier(server, database, schema, localName);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.LocalName, Is.EqualTo(localName));
                Assert.That(identifier.Schema, Is.EqualTo(schema));
                Assert.That(identifier.Database, Is.EqualTo(database));
                Assert.That(identifier.Server, Is.EqualTo(server));
            });
        }

        [Test]
        public static void LocalIdentifierCtor_GivenNullWhiteSpaceLocalName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => new Identifier(null), Throws.ArgumentNullException);
                Assert.That(() => new Identifier(string.Empty), Throws.ArgumentNullException);
                Assert.That(() => new Identifier("   "), Throws.ArgumentNullException);
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(identifier, Is.EqualTo(identifier));
                Assert.That(identifier, Is.EqualTo(otherIdentifier));
            });
        }

        [Test]
        public static void ObjectsEquals_GivenDifferentObjects_ReturnsFalse()
        {
            const string name = "test";
            const string otherName = "another";
            object identifier = new Identifier(name, name);
            object otherIdentifier = new Identifier(otherName, name);

            Assert.Multiple(() =>
            {
                Assert.That(identifier, Is.Not.EqualTo(null));
                Assert.That(null, Is.Not.EqualTo(identifier));
                Assert.That(identifier, Is.Not.EqualTo(1));
                Assert.That(identifier, Is.Not.EqualTo(otherIdentifier));
            });
        }

        [Test]
        public static void Identifier_WhenOnlyLocalNameProvided_OnlyHasLocalNamePropertySet()
        {
            var identifier = new Identifier("test");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.Null);
                Assert.That(identifier.LocalName, Is.Not.Null);
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenFullyQualifiedArguments_CreatesFullyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("a", "b", "c", "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.EqualTo("a"));
                Assert.That(identifier.Database, Is.EqualTo("b"));
                Assert.That(identifier.Schema, Is.EqualTo("c"));
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsWithoutServer_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, "b", "c", "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.EqualTo("b"));
                Assert.That(identifier.Schema, Is.EqualTo("c"));
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabase_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, null, "c", "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.EqualTo("c"));
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabaseAndSchema_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, null, null, "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.Null);
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsMissingServer_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("b", "c", "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.EqualTo("b"));
                Assert.That(identifier.Schema, Is.EqualTo("c"));
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsMissingServerAndWithoutDatabase_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, "c", "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.EqualTo("c"));
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsMissingServerAndWithoutDatabaseAndSchema_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, null, "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.Null);
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsWithOnlyDatabaseAndLocalName_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("c", "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.EqualTo("c"));
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsWithoutSchemaAndWithLocalName_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, "d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.Null);
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsWithOnlyLocalName_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("d");

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Server, Is.Null);
                Assert.That(identifier.Database, Is.Null);
                Assert.That(identifier.Schema, Is.Null);
                Assert.That(identifier.LocalName, Is.EqualTo("d"));
            });
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabaseAndSchema_ThrowsArgumentNullException()
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier(null, null, null, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenAllArgumentsExceptDatabase_ThrowsArgumentNullException()
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier("a", null, "c", "d"), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenAllArgumentsExceptSchema_ThrowsArgumentNullException()
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier("a", "b", null, "d"), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenAllArgumentsExceptLocalName_ThrowsArgumentNullException()
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier("a", "b", "c", null), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenAllArgumentsExceptServerAndSchema_ThrowsArgumentNullException()
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier(null, "b", null, "d"), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenOnlyDatabase_ThrowsArgumentNullException()
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier(null, "b", null, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void CreateQualifiedIdentifier_GivenOnlySchema_ThrowsArgumentNullException()
        {
            Assert.That(() => Identifier.CreateQualifiedIdentifier(null, null, "c", null), Throws.ArgumentNullException);
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
    }
}
