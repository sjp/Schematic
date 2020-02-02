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
            Assert.That(() => new IdentifierComparer(badStringComparison), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNullComparer_ThrowsArgumentNullException()
        {
            Assert.That(() => new IdentifierComparer(null), Throws.ArgumentNullException);
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
                Assert.That(comparer.Equals(null, null), Is.True);
                Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
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
                Assert.That(comparer.Equals(identifier, null), Is.False);
                Assert.That(comparer.Equals(null, identifier), Is.False);
                Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
            });
        }

        [Test]
        public static void GetHashCode_GivenNullArgument_ReturnsZero()
        {
            var comparer = new IdentifierComparer();
            Assert.That(comparer.GetHashCode(null), Is.Zero);
        }

        [Test]
        public static void GetHashCode_GivenNonNullArgument_ReturnsNonZeroValue()
        {
            var comparer = new IdentifierComparer();
            Assert.That(comparer.GetHashCode("test"), Is.Not.Zero);
        }

        [Test]
        [SetCulture("en-US")]
        public static void Equals_GivenCurrentCultureWithDifferentCasesOnly_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.CurrentCulture;

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        [SetCulture("en-US")]
        public static void Equals_GivenCurrentCultureIgnoreCaseWithDifferentCasesOnly_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.CurrentCultureIgnoreCase;

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Equals_GivenOrdinalWithDifferentCasesOnly_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.Ordinal;

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_GivenOrdinalIgnoreCaseWithDifferentCasesOnly_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("TEST");
            var comparer = IdentifierComparer.OrdinalIgnoreCase;

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Equals_WhenDefaultSchemaSetAndGivenIdentifierWithNullSchema_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Equals_WhenDefaultSchemaSetAndGivenIdentifiersWithDifferentSchema_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("other", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_WhenDefaultSchemaSetAndGivenIdentifiersWithDifferentSchemasSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultSchema: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_GivenIdentifiersWithDifferentSchemasSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_GivenIdentifiersWithSameSchemasSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("name", "test");
            var otherIdentifier = new Identifier("name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Equals_WhenDefaultDatabaseSetAndGivenIdentifierWithNullDatabase_ReturnsTrue()
        {
            var identifier = new Identifier("name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Equals_WhenDefaultDatabaseSetAndGivenIdentifiersWithDifferentDatabase_ReturnsFalse()
        {
            var identifier = new Identifier("name", "test");
            var otherIdentifier = new Identifier("other", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_WhenDefaultDatabaseSetAndGivenIdentifiersWithDifferentDatabasesSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultDatabase: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_GivenIdentifiersWithDifferentDatabasesSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_GivenIdentifiersWithSameDatabasesSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Equals_WhenServerSetAndGivenIdentifierWithNullServer_ReturnsTrue()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "name", defaultDatabase: "name", defaultSchema: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Equals_WhenServerSetAndGivenIdentifiersWithDifferentServer_ReturnsFalse()
        {
            var identifier = new Identifier("test");
            var otherIdentifier = new Identifier("other", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "name", defaultDatabase: "name", defaultSchema: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_WhenServerSetAndGivenIdentifiersWithDifferentServersSet_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, defaultServer: "name", defaultDatabase: "name", defaultSchema: "name");

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_GivenIdentifiersWithDifferentServersSetAndExplicitComparer_ReturnsFalse()
        {
            var identifier = new Identifier("other", "name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.False);
        }

        [Test]
        public static void Equals_GivenIdentifiersWithSameServersSetAndExplicitComparer_ReturnsTrue()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var otherIdentifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.That(comparer.Equals(identifier, otherIdentifier), Is.True);
        }

        [Test]
        public static void Compare_GivenSameIdentifier_ReturnsZero()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, identifier);

            Assert.That(compareResult, Is.Zero);
        }

        [Test]
        public static void Compare_GivenTwoNullIdentifiers_ReturnsZero()
        {
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(null, null);

            Assert.That(compareResult, Is.Zero);
        }

        [Test]
        public static void Compare_GivenLeftNullIdentifier_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(null, identifier);

            Assert.That(compareResult, Is.Not.Zero);
        }

        [Test]
        public static void Compare_GivenRightNullIdentifier_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "test");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, null);

            Assert.That(compareResult, Is.Not.Zero);
        }

        [Test]
        public static void Compare_GivenDifferentServer_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("z", "name", "name", "name");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.That(compareResult, Is.Not.Zero);
        }

        [Test]
        public static void Compare_GivenDifferentDatabase_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("name", "z", "name", "name");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.That(compareResult, Is.Not.Zero);
        }

        [Test]
        public static void Compare_GivenDifferentSchema_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("name", "name", "z", "name");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.That(compareResult, Is.Not.Zero);
        }

        [Test]
        public static void Compare_GivenDifferentLocalName_ReturnsNonZero()
        {
            var identifier = new Identifier("name", "name", "name", "name");
            var otherIdentifier = new Identifier("name", "name", "name", "z");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            var compareResult = comparer.Compare(identifier, otherIdentifier);

            Assert.That(compareResult, Is.Not.Zero);
        }
    }
}
