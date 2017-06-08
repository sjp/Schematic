using System;
using NUnit.Framework;

namespace SJP.Schema.Core.Tests
{
    [TestFixture]
    public class IdentifierTests
    {
        [Test]
        public void IdentifierThrowsOnNullOrWhiteSpaceLocalName()
        {
            Assert.Throws<ArgumentNullException>(() => new Identifier(null));
            Assert.Throws<ArgumentNullException>(() => new Identifier(string.Empty));
            Assert.Throws<ArgumentNullException>(() => new Identifier("   "));
        }

        [Test]
        public void SingleArgumentMatchesLocalName()
        {
            var name = "abc";
            var identifier = new Identifier(name);
            Assert.AreEqual(identifier.LocalName, name);
        }

        [Test]
        public void IdentifierThrowsOnNullOrWhiteSpaceLocalNameAndSchema()
        {
            Assert.Throws<ArgumentNullException>(() => new Identifier("a", null));
            Assert.Throws<ArgumentNullException>(() => new Identifier("a", string.Empty));
            Assert.Throws<ArgumentNullException>(() => new Identifier("a", "   "));

            Assert.Throws<ArgumentNullException>(() => new Identifier(null, "a"));
            Assert.Throws<ArgumentNullException>(() => new Identifier(string.Empty, "a"));
            Assert.Throws<ArgumentNullException>(() => new Identifier("   ", "a"));
        }

        [Test]
        public void TwoArgumentsMatchesNames()
        {
            var localName = "abc";
            var schema = "def";
            var identifier = new Identifier(schema, localName);
            Assert.AreEqual(identifier.LocalName, localName);
            Assert.AreEqual(identifier.Schema, schema);
        }

        [Test]
        public void IdentifierThrowsOnNullOrWhiteSpaceLocalNameSchemaOrDatabase()
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
        }

        [Test]
        public void ThreeArgumentsMatchesNames()
        {
            var localName = "abc";
            var schema = "def";
            var database = "ghi";
            var identifier = new Identifier(database, schema, localName);
            Assert.AreEqual(identifier.LocalName, localName);
            Assert.AreEqual(identifier.Schema, schema);
            Assert.AreEqual(identifier.Database, database);
        }

        [Test]
        public void IdentifierThrowsOnNullOrWhiteSpaceLocalNameSchemaDatabaseOrServer()
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
        }

        [Test]
        public void FourArgumentsMatchesNames()
        {
            var localName = "abc";
            var schema = "def";
            var database = "ghi";
            var server = "jkl";
            var identifier = new Identifier(server, database, schema, localName);
            Assert.AreEqual(identifier.LocalName, localName);
            Assert.AreEqual(identifier.Schema, schema);
            Assert.AreEqual(identifier.Database, database);
            Assert.AreEqual(identifier.Server, server);
        }

        [Test]
        public void LocalIdentifierThrowsOnNullOrWhiteSpaceIdentifier()
        {
            Assert.Throws<ArgumentNullException>(() => new LocalIdentifier(null));
            Assert.Throws<ArgumentNullException>(() => new LocalIdentifier(string.Empty));
            Assert.Throws<ArgumentNullException>(() => new LocalIdentifier("   "));
        }

        [Test]
        public void IdentifierEqualsAnotherIdentifier()
        {
            var name = "abc";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(name, name);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void IdentifierNotEqualsAnotherIdentifier()
        {
            var name = "abc";
            var otherName = "def";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(otherName, name);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void IdentifierEqualsAnotherIdentifierViaOperator()
        {
            var name = "abc";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(name, name);
            var isEqual = identifier == otherIdentifier;
            Assert.IsTrue(isEqual);
        }

        [Test]
        public void IdentifierNotEqualsAnotherIdentifierViaOperator()
        {
            var name = "abc";
            var otherName = "def";
            var identifier = new Identifier(name, name);
            var otherIdentifier = new Identifier(otherName, name);
            var isNotEqual = identifier != otherIdentifier;
            Assert.IsTrue(isNotEqual);
        }

        [Test]
        public void IdentifierObjectEquals()
        {
            var name = "abc";
            object identifier = new Identifier(name, name);
            object otherIdentifier = new Identifier(name, name);

            Assert.AreEqual(identifier, identifier);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void IdentifierObjectNotEquals()
        {
            var name = "abc";
            var otherName = "def";
            object identifier = new Identifier(name, name);
            object otherIdentifier = new Identifier(otherName, name);

            Assert.AreNotEqual(identifier, null);
            Assert.AreNotEqual(null, identifier);
            Assert.AreNotEqual(identifier, 1);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void LocalIdentifierOnlyContainsLocalName()
        {
            var identifier = new LocalIdentifier("abc");
            Assert.IsNull(identifier.Server);
            Assert.IsNull(identifier.Database);
            Assert.IsNull(identifier.Schema);
            Assert.NotNull(identifier.LocalName);
        }

        [Test]
        public void LocalIdentifierEqualsLocalName()
        {
            var name = "abc";
            var identifier = new LocalIdentifier(name);
            Assert.AreEqual(identifier.LocalName, name);
        }

        [Test]
        public void LocalIdentifierNotEqualsLocalName()
        {
            var name = "abc";
            var identifier = new LocalIdentifier("def");
            Assert.AreNotEqual(identifier.LocalName, name);
        }

        [Test]
        public void LocalIdentifierEqualsAnotherLocalIdentifier()
        {
            var name = "abc";
            var identifier = new LocalIdentifier(name);
            var otherIdentifier = new LocalIdentifier(name);
            Assert.AreEqual(identifier, identifier);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void LocalIdentifierNotEqualsAnotherLocalIdentifier()
        {
            var identifier = new LocalIdentifier("abc");
            var otherIdentifier = new LocalIdentifier("def");
            Assert.AreNotEqual(null, identifier);
            Assert.AreNotEqual(identifier, null);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void LocalIdentifierHashEqualsAnotherLocalIdentifier()
        {
            var name = "abc";
            var identifier = new LocalIdentifier(name);
            var otherIdentifier = new LocalIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void LocalIdentifierHashNotEqualsAnotherLocalIdentifier()
        {
            var identifier = new LocalIdentifier("abc");
            var otherIdentifier = new LocalIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void SchemaIdentifierThrowsOnNullOrWhiteSpaceIdentifier()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaIdentifier(null));
            Assert.Throws<ArgumentNullException>(() => new SchemaIdentifier(string.Empty));
            Assert.Throws<ArgumentNullException>(() => new SchemaIdentifier("   "));
        }

        [Test]
        public void SchemaIdentifierOnlyContainsSchemaName()
        {
            var identifier = new SchemaIdentifier("abc");
            Assert.IsNull(identifier.Server);
            Assert.IsNull(identifier.Database);
            Assert.NotNull(identifier.Schema);
            Assert.IsNull(identifier.LocalName);
        }

        [Test]
        public void SchemaIdentifierEqualsSchemaName()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier(name);
            Assert.AreEqual(identifier.Schema, name);
        }

        [Test]
        public void SchemaIdentifierNotEqualsSchema()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier("def");
            Assert.AreNotEqual(identifier.Schema, name);
        }

        [Test]
        public void SchemaIdentifierEqualsAnotherSchemaIdentifier()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier(name);
            var otherIdentifier = new SchemaIdentifier(name);
            Assert.AreEqual(identifier, identifier);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void SchemaIdentifierNotEqualsAnotherSchemaIdentifier()
        {
            var identifier = new SchemaIdentifier("abc");
            var otherIdentifier = new SchemaIdentifier("def");
            Assert.AreNotEqual(null, identifier);
            Assert.AreNotEqual(identifier, null);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void SchemaIdentifierHashEqualsAnotherSchemaIdentifier()
        {
            var name = "abc";
            var identifier = new SchemaIdentifier(name);
            var otherIdentifier = new SchemaIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void SchemaIdentifierHashNotEqualsAnotherSchenaIdentifier()
        {
            var identifier = new SchemaIdentifier("abc");
            var otherIdentifier = new SchemaIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void DatabaseIdentifierThrowsOnNullOrWhiteSpaceIdentifier()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseIdentifier(null));
            Assert.Throws<ArgumentNullException>(() => new DatabaseIdentifier(string.Empty));
            Assert.Throws<ArgumentNullException>(() => new DatabaseIdentifier("   "));
        }

        [Test]
        public void DatabaseIdentifierOnlyContainsDatabaseName()
        {
            var identifier = new DatabaseIdentifier("abc");
            Assert.IsNull(identifier.Server);
            Assert.NotNull(identifier.Database);
            Assert.IsNull(identifier.Schema);
            Assert.IsNull(identifier.LocalName);
        }

        [Test]
        public void DatabaseIdentifierEqualsDatabaseName()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier(name);
            Assert.AreEqual(identifier.Database, name);
        }

        [Test]
        public void DatabaseIdentifierNotEqualsDatabaseName()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier("def");
            Assert.AreNotEqual(identifier.Database, name);
        }

        [Test]
        public void DatabaseIdentifierEqualsAnotherDatabaseIdentifier()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier(name);
            var otherIdentifier = new DatabaseIdentifier(name);
            Assert.AreEqual(identifier, identifier);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void DatabaseIdentifierNotEqualsAnotherDatabaseIdentifier()
        {
            var identifier = new DatabaseIdentifier("abc");
            var otherIdentifier = new DatabaseIdentifier("def");
            Assert.AreNotEqual(null, identifier);
            Assert.AreNotEqual(identifier, null);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void DatabaseIdentifierHashEqualsAnotherDatabaseIdentifier()
        {
            var name = "abc";
            var identifier = new DatabaseIdentifier(name);
            var otherIdentifier = new DatabaseIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void DatabaseIdentifierHashNotEqualsAnotherDatabaseIdentifier()
        {
            var identifier = new DatabaseIdentifier("abc");
            var otherIdentifier = new DatabaseIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void ServerIdentifierThrowsOnNullOrWhiteSpaceIdentifier()
        {
            Assert.Throws<ArgumentNullException>(() => new ServerIdentifier(null));
            Assert.Throws<ArgumentNullException>(() => new ServerIdentifier(string.Empty));
            Assert.Throws<ArgumentNullException>(() => new ServerIdentifier("   "));
        }

        [Test]
        public void ServerIdentifierOnlyContainsServerName()
        {
            var identifier = new ServerIdentifier("abc");
            Assert.NotNull(identifier.Server);
            Assert.IsNull(identifier.Database);
            Assert.IsNull(identifier.Schema);
            Assert.IsNull(identifier.LocalName);
        }

        [Test]
        public void ServerIdentifierEqualsServerName()
        {
            var name = "abc";
            var identifier = new ServerIdentifier(name);
            Assert.AreEqual(identifier.Server, name);
        }

        [Test]
        public void ServerIdentifierNotEqualsServerName()
        {
            var name = "abc";
            var identifier = new ServerIdentifier("def");
            Assert.AreNotEqual(identifier.Server, name);
        }

        [Test]
        public void ServerIdentifierEqualsAnotherServerIdentifier()
        {
            var name = "abc";
            var identifier = new ServerIdentifier(name);
            var otherIdentifier = new ServerIdentifier(name);
            Assert.AreEqual(identifier, identifier);
            Assert.AreEqual(identifier, otherIdentifier);
        }

        [Test]
        public void ServerIdentifierNotEqualsAnotherServerIdentifier()
        {
            var identifier = new ServerIdentifier("abc");
            var otherIdentifier = new ServerIdentifier("def");
            Assert.AreNotEqual(null, identifier);
            Assert.AreNotEqual(identifier, null);
            Assert.AreNotEqual(identifier, otherIdentifier);
        }

        [Test]
        public void ServerIdentifierHashEqualsAnotherServerIdentifier()
        {
            var name = "abc";
            var identifier = new ServerIdentifier(name);
            var otherIdentifier = new ServerIdentifier(name);
            Assert.AreEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }

        [Test]
        public void ServerIdentifierHashNotEqualsAnotherServerIdentifier()
        {
            var identifier = new ServerIdentifier("abc");
            var otherIdentifier = new ServerIdentifier("def");
            Assert.AreNotEqual(identifier.GetHashCode(), otherIdentifier.GetHashCode());
        }
    }
}
