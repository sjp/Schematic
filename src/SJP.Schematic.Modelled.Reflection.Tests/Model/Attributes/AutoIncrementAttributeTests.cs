using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class AutoIncrementAttributeTests
    {
        [Test]
        public static void Ctor_UsingDefaultCtor_SetsInitialValueToOne()
        {
            var aliasAttr = new AutoIncrementAttribute();

            Assert.That(aliasAttr.InitialValue, Is.EqualTo(1));
        }

        [Test]
        public static void Ctor_UsingDefaultCtor_SetsIncrementToOne()
        {
            var aliasAttr = new AutoIncrementAttribute();

            Assert.That(aliasAttr.Increment, Is.EqualTo(1));
        }

        [Test]
        public static void InitialValue_PropertyGet_MatchesCtorArgument()
        {
            const int initialValue = 10;
            const int increment = 20;
            var aliasAttr = new AutoIncrementAttribute(initialValue, increment);

            Assert.That(aliasAttr.InitialValue, Is.EqualTo(initialValue));
        }

        [Test]
        public static void Increment_PropertyGet_MatchesCtorArgument()
        {
            const int initialValue = 10;
            const int increment = 20;
            var aliasAttr = new AutoIncrementAttribute(initialValue, increment);

            Assert.That(aliasAttr.Increment, Is.EqualTo(increment));
        }
    }
}
