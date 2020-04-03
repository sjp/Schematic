using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class AliasAttributeTests
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceAlias_ThrowsArgumentNullException(string alias)
        {
            Assert.That(() => new AliasAttribute(alias), Throws.ArgumentNullException);
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
