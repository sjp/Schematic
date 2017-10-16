using System;
using NUnit.Framework;

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
        public void CollationName_PropertyGet_MatchesCtorArgument()
        {
            const string collationValue = "test";
            var collectionAttr = new CollationAttribute(collationValue);

            Assert.AreEqual(collationValue, collectionAttr.CollationName);
        }
    }
}
