using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    public class AutoIncrementAttributeTests
    {
        [Test]
        public void Ctor_UsingDefaultCtor_SetsInitialValueToOne()
        {
            var aliasAttr = new AutoIncrementAttribute();

            Assert.AreEqual(1, aliasAttr.InitialValue);
        }

        [Test]
        public void Ctor_UsingDefaultCtor_SetsIncrementToOne()
        {
            var aliasAttr = new AutoIncrementAttribute();

            Assert.AreEqual(1, aliasAttr.Increment);
        }

        [Test]
        public void InitialValue_PropertyGet_MatchesCtorArgument()
        {
            const int initialValue = 10;
            const int increment = 20;
            var aliasAttr = new AutoIncrementAttribute(initialValue, increment);

            Assert.AreEqual(initialValue, aliasAttr.InitialValue);
        }

        [Test]
        public void Increment_PropertyGet_MatchesCtorArgument()
        {
            const int initialValue = 10;
            const int increment = 20;
            var aliasAttr = new AutoIncrementAttribute(initialValue, increment);

            Assert.AreEqual(increment, aliasAttr.Increment);
        }
    }
}
