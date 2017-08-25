using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    public class IdentifierTests
    {
        [Test]
        public void Ctor_GivenNullOrWhiteSpaceLocalName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new Identifier(null));
                Assert.Throws<ArgumentNullException>(() => new Identifier(string.Empty));
                Assert.Throws<ArgumentNullException>(() => new Identifier("   "));
            });
        }

        [Test]
        public void LocalName_PropertyGet_EqualsCtorArgument()
        {
            var name = "abc";
            var identifier = new Identifier(name);
            Assert.AreEqual(identifier.LocalName, name);
        }

        [Test]
        public void Ctor_GivenNullWhiteSpaceSchemaAndLocalNames_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", null));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", string.Empty));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "   "));

                Assert.Throws<ArgumentNullException>(() => new Identifier(null, "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier(string.Empty, "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("   ", "a"));
            });
        }

        [Test]
        public void SchemaAndLocalName_PropertyGets_MatchCtorArguments()
        {
            var localName = "abc";
            var schema = "def";
            var identifier = new Identifier(schema, localName);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier.LocalName, localName);
                Assert.AreEqual(identifier.Schema, schema);
            });
        }

        [Test]
        public void Ctor_GivenNullWhiteSpaceDatabaseAndSchemaAndLocalNames_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", null));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", string.Empty));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", "   "));

                Assert.Throws<ArgumentNullException>(() => new Identifier("a", null, "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", string.Empty, "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "   ", "a"));

                Assert.Throws<ArgumentNullException>(() => new Identifier(null, "a", "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier(string.Empty, "a", "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("   ", "a", "a"));
            });
        }

        [Test]
        public void DatabaseAndSchemaAndLocalName_PropertyGets_MatchCtorArguments()
        {
            var localName = "abc";
            var schema = "def";
            var database = "ghi";
            var identifier = new Identifier(database, schema, localName);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier.LocalName, localName);
                Assert.AreEqual(identifier.Schema, schema);
                Assert.AreEqual(identifier.Database, database);
            });
        }

        [Test]
        public void Ctor_GivenNullWhiteSpaceServerAndDatabaseAndSchemaAndLocalNames_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", "a", null));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", "a", string.Empty));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", "a", "   "));

                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", null, "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", string.Empty, "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "a", "   ", "a"));

                Assert.Throws<ArgumentNullException>(() => new Identifier("a", null, "a", "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", string.Empty, "a", "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("a", "   ", "a", "a"));

                Assert.Throws<ArgumentNullException>(() => new Identifier(null, "a", "a", "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier(string.Empty, "a", "a", "a"));
                Assert.Throws<ArgumentNullException>(() => new Identifier("   ", "a", "a", "a"));
            });
        }

        [Test]
        public void ServerAndDatabaseAndSchemaAndLocalName_PropertyGets_MatchCtorArguments()
        {
            var localName = "abc";
            var schema = "def";
            var database = "ghi";
            var server = "jkl";
            var identifier = new Identifier(server, database, schema, localName);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier.LocalName, localName);
                Assert.AreEqual(identifier.Schema, schema);
                Assert.AreEqual(identifier.Database, database);
                Assert.AreEqual(identifier.Server, server);
            });
        }

        [Test]
        public void LocalIdentifierCtor_GivenNullWhiteSpaceLocalName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new LocalIdentifier(null));
                Assert.Throws<ArgumentNullException>(() => new LocalIdentifier(string.Empty));
                Assert.Throws<ArgumentNullException>(() => new LocalIdentifier("   "));
            });
        }

        [Test]
        public void Equals_GivenEqualIdentifiers_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(name, name);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void Equals_GivenDifferentIdentifiers_ReturnsFalse()
        {
            var name = "abc";
            var otherName = "def";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(otherName, name);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void EqualsOp_GivenEqualIdentifiers_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(name, name);
            var isEqual = identifier == otherIdentifier;
            Assert.IsTrue(isEqual);
        }

        [Test]
        public void EqualsOp_GivenDifferentIdentifiers_ReturnsFalse()
        {
            var name = "abc";
            var otherName = "def";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(otherName, name);
            var isNotEqual = identifier != otherIdentifier;
            Assert.IsTrue(isNotEqual);
        }

        [Test]
        public void ObjectsEquals_GivenEqualIdentifiers_ReturnsTrue()
        {
            var name = "abc";
            object identifier = new Identifier(name, name);
            object otherIdentifier = new Identifier(name, name);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier, identifier);
                Assert.AreEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void ObjectsEquals_GivenDifferentObjects_ReturnsFalse()
        {
            var name = "abc";
            var otherName = "def";
            object identifier = new Identifier(name, name);
            object otherIdentifier = new Identifier(otherName, name);

            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(identifier, null);
                Assert.AreNotEqual(null, identifier);
                Assert.AreNotEqual(identifier, 1);
                Assert.AreNotEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void Identifier_WhenOnlyLocalNameProvided_OnlyHasLocalNamePropertySet()
        {
            var identifier = new LocalIdentifier("abc");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.NotNull(identifier.LocalName);
            });
        }

        [Test]
        public void LocalIdentifierEquals_GivenSameStringIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new LocalIdentifier(name);
            Assert.AreEqual(identifier.LocalName, name);
        }

        [Test]
        public void LocalIdentifierEquals_GivenDifferentStringIdentifier_ReturnsFalse()
        {
            var name = "abc";
            var identifier = new LocalIdentifier("def");
            Assert.AreNotEqual(identifier.LocalName, name);
        }

        [Test]
        public void LocalIdentifierEquals_GivenEqualLocalIdentifiers_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new LocalIdentifier(name);
            var otherIdentifier = new LocalIdentifier(name);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier, identifier);
                Assert.AreEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void LocalIdentifierEquals_GivenDifferentLocalIdentifiers_ReturnsFalse()
        {
            var identifier = new LocalIdentifier("abc");
            var otherIdentifier = new LocalIdentifier("def");

            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(null, identifier);
                Assert.AreNotEqual(identifier, null);
                Assert.AreNotEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void LocalIdentifierGetHashCode_GivenEqualLocalIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new LocalIdentifier(name);
            var otherIdentifier = new LocalIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void LocalIdentifierGetHashCode_GivenDifferentLocalIdentifier_ReturnsFalse()
        {
            var identifier = new LocalIdentifier("abc");
            var otherIdentifier = new LocalIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void SchemaIdentifierCtor_GivenNullWhiteSpaceLocalName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new SchemaIdentifier(null));
                Assert.Throws<ArgumentNullException>(() => new SchemaIdentifier(string.Empty));
                Assert.Throws<ArgumentNullException>(() => new SchemaIdentifier("   "));
            });
        }

        [Test]
        public void Identifier_WhenOnlySchemaProvided_OnlyHasSchemaPropertySet()
        {
            var identifier = new SchemaIdentifier("abc");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.NotNull(identifier.Schema);
                Assert.IsNull(identifier.LocalName);
            });
        }

        [Test]
        public void SchemaIdentifierEquals_GivenSameStringIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier(name);
            Assert.AreEqual(identifier.Schema, name);
        }

        [Test]
        public void SchemaIdentifierEquals_GivenDifferentStringIdentifier_ReturnsFalse()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier("def");
            Assert.AreNotEqual(identifier.Schema, name);
        }

        [Test]
        public void SchemaIdentifierEquals_GivenEqualSchemaIdentifiers_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier(name);
            var otherIdentifier = new SchemaIdentifier(name);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier, identifier);
                Assert.AreEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void SchemaIdentifierEquals_GivenDifferentSchemaIdentifiers_ReturnsFalse()
        {
            var identifier = new SchemaIdentifier("abc");
            var otherIdentifier = new SchemaIdentifier("def");

            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(null, identifier);
                Assert.AreNotEqual(identifier, null);
                Assert.AreNotEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void SchemaIdentifierGetHashCode_GivenEqualSchemaIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier(name);
            var otherIdentifier = new SchemaIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void SchemaIdentifierGetHashCode_GivenDifferentSchemaIdentifier_ReturnsFalse()
        {
            var identifier = new SchemaIdentifier("abc");
            var otherIdentifier = new SchemaIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void DatabaseIdentifierCtor_GivenNullWhiteSpaceLocalName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new DatabaseIdentifier(null));
                Assert.Throws<ArgumentNullException>(() => new DatabaseIdentifier(string.Empty));
                Assert.Throws<ArgumentNullException>(() => new DatabaseIdentifier("   "));
            });
        }

        [Test]
        public void Identifier_WhenOnlyDatabaseProvided_OnlyHasDatabasePropertySet()
        {
            var identifier = new DatabaseIdentifier("abc");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.NotNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.IsNull(identifier.LocalName);
            });
        }

        [Test]
        public void DatabaseIdentifierEquals_GivenSameStringIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier(name);
            Assert.AreEqual(identifier.Database, name);
        }

        [Test]
        public void DatabaseIdentifierEquals_GivenDifferentStringIdentifier_ReturnsFalse()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier("def");
            Assert.AreNotEqual(identifier.Database, name);
        }

        [Test]
        public void DatabaseIdentifierEquals_GivenEqualDatabaseIdentifiers_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier(name);
            var otherIdentifier = new DatabaseIdentifier(name);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier, identifier);
                Assert.AreEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void DatabaseIdentifierEquals_GivenDifferentDatabaseIdentifiers_ReturnsFalse()
        {
            var identifier = new DatabaseIdentifier("abc");
            var otherIdentifier = new DatabaseIdentifier("def");

            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(null, identifier);
                Assert.AreNotEqual(identifier, null);
                Assert.AreNotEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void DatabaseIdentifierGetHashCode_GivenEqualDatabaseIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier(name);
            var otherIdentifier = new DatabaseIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void DatabaseIdentifierGetHashCode_GivenDifferentDatabaseIdentifier_ReturnsFalse()
        {
            var identifier = new DatabaseIdentifier("abc");
            var otherIdentifier = new DatabaseIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void ServerIdentifierCtor_GivenNullWhiteSpaceLocalName_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new ServerIdentifier(null));
                Assert.Throws<ArgumentNullException>(() => new ServerIdentifier(string.Empty));
                Assert.Throws<ArgumentNullException>(() => new ServerIdentifier("   "));
            });
        }

        [Test]
        public void Identifier_WhenOnlyServerProvided_OnlyHasServerPropertySet()
        {
            var identifier = new ServerIdentifier("abc");

            Assert.Multiple(() =>
            {
                Assert.NotNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.IsNull(identifier.LocalName);
            });
        }

        [Test]
        public void ServerIdentifierEquals_GivenSameStringIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new ServerIdentifier(name);
            Assert.AreEqual(identifier.Server, name);
        }

        [Test]
        public void ServerIdentifierEquals_GivenDifferentStringIdentifier_ReturnsFalse()
        {
            var name = "abc";
            var identifier = new ServerIdentifier("def");
            Assert.AreNotEqual(identifier.Server, name);
        }

        [Test]
        public void ServerIdentifierEquals_GivenEqualServerIdentifiers_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new ServerIdentifier(name);
            var otherIdentifier = new ServerIdentifier(name);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(identifier, identifier);
                Assert.AreEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void ServerIdentifierEquals_GivenDifferentServerIdentifiers_ReturnsFalse()
        {
            var identifier = new ServerIdentifier("abc");
            var otherIdentifier = new ServerIdentifier("def");

            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(null, identifier);
                Assert.AreNotEqual(identifier, null);
                Assert.AreNotEqual(identifier, otherIdentifier);
            });
        }

        [Test]
        public void ServerIdentifierGetHashCode_GivenEqualServerIdentifier_ReturnsTrue()
        {
            var name = "abc";
            var identifier = new ServerIdentifier(name);
            var otherIdentifier = new ServerIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void ServerIdentifierGetHashCode_GivenDifferentServerIdentifier_ReturnsFalse()
        {
            var identifier = new ServerIdentifier("abc");
            var otherIdentifier = new ServerIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }
    }
}
