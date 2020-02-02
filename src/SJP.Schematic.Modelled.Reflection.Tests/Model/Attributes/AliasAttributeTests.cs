using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class AliasAttributeTests
    {
        [Test]
        public static void Ctor_GivenNullAlias_ThrowsArgumentNullException()
        {
            Assert.That(() => new AliasAttribute(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyAlias_ThrowsArgumentNullException()
        {
            Assert.That(() => new AliasAttribute(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceAlias_ThrowsArgumentNullException()
        {
            Assert.That(() => new AliasAttribute("   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void Alias_PropertyGet_MatchesCtorArgument()
        {
            const string aliasValue = "test";
            var aliasAttr = new AliasAttribute(aliasValue);

            Assert.That(aliasAttr.Alias, Is.EqualTo(aliasValue));
        }
    }
}
