using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class CollationAttributeTests
    {
        [Test]
        public static void Ctor_GivenNullCollationName_ThrowsArgumentNullException()
        {
            Assert.That(() => new CollationAttribute(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyCollationName_ThrowsArgumentNullException()
        {
            Assert.That(() => new CollationAttribute(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceCollationName_ThrowsArgumentNullException()
        {
            Assert.That(() => new CollationAttribute("   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDialects_ThrowsArgumentNullException()
        {
            const string collationValue = "test";
            Assert.That(() => new CollationAttribute(collationValue, (Type[])null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyDialects_ThrowsArgumentNullException()
        {
            const string collationValue = "test";
            Assert.That(() => new CollationAttribute(collationValue, Array.Empty<Type>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenDialectsWithNullValue_ThrowsArgumentNullException()
        {
            const string collationValue = "test";
            var dialects = new Type[] { null };

            Assert.That(() => new CollationAttribute(collationValue, dialects), Throws.ArgumentNullException);
        }

        [Test]
        public static void CollationName_PropertyGet_MatchesCtorArgument()
        {
            const string collationValue = "test";
            var collectionAttr = new CollationAttribute(collationValue, typeof(FakeDialect));
            var expected = new Identifier(collationValue);

            Assert.That(collectionAttr.CollationName, Is.EqualTo(expected));
        }
    }
}
