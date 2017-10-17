using System;
using NUnit.Framework;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    public class CollationAttributeTests
    {
        [Test]
        public void Ctor_GivenNullCollationName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CollationAttribute(null));
        }

        [Test]
        public void Ctor_GivenEmptyCollationName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CollationAttribute(string.Empty));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceCollationName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CollationAttribute("   "));
        }

        [Test]
        public void Ctor_GivenNullDialects_ThrowsArgumentNullException()
        {
            const string collationValue = "test";
            Assert.Throws<ArgumentNullException>(() => new CollationAttribute(collationValue, null));
        }

        [Test]
        public void Ctor_GivenEmptyDialects_ThrowsArgumentNullException()
        {
            const string collationValue = "test";
            Assert.Throws<ArgumentNullException>(() => new CollationAttribute(collationValue, Array.Empty<Type>()));
        }

        [Test]
        public void Ctor_GivenDialectsWithNullValue_ThrowsArgumentNullException()
        {
            const string collationValue = "test";
            var dialects = new Type[] { null };

            Assert.Throws<ArgumentNullException>(() => new CollationAttribute(collationValue, dialects));
        }

        [Test]
        public void CollationName_PropertyGet_MatchesCtorArgument()
        {
            const string collationValue = "test";
            var collectionAttr = new CollationAttribute(collationValue, typeof(FakeDialect));

            Assert.AreEqual(collationValue, collectionAttr.CollationName);
        }
    }
}
