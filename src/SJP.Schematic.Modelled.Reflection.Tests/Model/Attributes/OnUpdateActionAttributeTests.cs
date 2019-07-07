using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class OnUpdateActionAttributeTests
    {
        [Test]
        public static void Ctor_GivenInvalidAction_ThrowsArgumentException()
        {
            const ReferentialAction action = (ReferentialAction)55;
            Assert.Throws<ArgumentException>(() => new OnUpdateActionAttribute(action));
        }

        [Test]
        public static void Action_PropertyGet_MatchesCtorArgument()
        {
            const ReferentialAction action = ReferentialAction.SetDefault;
            var onUpdateActionAttr = new OnUpdateActionAttribute(action);

            Assert.AreEqual(action, onUpdateActionAttr.Action);
        }
    }
}
