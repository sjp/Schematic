using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal class IdentifierComparerTests
    {
        [Test]
        public void Ctor_GivenInvalidStringComparison_ThrowsArgumentException()
        {
            const StringComparison badStringComparison = (StringComparison)55;
            Assert.Throws<ArgumentException>(() => new IdentifierComparer(badStringComparison));
        }

        [Test]
        public void Ctor_GivenNullComparer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new IdentifierComparer(null));
        }

        [Test]
        public void Equals_GivenEqualValues_ReturnsTrue()
        {
            const string name = "abc";
            var comparer = new IdentifierComparer();

            var identifier = new Identifier(name);
            var otherIdentifier = new Identifier(name);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(comparer.Equals(null, null));
                Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
            });
        }

        [Test]
        public void Equals_GivenDifferentValues_ReturnsFalse()
        {
            const string name = "abc";
            const string otherName = "def";
            var comparer = new IdentifierComparer();

            var identifier = new Identifier(name);
            var otherIdentifier = new Identifier(otherName);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(comparer.Equals(identifier, null));
                Assert.IsFalse(comparer.Equals(null, identifier));
                Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
            });
        }

        [Test]
        public void GetHashCode_GivenNullArgument_ReturnsZero()
        {
            var comparer = new IdentifierComparer();
            Assert.Zero(comparer.GetHashCode(null));
        }

        [Test]
        public void GetHashCode_GivenNonNullArgument_ReturnsNonZeroValue()
        {
            var comparer = new IdentifierComparer();
            Assert.NotZero(comparer.GetHashCode("abc"));
        }

        [Test]
        //[SetCulture("en-US")] // uncomment when supported .NET Standard
        public void Equals_GivenCurrentCultureWithDifferentCasesOnly_ReturnsFalse()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.CurrentCulture;

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        //[SetCulture("en-US")] // uncomment when supported in .NET Standard
        public void Equals_GivenCurrentCultureIgnoreCaseWithDifferentCasesOnly_ReturnsTrue()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.CurrentCultureIgnoreCase;

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenOrdinalWithDifferentCasesOnly_ReturnsFalse()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.Ordinal;

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenOrdinalIgnoreCaseWithDifferentCasesOnly_ReturnsTrue()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.OrdinalIgnoreCase;

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenDefaultSchemaSetAndGivenIdentifierWithNullSchema_ReturnsTrue()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "dbo");

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenDefaultSchemaSetAndGivenIdentifiersWithDifferentSchema_ReturnsFalse()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("other", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenDefaultSchemaSetAndGivenIdentifiersWithDifferentSchemasSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenIdentifiersWithDifferentSchemasSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenIdentifiersWithSameSchemasSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenDefaultDatabaseSetAndGivenIdentifierWithNullDatabase_ReturnsTrue()
        {
            var identifier = new Identifier("dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "dbo");

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenDefaultDatabaseSetAndGivenIdentifiersWithDifferentDatabase_ReturnsFalse()
        {
            var identifier = new Identifier("dbo", "abc");
            var otherIdentifier = new Identifier("other", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenDefaultDatabaseSetAndGivenIdentifiersWithDifferentDatabasesSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenIdentifiersWithDifferentDatabasesSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenIdentifiersWithSameDatabasesSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("dbo", "dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenServerSetAndGivenIdentifierWithNullServer_ReturnsTrue()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "dbo", defaultDatabase: "dbo", defaultSchema: "dbo");

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenServerSetAndGivenIdentifiersWithDifferentServer_ReturnsFalse()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("other", "dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "dbo", defaultDatabase: "dbo", defaultSchema: "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_WhenServerSetAndGivenIdentifiersWithDifferentServersSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "dbo", "dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "dbo", defaultDatabase: "dbo", defaultSchema: "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenIdentifiersWithDifferentServersSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "dbo", "dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void Equals_GivenIdentifiersWithSameServersSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("dbo", "dbo", "dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "dbo", "dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }
    }
}
