using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal class RuleTests
    {
        [Test]
        public void Ctor_GivenNullTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new FakeRule(null, level));
        }

        [Test]
        public void Ctor_GivenEmptyTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new FakeRule(string.Empty, level));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new FakeRule("   ", level));
        }

        [Test]
        public void Ctor_GivenInvalidRuleLevel_ThrowsArgumentException()
        {
            const string title = "test";
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new FakeRule(title, level));
        }

        [Test]
        public void Title_PropertyGet_MatchesCtorArg()
        {
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(title, level);

            Assert.AreEqual(title, rule.Title);
        }

        [Test]
        public void Level_PropertyGet_MatchesCtorArg()
        {
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(title, level);

            Assert.AreEqual(level, rule.Level);
        }

        protected class FakeRule : Rule
        {
            public FakeRule(string title, RuleLevel level)
                : base(title, level)
            {
            }

            public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database) => Array.Empty<IRuleMessage>();
        }
    }
}
