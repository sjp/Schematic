using System;
using NUnit.Framework;

namespace SJP.Schema.Core.Tests
{
    [TestFixture]
    public class IdentifierComparerTests
    {
        [Test]
        public void ComparerThrowsOnUndefinedComparison()
        {
            var badStringComparison = (StringComparison)55;
            Assert.Throws<InvalidOperationException>(() => new IdentifierComparer(badStringComparison));
        }

        [Test]
        public void ComparerThrowsOnNullStringComparer()
        {
            Assert.Throws<ArgumentNullException>(() => new IdentifierComparer(null));
        }

        [Test]
        public void SameNameEquivalent()
        {
            const string name = "abc";
            var comparer = new IdentifierComparer();

            var identifier = new Identifier(name);
            var otherIdentifier = new Identifier(name);

            Assert.IsTrue(comparer.Equals(null, null));
            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void IdentifiersNotEquivalent()
        {
            const string name = "abc";
            const string otherName = "def";
            var comparer = new IdentifierComparer();

            var identifier = new Identifier(name);
            var otherIdentifier = new Identifier(otherName);

            Assert.IsFalse(comparer.Equals(identifier, null));
            Assert.IsFalse(comparer.Equals(null, identifier));
            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void NullIdentifierReturnsZeroHashCode()
        {
            var comparer = new IdentifierComparer();
            Assert.Zero(comparer.GetHashCode(null));
        }

        [Test]
        public void NotNullIdentifierReturnsNonZeroHashCode()
        {
            var comparer = new IdentifierComparer();
            Assert.NotZero(comparer.GetHashCode("abc"));
        }

        [Test]
        //[SetCulture("en-US")] // uncomment when supported .NET Standard
        public void CurrentCultureIsCaseSensitive()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.CurrentCulture;

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        //[SetCulture("en-US")] // uncomment when supported in .NET Standard
        public void CurrentCultureIgnoreCaseIsNotCaseSensitive()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.CurrentCultureIgnoreCase;

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void OrdinalIsCaseSensitive()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.Ordinal;

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void OrdinalIgnoreCaseIsNotCaseSensitive()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("ABC");
            var comparer = IdentifierComparer.OrdinalIgnoreCase;

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void DefaultSchemaComparesEqualWithNullSchema()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, "dbo");

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void DefaultSchemaComparesNotEqualWithNullAndNonDefaultSchema()
        {
            var identifier = new Identifier("abc");
            var otherIdentifier = new Identifier("other", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void DefaultSchemaComparesNotEqualWithDefaultAndNonDefaultSchema()
        {
            var identifier = new Identifier("other", "abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture, "dbo");

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void SchemaComparesNotEqualWithDifferentSchema()
        {
            var identifier = new Identifier("other", "abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsFalse(comparer.Equals(identifier, otherIdentifier));
        }

        [Test]
        public void SchemaComparesEqualWithSameSchema()
        {
            var identifier = new Identifier("dbo", "abc");
            var otherIdentifier = new Identifier("dbo", "abc");
            var comparer = new IdentifierComparer(StringComparison.CurrentCulture);

            Assert.IsTrue(comparer.Equals(identifier, otherIdentifier));
        }
    }
}
