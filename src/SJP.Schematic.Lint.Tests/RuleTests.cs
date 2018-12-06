using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class RuleTests
    {
        [Test]
        public static void Ctor_GivenNullTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new FakeRule(null, level));
        }

        [Test]
        public static void Ctor_GivenEmptyTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new FakeRule(string.Empty, level));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new FakeRule("   ", level));
        }

        [Test]
        public static void Ctor_GivenInvalidRuleLevel_ThrowsArgumentException()
        {
            const string title = "test";
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new FakeRule(title, level));
        }

        [Test]
        public static void Title_PropertyGet_MatchesCtorArg()
        {
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(title, level);

            Assert.AreEqual(title, rule.Title);
        }

        [Test]
        public static void Level_PropertyGet_MatchesCtorArg()
        {
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(title, level);

            Assert.AreEqual(level, rule.Level);
        }

        private sealed class FakeRule : Rule
        {
            public FakeRule(string title, RuleLevel level)
                : base(title, level)
            {
            }

            public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database) => Array.Empty<IRuleMessage>();

            public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
                => Task.FromResult<IEnumerable<IRuleMessage>>(Array.Empty<IRuleMessage>());
        }
    }
}
