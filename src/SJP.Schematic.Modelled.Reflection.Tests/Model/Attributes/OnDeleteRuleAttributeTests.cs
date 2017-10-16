using System;
using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    public class OnDeleteRuleAttributeTests
    {
        [Test]
        public void Ctor_GivenInvalidRule_ThrowsArgumentException()
        {
            const Rule rule = (Rule)55;
            Assert.Throws<ArgumentException>(() => new OnDeleteRuleAttribute(rule));
        }

        [Test]
        public void Rule_PropertyGet_MatchesCtorArgument()
        {
            const Rule rule = Rule.SetDefault;
            var onUpdateRuleAttr = new OnDeleteRuleAttribute(rule);

            Assert.AreEqual(rule, onUpdateRuleAttr.Rule);
        }
    }
}
