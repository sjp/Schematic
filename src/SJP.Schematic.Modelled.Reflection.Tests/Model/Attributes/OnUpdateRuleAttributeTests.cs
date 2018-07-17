using System;
using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class OnUpdateRuleAttributeTests
    {
        [Test]
        public static void Ctor_GivenInvalidRule_ThrowsArgumentException()
        {
            const Rule rule = (Rule)55;
            Assert.Throws<ArgumentException>(() => new OnUpdateRuleAttribute(rule));
        }

        [Test]
        public static void Rule_PropertyGet_MatchesCtorArgument()
        {
            const Rule rule = Rule.SetDefault;
            var onUpdateRuleAttr = new OnUpdateRuleAttribute(rule);

            Assert.AreEqual(rule, onUpdateRuleAttr.Rule);
        }
    }
}
