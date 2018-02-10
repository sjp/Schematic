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
            const string name = "abc";
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
            const string localName = "abc";
            const string schema = "def";
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
            const string localName = "abc";
            const string schema = "def";
            const string database = "ghi";
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
            const string localName = "abc";
            const string schema = "def";
            const string database = "ghi";
            const string server = "jkl";
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
                Assert.Throws<ArgumentNullException>(() => new Identifier(null));
                Assert.Throws<ArgumentNullException>(() => new Identifier(string.Empty));
                Assert.Throws<ArgumentNullException>(() => new Identifier("   "));
            });
        }

        [Test]
        public void Equals_GivenEqualIdentifiers_ReturnsTrue()
        {
            const string name = "abc";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(name, name);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void Equals_GivenDifferentIdentifiers_ReturnsFalse()
        {
            const string name = "abc";
            const string otherName = "def";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(otherName, name);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void EqualsOp_GivenEqualIdentifiers_ReturnsTrue()
        {
            const string name = "abc";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(name, name);
            var isEqual = identifier == otherIdentifier;
            Assert.IsTrue(isEqual);
        }

        [Test]
        public void EqualsOp_GivenDifferentIdentifiers_ReturnsFalse()
        {
            const string name = "abc";
            const string otherName = "def";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(otherName, name);
            var isNotEqual = identifier != otherIdentifier;
            Assert.IsTrue(isNotEqual);
        }

        [Test]
        public void ObjectsEquals_GivenEqualIdentifiers_ReturnsTrue()
        {
            const string name = "abc";
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
            const string name = "abc";
            const string otherName = "def";
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
            var identifier = new Identifier("abc");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.NotNull(identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenFullyQualifiedArguments_CreatesFullyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("a", "b", "c", "d");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("a", identifier.Server);
                Assert.AreEqual("b", identifier.Database);
                Assert.AreEqual("c", identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsWithoutServer_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, "b", "c", "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.AreEqual("b", identifier.Database);
                Assert.AreEqual("c", identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabase_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, null, "c", "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.AreEqual("c", identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabaseAndSchema_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, null, "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsMissingServer_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("b", "c", "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.AreEqual("b", identifier.Database);
                Assert.AreEqual("c", identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsMissingServerAndWithoutDatabase_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, "c", "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.AreEqual("c", identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsMissingServerAndWithoutDatabaseAndSchema_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, null, "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsWithOnlyDatabaseAndLocalName_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("c", "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.AreEqual("c", identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsWithoutSchemaAndWithLocalName_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier(null, "d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsWithOnlyLocalName_CreatesCorrectlyQualifiedIdentifier()
        {
            var identifier = Identifier.CreateQualifiedIdentifier("d");

            Assert.Multiple(() =>
            {
                Assert.IsNull(identifier.Server);
                Assert.IsNull(identifier.Database);
                Assert.IsNull(identifier.Schema);
                Assert.AreEqual("d", identifier.LocalName);
            });
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenArgumentsWithoutServerAndDatabaseAndSchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Identifier.CreateQualifiedIdentifier(null, null, null, null));
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenAllArgumentsExceptDatabase_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Identifier.CreateQualifiedIdentifier("a", null, "c", "d"));
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenAllArgumentsExceptSchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Identifier.CreateQualifiedIdentifier("a", "b", null, "d"));
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenAllArgumentsExceptLocalName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Identifier.CreateQualifiedIdentifier("a", "b", "c", null));
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenAllArgumentsExceptServerAndSchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Identifier.CreateQualifiedIdentifier(null, "b", null, "d"));
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenOnlyDatabase_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Identifier.CreateQualifiedIdentifier(null, "b", null, null));
        }

        [Test]
        public void CreateQualifiedIdentifier_GivenOnlySchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Identifier.CreateQualifiedIdentifier(null, null, "c", null));
        }
    }
}
