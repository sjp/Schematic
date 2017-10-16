using System;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    public class AliasAttributeTests
    {
        [Test]
        public void Ctor_GivenNullAlias_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasAttribute(null));
        }

        [Test]
        public void Ctor_GivenEmptyAlias_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasAttribute(string.Empty));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceAlias_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasAttribute("   "));
        }

        [Test]
        public void Alias_PropertyGet_MatchesCtorArgument()
        {
            const string aliasValue = "test";
            var aliasAttr = new AliasAttribute(aliasValue);

            Assert.AreEqual(aliasValue, aliasAttr.Alias);
        }
    }
}
