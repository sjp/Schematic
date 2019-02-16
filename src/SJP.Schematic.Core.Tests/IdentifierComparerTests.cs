using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class IdentifierComparerTests
    {
        [Test]
        public static void Ctor_GivenInvalidStringComparison_ThrowsArgumentException()
        {
            const StringComparison badStringComparison = (StringComparison)55;
            Assert.Throws<ArgumentException>(() => new IdentifierComparer(badStringComparison));
        }

        [Test]
        public static void Ctor_GivenNullComparer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new IdentifierComparer(null));
        }

        [Test]
        public static void Equals_GivenEqualValues_ReturnsTrue()
        {
            const string name = "test";
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
        public static void Equals_GivenDifferentValues_ReturnsFalse()
        {
            const string name = "test";
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
        public static void GetHashCode_GivenNullArgument_ReturnsZero()
        {
            var comparer = new IdentifierComparer();
            Assert.Zero(comparer.GetHashCode(null));
        }

        [Test]
        public static void GetHashCode_GivenNonNullArgument_ReturnsNonZeroValue()
        {
            var comparer = new IdentifierComparer();
            Assert.NotZero(comparer.GetHashCode("test"));
        }

        [Test]
        [SetCulture("en-US")]
        public static void Equals_GivenCurrentCultureWithDifferentCasesOnly_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.CurrentCulture;

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        [SetCulture("en-US")]
        public static void Equals_GivenCurrentCultureIgnoreCaseWithDifferentCasesOnly_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.CurrentCultureIgnoreCase;

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenOrdinalWithDifferentCasesOnly_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.Ordinal;

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenOrdinalIgnoreCaseWithDifferentCasesOnly_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.OrdinalIgnoreCase;

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenDefaultSchemaSetAndGivenIdentifierWithNullSchema_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "name");

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenDefaultSchemaSetAndGivenIdentifiersWithDifferentSchema_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("other", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "name");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenDefaultSchemaSetAndGivenIdentifiersWithDifferentSchemasSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "name");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenIdentifiersWithDifferentSchemasSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenIdentifiersWithSameSchemasSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("name", "test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenDefaultDatabaseSetAndGivenIdentifierWithNullDatabase_ReturnsTrue()
        {
            var identifier = new Identifier("name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "name");

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenDefaultDatabaseSetAndGivenIdentifiersWithDifferentDatabase_ReturnsFalse()
        {
            var identifier = new Identifier("name", "test");
            var otherIdentifier = new Identifier("other", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "name");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenDefaultDatabaseSetAndGivenIdentifiersWithDifferentDatabasesSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "name");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenIdentifiersWithDifferentDatabasesSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenIdentifiersWithSameDatabasesSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenServerSetAndGivenIdentifierWithNullServer_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "name", defaultDatabase: "name", defaultSchema: "name");

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenServerSetAndGivenIdentifiersWithDifferentServer_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("other", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "name", defaultDatabase: "name", defaultSchema: "name");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_WhenServerSetAndGivenIdentifiersWithDifferentServersSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "name", defaultDatabase: "name", defaultSchema: "name");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenIdentifiersWithDifferentServersSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Equals_GivenIdentifiersWithSameServersSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public static void Compare_GivenSameIdentifier_ReturnsZero()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, identifier);

            Assert.Zero(compareResult);
        }

        [Test]
        public static void Compare_GivenTwoNullIdentifiers_ReturnsZero()
        {
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(null, null);

            Assert.Zero(compareResult);
        }

        [Test]
        public static void Compare_GivenLeftNullIdentifier_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(null, identifier);

            Assert.NotZero(compareResult);
        }

        [Test]
        public static void Compare_GivenRightNullIdentifier_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, null);

            Assert.NotZero(compareResult);
        }

        [Test]
        public static void Compare_GivenDifferentServer_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("z", "name", "name", "name");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.NotZero(compareResult);
        }

        [Test]
        public static void Compare_GivenDifferentDatabase_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("name", "z", "name", "name");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.NotZero(compareResult);
        }

        [Test]
        public static void Compare_GivenDifferentSchema_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("name", "name", "z", "name");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.NotZero(compareResult);
        }

        [Test]
        public static void Compare_GivenDifferentLocalName_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("name", "name", "name", "z");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.NotZero(compareResult);
        }
    }
}
