using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class OnDeleteActionAttributeTests
    {
        [Test]
        public static void Ctor_GivenInvalidAction_ThrowsArgumentException()
        {
            const ReferentialAction action = (ReferentialAction)55;
            Assert.That(() => new OnDeleteActionAttribute(action), Throws.ArgumentException);
        }

        [Test]
        public static void Action_PropertyGet_MatchesCtorArgument()
        {
            const ReferentialAction action = ReferentialAction.SetDefault;
            var onUpdateActionAttr = new OnDeleteActionAttribute(action);

            Assert.That(onUpdateActionAttr.Action, Is.EqualTo(action));
        }
    }
}
