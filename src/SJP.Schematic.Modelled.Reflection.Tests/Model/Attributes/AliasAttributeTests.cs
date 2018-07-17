using System;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class AliasAttributeTests
    {
        [Test]
        public static void Ctor_GivenNullAlias_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasAttribute(null));
        }

        [Test]
        public static void Ctor_GivenEmptyAlias_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasAttribute(string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceAlias_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasAttribute("   "));
        }

        [Test]
        public static void Alias_PropertyGet_MatchesCtorArgument()
        {
            const string aliasValue = "test";
            var aliasAttr = new AliasAttribute(aliasValue);

            Assert.AreEqual(aliasValue, aliasAttr.Alias);
        }
    }
}
